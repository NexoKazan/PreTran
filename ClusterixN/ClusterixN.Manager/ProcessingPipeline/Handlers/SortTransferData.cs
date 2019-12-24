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
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Handlers.Base;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class SortTransferData : HandlerBase
    {
        private readonly TaskSequenceHelper _taskSequenceHelper;

        public SortTransferData(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) :
            base(server, queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            _taskSequenceHelper = new TaskSequenceHelper();
        }
       
        private void TransferToSort()
        {
            var nodes =
                NodesManager.GetNodes(NodeType.Sort)
                    .Where(
                        n => n.CanTransferDataQuery &&
                             n.QueriesInProgress.Any(
                                 q =>
                                     q.SortQuery.SortRelation.Any(
                                         s =>
                                             s.Status == QueryRelationStatus.Prepared)))
                    .ToList();

            if (nodes.Count == 0) return;

            foreach (var node in nodes)
                TransferToSort(node);
        }

        private void TransferToSort(Node node)
        {
            Relation sortRelation = null;
            SortQuery sortQuery = null;
            Query fullQuery = null;
            foreach (var query in node.QueriesInProgress)
            foreach (var relation in query.SortQuery.SortRelation)
                if (relation.Status == QueryRelationStatus.Prepared)
                {
                    sortRelation = relation;
                    sortQuery = query.SortQuery;
                    fullQuery = query;
                    break;
                }

            if (sortRelation != null)
            {
                QueryManager.SetSortRelationStatus(sortRelation.RelationId, QueryRelationStatus.TransferData);
                var data = QueryBufferManager.GetQueryBuffer(sortRelation.RelationId);
                for (var i = 0; i < data.Count; i++)
                {
                    var sendData = new RelationDataPacket()
                    {
                        Id = new Identify() { ClientId = node.Id },
                        Data = data[i].Data,
                        RelationId = sortRelation.RelationId,
                        QueryNumber = fullQuery.Number,
                        QueryId = fullQuery.Id,
                        SubQueryId = sortQuery.QueryId,
                        IsLast = i == data.Count - 1,
                        OrderNumber = i
                    };

                    _taskSequenceHelper.AddTask(new Task(obj =>
                        {
                            var tup = (Tuple<RelationDataPacket, Relation>)obj;
                            Server.Send(tup.Item1);
                            if (tup.Item1.IsLast)
                            {
                                QueryManager.SetSortRelationStatus(tup.Item2.RelationId, QueryRelationStatus.Transfered);
                                QueryBufferManager.RemoveData(tup.Item2.RelationId);
                            }
                        },
                        new Tuple<RelationDataPacket, Relation>(sendData, sortRelation)));
                }
            }
        }

        #region EventHandlers
        

        #endregion
        
        public override void DoAction()
        {
            TransferToSort();
        }
    }
}