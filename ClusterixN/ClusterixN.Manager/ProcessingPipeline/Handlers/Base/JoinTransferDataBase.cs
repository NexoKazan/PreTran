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
using System.Linq;
using System.Threading.Tasks;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Managers;
using Relation = ClusterixN.Common.Data.Query.Relation.Relation;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers.Base
{
    internal abstract class JoinTransferDataBase : HandlerBase
    {
        protected readonly TaskSequenceHelper TaskSequenceHelper;

        public JoinTransferDataBase(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            TaskSequenceHelper = new TaskSequenceHelper();
        }

        protected void TransferJoinRelation(Node node, Relation joinRelation,
            JoinQuery joinQuery, Query fullQuery)
        {
            var queryBuffers = QueryBufferManager.GetQueryBuffer(joinRelation.RelationId)
                .Where(b => b.IsPreparedToTransfer == false)
                .ToList();

            for (var i = 0; i < queryBuffers.Count; i++)
            {
                var queryBuffer = queryBuffers[i];
                queryBuffer.IsPreparedToTransfer = true;
                var sendData = new RelationDataPacket
                {
                    Id = new Identify() { ClientId = node.Id },
                    Data = queryBuffer.Data,
                    RelationId = joinRelation.RelationId,
                    QueryNumber = QueryManager.GetQueryByJoinQueryId(joinQuery.QueryId).Number,
                    QueryId = fullQuery.Id,
                    SubQueryId = joinQuery.QueryId,
                    IsLast = queryBuffer.IsLast,
                    OrderNumber = queryBuffer.OrderNumber
                };

                QueryManager.SetJoinRelationStatus(joinRelation.RelationId, QueryRelationStatus.TransferData);
                TaskSequenceHelper.AddTask(new Task(obj =>
                    {
                        var tup = (Tuple<RelationDataPacket, Relation, QueryBuffer>) obj;
                        Server.Send(tup.Item1);
                        if (tup.Item1.IsLast)
                        {
                            QueryManager.SetJoinRelationStatus(tup.Item2.RelationId, QueryRelationStatus.Transfered);
                            QueryBufferManager.RemoveData(tup.Item3.QueryId);
                        }
                        else
                        {
                            QueryManager.SetJoinRelationStatus(joinRelation.RelationId, QueryRelationStatus.TransferData);
                            QueryBufferManager.RemoveBlock(tup.Item3);
                        }
                    },
                    new Tuple<RelationDataPacket, Relation, QueryBuffer>(sendData, joinRelation, queryBuffer)));
            }
        }
    }
}