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
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
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
    internal class SortPrepare : HandlerBase
    {
        public SortPrepare(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) :
            base(server, queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<RelationPreparedPacket>(RelationPreparedPacketHandler);
        }

        private void StartSort()
        {
            var nodes = NodesManager.GetFreeNodes(NodeType.Sort);

            if (nodes.Count == 0)
            {
                var busyNodes = NodesManager.GetNodes(NodeType.Sort);
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

            foreach (var node in nodes)
                ProcessSortNode(node);
        }

        private void ProcessSortNode(Node node)
        {
            SendNewQueryToSortNode(node);
        }

        private void SendNewQueryToSortNode(Node node)
        {
            var query = QueryManager.GetQueryToSort();
            if (query != null)
            {
                if (query.SortQuery == null)
                {
                    QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.SortComplete);
                    return;
                }

                var sortQuery = query.SortQuery;

                node.QueriesInProgress.Add(query);
                QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.Sort);
                QueryManager.SetSubQueryStatus(sortQuery.QueryId, QueryStatus.TransferToSort);

                foreach (var relation in sortQuery.SortRelation)
                {
                    node.QueryInProgress++;
                    QueryManager.SetSortRelationStatus(relation.RelationId, QueryRelationStatus.Preparing);
                    Server.Send(new RelationPreparePacket()
                    {
                        Id = new Identify() { ClientId = node.Id },
                        RelationShema = relation.Shema.ToPacketRelationSchema(),
                        RelationId = relation.RelationId,
                        QueryId = query.Id,
                        QueryNumber = query.Number,
                        RelationName = relation.Name,
                        IsEmptyRelation = relation.IsEmpty
                    });
                }
            }
        }

        #region Packet Handlers

        private void RelationPreparedPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as RelationPreparedPacket;
            if (packet != null)
                QueueManager.Add(() => SortRelationPrepared(packet));
        }

        private void SortRelationPrepared(RelationPreparedPacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Sort)
            {
                node.QueryInProgress--;
                QueryManager.SetSortRelationStatus(packet.RelationId, QueryRelationStatus.Prepared);
            }
        }

        #endregion

        public override void DoAction()
        {
            StartSort();
        }
    }
}