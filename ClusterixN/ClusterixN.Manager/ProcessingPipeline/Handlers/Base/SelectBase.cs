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

﻿using System.Collections.Generic;
using System.Linq;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers.Base
{
    internal abstract class SelectBase : HandlerBase
    {
        protected readonly MultiNodeProcessingManager MultiNodeProcessingManager;

        protected SelectBase(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            MultiNodeProcessingManager = new MultiNodeProcessingManager();
            server.SubscribeToPacket<SelectResult>(SelectResultPacketHandler);
        }
        
        protected List<Query> GetNodesSharedExistQuery(List<Node> nodes)
        {
            var node = nodes.FirstOrDefault();
            var exeptNodes = new List<Node> {node};
            var anotherNodes = nodes.Except(exeptNodes).ToList();
            var queries = new List<Query>();
            if (node == null) return queries;

            foreach (var query in node.QueriesInProgress)
            {
                var result = true;
                foreach (var anotherNode in anotherNodes)
                    if (anotherNode.QueriesInProgress.All(q => q.Id != query.Id))
                    {
                        result = false;
                        break;
                    }

                if (result)
                    queries.Add(query);
            }
            return queries;
        }

        protected bool SendExitsQueryToIoNode(List<Node> nodes, Query existQuery)
        {
            var result = SendSelectQuery(nodes, existQuery);
            return result;
        }

        private bool SendSelectQuery(List<Node> nodes, Query query)
        {
            var result = false;

            var nextQuery = QueryManager.GetNextSelectQuery(query.Id);
            if (nextQuery != null)
            {
                foreach (var node in nodes)
                {
                    var selectQuery = nextQuery;

                    node.QueryInProgress++;
                    QueryManager.SetSubQueryStatus(selectQuery.QueryId, QueryStatus.SelectProcessing);
                    MultiNodeProcessingManager.AddTask(node.Id, selectQuery.QueryId);

                    SendPacket(query, node, selectQuery);
                }

                result = true;
            }
            return result;
        }

        protected virtual void SendPacket(Query query, Node node, SelectQuery selectQuery)
        {
            Server.Send(new QueryPacket(query.Number)
            {
                Id = new Identify() {ClientId = node.Id},
                SubQueryId = selectQuery.QueryId,
                QueryId = query.Id,
                Query = selectQuery.Query
            });
        }

        protected void SendNewQueryToIoNode(List<Node> nodes)
        {
            var query = QueryManager.GetQueryToProcess();
            if (query != null)
            {
                QueryManager.SetQueryStatus(query.Id, QueryProcessStatus.Select);
                foreach (var node in nodes)
                    node.QueriesInProgress.Add(query);
                SendSelectQuery(nodes, query);
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
            if (node != null)
            {
                Logger.Trace(
                    $"Получен результат SELECT для {packet.SubQueryId}, IsLast = {packet.IsLast}, OrderNumber = {packet.OrderNumber}");
                if (node.NodeType == NodeType.Io)
                {
                    var ioNodes = NodesManager.GetNodes(NodeType.Io);
                    QueryBufferManager.AddData(new QueryBuffer
                    {
                        QueryId = packet.SubQueryId,
                        IsLast = false,
                        Data = packet.Result,
                        OrderNumber = packet.OrderNumber * ioNodes.Count +
                                      ioNodes.IndexOf(node) % ioNodes.Count
                    });

                    QueryManager.SetSubQueryStatus(packet.SubQueryId, QueryStatus.TransferSelectResult);
                    if (packet.IsLast)
                    {
                        node.QueryInProgress--;
                        MultiNodeProcessingManager.SetComplete(packet.Id.ClientId,packet.SubQueryId);
                        if (MultiNodeProcessingManager.Check(packet.SubQueryId))
                        {
                            QueryManager.SetSubQueryStatus(packet.SubQueryId,QueryStatus.SelectProcessed);
                            QueryBufferManager.MarkLastPacket(packet.SubQueryId);
                        }
                    }
                }
            }
        }
    }
}