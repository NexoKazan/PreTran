#region Copyright
/*
 * Copyright 2017 Roman Klassen
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */
#endregion

﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Handlers.Base;
using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class JoinPrepare : HandlerBase
    {
        private readonly int _concurentQueriesCount;

        public JoinPrepare(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : 
            base(server, queryManager, nodesManager, pauseLogManager, queryBufferManager,queueManager)
        {
            server.SubscribeToPacket<RelationPreparedPacket>(RelationPreparedPacketHandler);
            _concurentQueriesCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("JoinQueriesCount"));
        }

        private void StartJoin()
        {
            var nodes = NodesManager.GetFreeNodes(NodeType.Join);

            if (nodes.Count == 0)
            {
                var busyNodes = NodesManager.GetNodes(NodeType.Join);
                foreach (var busyNode in busyNodes)
                {
                    if ((busyNode.Status & (uint)NodeStatus.Full) > 0) continue;
                    if ((busyNode.Status & (uint)NodeStatus.LowMemory) > 0)
                        PauseLogManager.PauseNode(busyNode.Id, Guid.Empty);
                }
                return;
            }

            foreach (var node in nodes)
                PauseLogManager.ResumeNode(node.Id, Guid.Empty);

            RoundRobinJoinNodeProcess(nodes);
        }

        private void RoundRobinJoinNodeProcess(List<Node> nodes)
        {
            foreach (var node in nodes.OrderBy(n => n.LastSendTime))
                if (ProcessJoinNode(node))
                    node.LastSendTime = SystemTime.Now;
        }

        private bool ProcessJoinNode(Node node)
        {
            var sended = false;
            var existQuery = node.QueriesInProgress.FirstOrDefault();

            if (existQuery != null)
            {
                foreach (var query in node.QueriesInProgress)
                    if (node.CanSendQuery)
                        sended |= SendExitsQueryToJoinNode(node, query);

                if (!sended)
                    sended = SendNewQueryToJoinNode(node);
            }
            else
            {
                sended = SendNewQueryToJoinNode(node);
            }

            return sended;
        }

        private bool SendExitsQueryToJoinNode(Node node, Query existQuery)
        {
            var result = false;
            var nextQuery = QueryManager.GetNextJoinQuery(existQuery.Id);
            if (nextQuery != null && (nextQuery.LeftRelation.Status == QueryRelationStatus.Wait ||
                                      nextQuery.RightRelation.Status == QueryRelationStatus.Wait))
            {
                var joinQuery = nextQuery;
                var joinRelation = joinQuery.LeftRelation.Status == QueryRelationStatus.Wait
                    ? joinQuery.LeftRelation
                    : joinQuery.RightRelation;

                if (!QueryManager.CheckJoinQueryReady(joinQuery)) return false;

                node.QueryInProgress++;
                QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.TransferToJoin);
                QueryManager.SetJoinRelationStatus(joinRelation.RelationId, QueryRelationStatus.Preparing);

                Server.Send(new RelationPreparePacket()
                {
                    Id = new Identify() { ClientId = node.Id },
                    RelationShema = joinRelation.Shema.ToPacketRelationSchema(),
                    RelationId = joinRelation.RelationId,
                    QueryId = existQuery.Id,
                    QueryNumber = existQuery.Number,
                    RelationName = joinRelation.Name,
                    IsEmptyRelation = joinRelation.IsEmpty
                });

                result = true;
            }

            return result;
        }

        private bool SendNewQueryToJoinNode(Node node)
        {
            if (_concurentQueriesCount > 0)
                if (node.QueriesInProgress.Count >= _concurentQueriesCount)
                    return false;

            var query = QueryManager.GetQueryToJoin();
            if (query == null) return false;
            if (query.JoinQueries.Count == 0)
            {
                QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.JoinComplete);
                return false;
            }

            var joinQuery = query.JoinQueries.First();
            if (!QueryManager.CheckJoinQueryReady(joinQuery) || !node.CanSendData(query.DataAmount)) return false;

            node.QueryInProgress++;
            node.QueriesInProgress.Add(query);
            QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.Join);
            QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.TransferToJoin);
            QueryManager.SetJoinRelationStatus(joinQuery.LeftRelation.RelationId, QueryRelationStatus.Preparing);

            Server.Send(new RelationPreparePacket()
            {
                Id = new Identify() { ClientId = node.Id },
                RelationShema = joinQuery.LeftRelation.Shema.ToPacketRelationSchema(),
                RelationId = joinQuery.LeftRelation.RelationId,
                QueryId = query.Id,
                QueryNumber = query.Number,
                RelationName = joinQuery.LeftRelation.Name,
                IsEmptyRelation = joinQuery.LeftRelation.IsEmpty
            });
            return true;
        }

        #region Packet Handlers

        private void RelationPreparedPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as RelationPreparedPacket;
            if (packet != null)
                QueueManager.Add(() => JoinRelationPrepared(packet));
        }

        private void JoinRelationPrepared(RelationPreparedPacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Join)
            {
                node.QueryInProgress--;
                QueryManager.SetJoinRelationStatus(packet.RelationId, QueryRelationStatus.Prepared);

                if (QueryManager.GetRelationById(packet.RelationId).IsEmpty)
                    QueryManager.SetJoinRelationStatus(packet.RelationId, QueryRelationStatus.Transfered);
            }
        }

        #endregion

        public override void DoAction()
        {
            StartJoin();
        }
    }
}