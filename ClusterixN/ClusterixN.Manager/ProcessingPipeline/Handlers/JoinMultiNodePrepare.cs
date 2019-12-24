#region Copyright
/*
 * Copyright 2018 Roman Klassen
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
using ClusterixN.Common.Data.Query.Relation;
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
    internal class JoinMultiNodePrepare : HandlerBase
    {
        private readonly int _concurentQueriesCount;
        private readonly MultiNodeProcessingManager _multiNodeProcessingManager;

        public JoinMultiNodePrepare(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : 
            base(server, queryManager, nodesManager, pauseLogManager, queryBufferManager,queueManager)
        {
            server.SubscribeToPacket<RelationPreparedPacket>(RelationPreparedPacketHandler);
            _concurentQueriesCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("JoinQueriesCount"));
            _multiNodeProcessingManager = new MultiNodeProcessingManager();
        }

        private void StartJoin()
        {
            var nodes = NodesManager.GetNodes(NodeType.Join);

            if (nodes.Count == 0)
            {
                foreach (var busyNode in nodes)
                {
                    if ((busyNode.Status & (uint)NodeStatus.Full) > 0) continue;
                    if ((busyNode.Status & (uint)NodeStatus.LowMemory) > 0)
                        PauseLogManager.PauseNode(busyNode.Id, Guid.Empty);
                }
                return;
            }

            foreach (var node in nodes)
                PauseLogManager.ResumeNode(node.Id, Guid.Empty);

            ProcessJoinNodes(nodes);
        }

        private void ProcessJoinNodes(List<Node> nodes)
        {
            if (SendNewQueryToJoinNode(nodes))
            {
                var existQuery = nodes.FirstOrDefault()?.QueriesInProgress.FirstOrDefault();
                if (existQuery != null)
                {
                    var node = nodes.First();
                    foreach (var query in node.QueriesInProgress)
                        SendExitsQueryToJoinNode(nodes, query);
                }
            }
        }

        private void SendExitsQueryToJoinNode(List<Node> nodes, Query existQuery)
        {
            var joinQuery = QueryManager.GetNextJoinQuery(existQuery.Id);
            while (joinQuery != null)
            {
                var joinRelations = new List<Relation>() {joinQuery.LeftRelation, joinQuery.RightRelation};

                foreach (var joinRelation in joinRelations)
                {
                    if (joinRelation.Status != QueryRelationStatus.Wait &&
                        joinRelation.Status != QueryRelationStatus.WaitAnotherRelation) continue;

                    QueryManager.SetJoinRelationStatus(joinRelation.RelationId, QueryRelationStatus.Preparing);

                    foreach (var node in nodes)
                    {
                        node.QueryInProgress++;

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

                        _multiNodeProcessingManager.AddTask(node.Id, joinRelation.RelationId);
                    }
                }

                joinQuery = QueryManager.GetNextJoinQuery(existQuery.Id);
            }
        }

        private bool SendNewQueryToJoinNode(List<Node> nodes)
        {
            if (_concurentQueriesCount > 0)
                if (nodes.FirstOrDefault()?.QueriesInProgress.Count >= _concurentQueriesCount)
                    return false;

            var query = QueryManager.GetQueryToJoin();
            if (query == null) return false;
            if (query.JoinQueries.Count == 0)
            {
                QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.JoinComplete);
                return false;
            }

            var joinQuery = query.JoinQueries.First();
            
            QueryManager.SetJoinRelationStatus(joinQuery.LeftRelation.RelationId, QueryRelationStatus.Preparing);

            foreach (var node in nodes)
            {
                node.QueryInProgress++;
                node.QueriesInProgress.Add(query);

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

                _multiNodeProcessingManager.AddTask(node.Id, joinQuery.LeftRelation.RelationId);
            }
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
                _multiNodeProcessingManager.SetComplete(node.Id, packet.RelationId);

                if (_multiNodeProcessingManager.Check(packet.RelationId))
                {
                    QueryManager.SetJoinRelationStatus(packet.RelationId, QueryRelationStatus.Prepared);
                    _multiNodeProcessingManager.Remove(packet.RelationId);

                    if (QueryManager.GetRelationById(packet.RelationId).IsEmpty)
                        QueryManager.SetJoinRelationStatus(packet.RelationId, QueryRelationStatus.Transfered);
                }
            }
        }

        #endregion

        public override void DoAction()
        {
            StartJoin();
        }
    }
}