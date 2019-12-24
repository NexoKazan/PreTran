#region Copyright
/*
 * Copyright 2019 Roman Klassen
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Task;
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
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SelectAndSendToJoinAndWait : SelectBase
    {
        private readonly TaskSequenceHelper _sequenceHelper;
        private readonly int _concurentQueriesCount;

        public SelectAndSendToJoinAndWait(IServerCommunicator server, IQueryManager queryManager,
            QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, queryBufferManager, nodesManager, pauseLogManager, queueManager)
        {
            _concurentQueriesCount =
                int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("SelectQueriesCount"));
            _sequenceHelper = new TaskSequenceHelper();
            server.SubscribeToPacket<QueryStatusPacket>(QueryStatusPacketHandler);
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
            if (node?.NodeType == NodeType.Io)
            {
                Logger.Trace(
                    $"Получен новый статус для {packet.SubQueryId} = {packet.Status}");

                QueryManager.SetSubQueryStatus(packet.SubQueryId, QueryStatus.TransferSelectResult);
                if (packet.Status == QueryStatus.SelectProcessed)
                {
                    node.QueryInProgress--;
                    MultiNodeProcessingManager.SetComplete(packet.Id.ClientId, packet.SubQueryId);
                    if (MultiNodeProcessingManager.Check(packet.SubQueryId))
                    {
                        QueryManager.SetSubQueryStatus(packet.SubQueryId, QueryStatus.SelectProcessed);
                        QueryManager.SetJoinRelationStatus(packet.RelationId, QueryRelationStatus.Transfered);

                        var query = QueryManager.GetQueryBySelectQueryId(packet.SubQueryId);
                        if (query.JoinQueries.Count == 0)
                            QueryManager.SetSubQueryStatus(query.SortQuery.QueryId, QueryStatus.TransferedToSort);
                    }
                }
            }
        }

        private void StartSelect()
        {
            var nodes = NodesManager.GetNodes(NodeType.Io);

            if (nodes.Count == 0 || nodes.Any(n => !n.CanSendQuery))
            {
                foreach (var node in nodes)
                {
                    if ((node.Status & (uint) NodeStatus.Full) > 0) continue;
                    if ((node.Status & (uint) NodeStatus.LowMemory) > 0)
                        PauseLogManager.PauseNode(node.Id, Guid.Empty);
                }

                return;
            }

            foreach (var node in nodes)
                PauseLogManager.ResumeNode(node.Id, Guid.Empty);

            var joinNodes = NodesManager.GetNodes(NodeType.Join);

            if (joinNodes.All(n => n.QueryInProgress == 0))
            {
                ProcessIoNodes(nodes);
            }
        }

        private void ProcessIoNodes(List<Node> nodes)
        {
            var existQuery = GetNodesSharedExistQuery(nodes);
            var trySendNewQuery = false;

            if (existQuery.Count > 0)
            {
                var sended = false;
                foreach (var query in existQuery)
                {
                    if (!nodes.All(n => n.CanSendQuery) || nodes.Any(n => n.QueryInProgress >= _concurentQueriesCount)) break;
                    sended |= SendExitsQueryToIoNode(nodes, query);
                }

                if (!sended && nodes.All(n => n.CanSendQuery) &&
                    nodes.All(n => n.QueriesInProgress.Count < _concurentQueriesCount))
                    trySendNewQuery = true;
            }
            else
            {
                trySendNewQuery = true;
            }

            if (trySendNewQuery)
            {
                var joinNodes = NodesManager.GetNodes(NodeType.Join);

                if (joinNodes.All(n => n.QueriesInProgress.Count == 0))
                    SendNewQueryToIoNode(nodes);
            }
        }

        protected override void SendPacket(Query query, Node node, SelectQuery selectQuery)
        {
            _sequenceHelper.AddTask(new Task(obj =>
            {
                var tup = (Tuple<Query, Node, SelectQuery>) obj;
                  SendPacketAsync(tup.Item1, tup.Item2, tup.Item3);
            }, new Tuple<Query, Node, SelectQuery>(query, node, selectQuery)));
        }

        private void SendPacketAsync(Query query, Node node, SelectQuery selectQuery)
        {
            var sendNodes = query.JoinQueries.Count == 0
                ? NodesManager.GetNodes(NodeType.Sort)
                : NodesManager.GetNodes(NodeType.Join);
            var hashCount = query.JoinQueries.Count == 0
                ? sendNodes.Select(n => 1).ToArray()
                : sendNodes.Select(n => n.CpuCount).ToArray();

            WaitJoin(query);
            WaitSort(query);

            Server.Send(new SelectAndSendPacket(query.Number)
            {
                Id = new Identify {ClientId = node.Id},
                SubQueryId = selectQuery.QueryId,
                RelationId = QueryManager.GetRelationById(selectQuery.QueryId).RelationId,
                QueryId = query.Id,
                Query = selectQuery.Query,
                JoinCount = sendNodes.Count,
                JoinAddresses = GetNodeAddresses(sendNodes),
                HashCount = hashCount,
                RelationShema = QueryManager.GetRelationById(selectQuery.QueryId).Shema.ToPacketRelationSchema(),
                IoCount = NodesManager.GetNodes(NodeType.Io).Count,
                IoNumber = NodesManager.GetNodes(NodeType.Io).FindIndex(n=>n.Id == node.Id) + 1
            });
        }

        void WaitSort(Query query)
        {
            if (query.SortQuery == null) return;
            while (query.SortQuery.Status == QueryStatus.Wait)
            {
                Thread.Sleep(10);
            }
        }

        void WaitJoin(Query query)
        {
            if (query.JoinQueries.Count == 0) return;
            while (!query.JoinQueries.All(j =>
                j.LeftRelation.Status > QueryRelationStatus.Preparing &&
                j.RightRelation.Status > QueryRelationStatus.Preparing))
            {
                Thread.Sleep(10);
            }
        }

        public override void DoAction()
        {
            StartSelect();
        }
    }
}