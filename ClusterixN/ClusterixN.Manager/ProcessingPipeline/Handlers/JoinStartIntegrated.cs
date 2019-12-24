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

﻿using System.Linq;
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
    internal class JoinStartIntegrated : JoinStartBase
    {
        public JoinStartIntegrated(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<IntegratedJoinCompletePacket>(IntegratedJoinCompletedEventHandler);
        }

        private void StartJoin()
        {
            foreach (var node in NodesManager.GetNodes(NodeType.Join)
                .Where(n => n.QueriesInProgress.Count > 0 && n.CanTransferDataQuery))
            {
                StartIntegratedJoinProcessing(node);
            }
        }
        
        private void StartIntegratedJoinProcessing(Node node)
        {
            foreach (var query in node.QueriesInProgress.Where(
                q => !q.IsHard &&
                     q.JoinQueries.All(
                         j =>
                             (j.LeftRelation.Status == QueryRelationStatus.Transfered || j.LeftRelation.Status == QueryRelationStatus.WaitAnotherRelation) &&
                             (j.RightRelation.Status == QueryRelationStatus.Transfered || j.RightRelation.Status == QueryRelationStatus.WaitAnotherRelation))))

            {
                if (!node.CanSendQuery) break;

                foreach (var joinQuery in query.JoinQueries)
                {
                    QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.JoinProcessing);
                    QueryManager.SetJoinRelationStatus(joinQuery.LeftRelation.RelationId, QueryRelationStatus.Processing);
                    QueryManager.SetJoinRelationStatus(joinQuery.RightRelation.RelationId, QueryRelationStatus.Processing);
                }
                node.QueryInProgress++;

                var jointree = query.GetJoinTree();
                foreach (var joinTreeLeaf in jointree)
                {
                    var join = query.JoinQueries.First(j => j.QueryId == joinTreeLeaf.RelationId);
                    Server.Send(new IntegratedJoinStartPacket()
                    {
                        Id = new Identify() { ClientId = node.Id },
                        JoinTree = joinTreeLeaf.ToPacketJoinLeaf(),
                        ResultSchema = join.ResultSchema.ToPacketRelationSchema(),
                        QueryNumber = query.Number,
                        QueryId = query.Id,
                        SubQueryId = join.QueryId
                    });
                }
            }
            if (node.QueriesInProgress.Any(q => q.IsHard))
            {
                StartJoinProcessing(node, true);
            }
        }

        private void IntegratedJoinCompletedEventHandler(PacketBase packetBase)
        {
            var packet = packetBase as IntegratedJoinCompletePacket;
            if (packet != null)
                QueueManager.Add(() => IntegratedJoinCompleted(packet));
        }

        private void IntegratedJoinCompleted(IntegratedJoinCompletePacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Join)
            {
                node.QueryInProgress--;

                foreach (var relation in packet.Relations)
                {
                    QueryManager.SetJoinRelationStatus(relation, QueryRelationStatus.Processed);
                    node.PendingRam -= QueryManager.GetRelationById(relation)?.DataAmount ?? 0;
                }

                QueryManager.SetJoinRelationStatus(packet.NewRelationId, QueryRelationStatus.Transfered);
            }
        }

        public override void DoAction()
        {
            StartJoin();
        }
    }
}