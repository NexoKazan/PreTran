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

using System.Collections.Generic;
using System.Linq;
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
    internal class JoinStartSequencedMultiNode : HandlerBase
    {
        private readonly MultiNodeProcessingManager _multiNodeProcessingManager;
        private readonly MultiNodeProcessingManager _multiNodeSatusProcessingManager;
        public JoinStartSequencedMultiNode(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<JoinCompletePacket>(JoinCompletePacketHandler);
            server.SubscribeToPacket<QueryStatusPacket>(QueryStatusPacketHandler);
            _multiNodeProcessingManager = new MultiNodeProcessingManager();
            _multiNodeSatusProcessingManager = new MultiNodeProcessingManager();
        }

        private void StartJoin()
        {
            StartJoinProcessing(NodesManager.GetNodes(NodeType.Join));
        }

        private void StartJoinProcessing(List<Node> nodes)
        {
            var queries = nodes.FirstOrDefault()?.QueriesInProgress;

            if (queries == null || queries.Count == 0) return;
            if (!nodes.All(n => n.CanSendQuery)) return;
            if (nodes.Sum(n => n.QueryInProgress) >= nodes.Count) return;

            var query = queries.First();

            var joinQuery = query.JoinQueries.FirstOrDefault(j =>
                j.LeftRelation.Status == QueryRelationStatus.Transfered &&
                j.RightRelation.Status == QueryRelationStatus.Transfered);

            if (joinQuery == null) return;

            QueryManager.SetJoinRelationStatus(joinQuery.LeftRelation.RelationId,
                QueryRelationStatus.Processing);
            QueryManager.SetJoinRelationStatus(joinQuery.RightRelation.RelationId,
                QueryRelationStatus.Processing);
            QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.JoinProcessing);
            QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.Join);
            
            foreach (var node in nodes)
            {
                node.QueryInProgress++;
                var isLastJoin = IsLastJoin(query, joinQuery);
                Server.Send(new MultiNodeJoinStartPacket()
                {
                    Id = new Identify() {ClientId = node.Id},
                    RelationRight = joinQuery.RightRelation.RelationId,
                    RelationLeft = joinQuery.LeftRelation.RelationId,
                    Query = joinQuery.Query,
                    ResultSchema = joinQuery.ResultSchema.ToPacketRelationSchema(),
                    QueryNumber = query.Number,
                    QueryId = query.Id,
                    SubQueryId = joinQuery.QueryId,
                    RelationId = joinQuery.QueryId, //для sort relationId = join.queryId
                    NextNodeAddresses = isLastJoin
                        ? GetNodeAddresses(NodesManager.GetNodes(NodeType.Sort))
                        : GetNodeAddresses(nodes),
                    HashCount = nodes.Select(n => n.CpuCount).ToArray(),
                    IsLast = isLastJoin,
                    JoinCount = nodes.Count,
                    JoinNumber = nodes.FindIndex(n=>n == node) + 1
                });
                _multiNodeProcessingManager.AddTask(node.Id, joinQuery.QueryId);
                _multiNodeSatusProcessingManager.AddTask(node.Id, joinQuery.QueryId);
            }
        }

        private bool IsLastJoin(Query query, JoinQuery joinQuery)
        {
            return !query.JoinQueries
                .Any(j => j.LeftRelation.RelationId == joinQuery.QueryId ||
                          j.RightRelation.RelationId == joinQuery.QueryId);
        }

        private void JoinCompletePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as JoinCompletePacket;
            if (packet != null)
            {
                QueueManager.Add(() => JoinCompletePacketReceived(packet));
            }
        }
        
        private void JoinCompletePacketReceived(JoinCompletePacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Join)
            {
                var taskId = packet.SubQueryId;
                node.QueryInProgress--;
                
                _multiNodeProcessingManager.SetComplete(node.Id, taskId);

                if (_multiNodeProcessingManager.Check(taskId))
                {
                    QueryManager.SetJoinRelationStatus(packet.LeftRelationId, QueryRelationStatus.Processed);
                    QueryManager.SetJoinRelationStatus(packet.RightRelationId, QueryRelationStatus.Processed);
                    node.PendingRam -= QueryManager.GetRelationById(packet.LeftRelationId)?.DataAmount ?? 0;
                    node.PendingRam -= QueryManager.GetRelationById(packet.RightRelationId)?.DataAmount ?? 0;

                    QueryManager.SetJoinRelationStatus(packet.NewRelationId, QueryRelationStatus.Transfered);

                    _multiNodeProcessingManager.Remove(taskId);

                    var query = QueryManager.GetQueryByJoinQueryId(packet.SubQueryId);
                    if (query != null &&
                        query.SortQuery.SortRelation.All(s => s.Status == QueryRelationStatus.Transfered))
                    {
                        QueryManager.SetSubQueryStatus(query.SortQuery.QueryId, QueryStatus.TransferedToSort);
                        QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.Sort);
                    }
                }
            }
        }
        private void QueryStatusPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as QueryStatusPacket;
            if (packet != null)
                QueueManager.Add(() => QueryStatusPacketRecieved(packet));
        }

        private void QueryStatusPacketRecieved(QueryStatusPacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node?.NodeType == NodeType.Join)
            {
                Logger.Trace(
                    $"Получен новый статус для {packet.SubQueryId} = {packet.Status} из {node.Id}");

                _multiNodeSatusProcessingManager.SetComplete(node.Id, packet.SubQueryId);

                if (_multiNodeSatusProcessingManager.Check(packet.SubQueryId))
                {
                    Logger.Trace($"Получен новый статус для {packet.SubQueryId} = {packet.Status} из всех узлов");
                    if (packet.Status == QueryStatus.TransferedToSort)
                    {
                        var query = QueryManager.GetQueryByJoinQueryId(packet.SubQueryId);
                        var relation =
                            query?.SortQuery.SortRelation.FirstOrDefault(s => s.Status < QueryRelationStatus.Transfered);
                        if (relation != null)
                        {
                            QueryManager.SetSortRelationStatus(relation.RelationId, QueryRelationStatus.Transfered);
                        }
                    }
                    else if(packet.Status == QueryStatus.JoinProcessing)
                    {
                        QueryManager.SetSubQueryStatus(packet.SubQueryId, packet.Status);
                    }
                    _multiNodeSatusProcessingManager.Remove(packet.SubQueryId);
                }
            }
        }


        public override void DoAction()
        {
            StartJoin();
        }
    }
}