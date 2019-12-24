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
    internal class SortStart : HandlerBase
    {
        public SortStart(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) :
            base(server, queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<SortCompletePacket>(SortCompletePacketHandler);
        }
        private void StartSort()
        {
            foreach (var node in NodesManager.GetNodes(NodeType.Sort)
                .Where(n => n.QueriesInProgress.Count > 0 && n.CanTransferDataQuery))
            {
                StartSortProcessing(node);
            }
        }

        private void StartSortProcessing(Node node)
        {
            foreach (var query in node.QueriesInProgress.Where(
                q =>
                    q.SortQuery.Status == QueryStatus.TransferedToSort))
            {
                if (!node.CanSendQuery) break;

                node.QueryInProgress++;
                QueryManager.SetSubQueryStatus(query.SortQuery.QueryId, QueryStatus.ProcessingSort);

                Server.Send(new SortStartPacket()
                {
                    Id = new Identify() { ClientId = node.Id },
                    QueryNumber = query.Number,
                    Query = query.SortQuery.Query,
                    ResultSchema = query.SortQuery.ResultSchema.ToPacketRelationSchema(),
                    RelationId = query.SortQuery.QueryId,
                    RelationIds = query.SortQuery.SortRelation.Select(s => s.RelationId).ToArray(),
                    QueryId = query.Id,
                    SubQueryId = query.SortQuery.QueryId,
                });
            }
        }

        #region Packet Handlers

        private void SortCompletePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as SortCompletePacket;
            if (packet != null)
                QueueManager.Add(() => SortCompleted(packet));
        }

        private void SortCompleted(SortCompletePacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            node.QueryInProgress--;

            QueryManager.SetSubQueryStatus(packet.NewRelationId, QueryStatus.SortProcessed);
        }

        #endregion

        public override void DoAction()
        {
            StartSort();
        }
    }
}