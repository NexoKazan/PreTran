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
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers.Base
{
    internal abstract class JoinStartBase : HandlerBase
    {
        public JoinStartBase(IServerCommunicator server, IQueryManager queryManager, NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueryBufferManager queryBufferManager, QueueManager queueManager) : base(server, queryManager,
            nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
        }

        protected void StartJoinProcessing(Node node, bool hardOnly)
        {
            var queries = hardOnly
                ? node.QueriesInProgress.Where(
                    q => q.IsHard)
                : node.QueriesInProgress;

            foreach (var query in queries.Where(
                q =>
                    q.JoinQueries.Any(
                        j =>
                            j.LeftRelation.Status == QueryRelationStatus.Transfered &&
                            j.RightRelation.Status == QueryRelationStatus.Transfered)))

            {
                if (!node.CanSendQuery) break;

                var joinQuery = query.JoinQueries.First(j =>
                    j.LeftRelation.Status == QueryRelationStatus.Transfered &&
                    j.RightRelation.Status == QueryRelationStatus.Transfered);

                node.QueryInProgress++;
                QueryManager.SetJoinRelationStatus(joinQuery.LeftRelation.RelationId, QueryRelationStatus.Processing);
                QueryManager.SetJoinRelationStatus(joinQuery.RightRelation.RelationId, QueryRelationStatus.Processing);
                QueryManager.SetSubQueryStatus(joinQuery.QueryId, QueryStatus.JoinProcessing);

                Server.Send(new JoinStartPacket()
                {
                    Id = new Identify() { ClientId = node.Id },
                    RelationRight = joinQuery.RightRelation.RelationId,
                    RelationLeft = joinQuery.LeftRelation.RelationId,
                    RelationId = joinQuery.QueryId,
                    Query = joinQuery.Query,
                    ResultSchema = joinQuery.ResultSchema.ToPacketRelationSchema(),
                    QueryNumber = query.Number,
                    QueryId = query.Id,
                    SubQueryId = joinQuery.QueryId
                });
            }
        }
    }
}