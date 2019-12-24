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
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class JoinTransferResult : HandlerBase
    {
        public JoinTransferResult(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<SelectResult>(SelectResultPacketHandler);
        }
        
        void TransferJoinResult()
        {
            var nodes =
                NodesManager.GetNodes(NodeType.Join)
                    .Where(
                        n =>
                            n.QueriesInProgress.Any(
                                q =>
                                    q.JoinQueries.Any(
                                        j =>
                                            j.Status == QueryStatus.JoinProcessed)))
                    .ToList();

            if (nodes.Count == 0) return;

            foreach (var node in nodes)
            {
                TransferFromJoin(node);
            }
        }

        private void TransferFromJoin(Node node)
        {
            foreach (var query in node.QueriesInProgress)
            {
                foreach (var jQuery in query.JoinQueries)
                {
                    if (jQuery.Status != QueryStatus.JoinProcessed) continue;

                    var joinQuery = jQuery;
                    var fullQuery = query;

                    if (string.IsNullOrWhiteSpace(joinQuery.ResultSelectQuery))
                    {
                        QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.JoinResultTransfered);
                    }
                    else
                    {
                        if (!node.CanSendQuery) break;

                        QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.TransferJoinResult);

                        node.QueryInProgress++;
                        Server.Send(new QueryPacket(fullQuery.Number)
                        {
                            Id = new Identify() { ClientId = node.Id },
                            RelationId = joinQuery.QueryId,
                            SubQueryId = joinQuery.QueryId,
                            QueryId = fullQuery.Id,
                            Query = joinQuery.ResultSelectQuery
                        });
                    }
                }
            }
        }

        private void SelectResultPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as SelectResult;
            if (packet != null)
                QueueManager.Add(() => SelectResultRecieved(packet));
        }
        
        private void SelectResultRecieved(SelectResult packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Join)
            {
                Logger.Trace(
                    $"Получен результат SELECT для {packet.SubQueryId}, IsLast = {packet.IsLast}, OrderNumber = {packet.OrderNumber}");

                QueryBufferManager.AddData(new QueryBuffer()
                {
                    QueryId = packet.SubQueryId,
                    IsLast = packet.IsLast,
                    Data = packet.Result,
                    OrderNumber = packet.OrderNumber
                });

                if (QueryBufferManager.CheckReady(packet.SubQueryId))
                {
                    node.QueryInProgress--;
                    QueryManager.SetSubQueryStatus(packet.SubQueryId, QueryStatus.JoinResultTransfered);
                }
            }
        }

        public override void DoAction()
        {
            TransferJoinResult();
        }
    }
}