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
using ClusterixN.Common.Data.Query;
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
    internal class SortTransferResult : HandlerBase
    {
        public SortTransferResult(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) :
            base(server, queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<SelectResult>(SelectResultPacketHandler);
        }

        void TransferSortResult()
        {
            var nodes =
                NodesManager.GetNodes(NodeType.Sort)
                    .Where(
                        n =>
                            n.QueriesInProgress.Any(
                                q =>
                                    q.SortQuery.Status == QueryStatus.SortProcessed))
                    .ToList();

            if (nodes.Count == 0) return;

            foreach (var node in nodes)
            {
                TransferFromSort(node);
            }
        }

        private void TransferFromSort(Node node)
        {
            SortQuery sortQuery = null;
            Query fullQuery = null;
            foreach (var query in node.QueriesInProgress)
                if (query.SortQuery.Status == QueryStatus.SortProcessed)
                {
                    sortQuery = query.SortQuery;
                    fullQuery = query;
                    break;
                }

            if (sortQuery != null)
            {
                QueryManager.SetSubQueryStatus(sortQuery.QueryId, QueryStatus.TransferSortResult);

                node.QueryInProgress++;
                Server.Send(new QueryPacket(fullQuery.Number)
                {
                    Id = new Identify() { ClientId = node.Id },
                    RelationId = sortQuery.QueryId,
                    SubQueryId = sortQuery.QueryId,
                    QueryId = fullQuery.Id,
                    Query = sortQuery.ResultSelectQuery
                });
            }
        }

        #region Packet Handlers

        private void SelectResultPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as SelectResult;
            if (packet != null)
                QueueManager.Add(() => SelectResultRecieved(packet));
        }

        private void SelectResultRecieved(SelectResult packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Sort)
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
                    QueryManager.SetSubQueryStatus(packet.SubQueryId, QueryStatus.SortResultTransfered);
                }
            }
        }
        
        #endregion

        public override void DoAction()
        {
            TransferSortResult();
        }
    }
}