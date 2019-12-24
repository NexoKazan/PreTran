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
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers.Base
{
    internal abstract class JoinTransferDataSyncBase : JoinTransferDataBase
    {
        protected JoinTransferDataSyncBase(IServerCommunicator server, IQueryManager queryManager,
            QueryBufferManager queryBufferManager, NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : base(
            server, queryManager, queryBufferManager, nodesManager, pauseLogManager, queueManager)
        {
        }

        protected void TransferReadyRelationsToJoin(Node node, IEnumerable<Query> queries)
        {
            foreach (var query in queries)
            {
                var dataVolume = CalculateJoinRelationsDataVolume(query.JoinQueries
                    .Where(q => q.LeftRelation.Status == QueryRelationStatus.Prepared ||
                                q.RightRelation.Status == QueryRelationStatus.Prepared)
                    .ToList());

                if (!node.CanSendData((float)dataVolume)) continue;
                node.PendingRam += (float)dataVolume;

                foreach (var jQuery in query.JoinQueries)
                {
                    if (jQuery.LeftRelation.Status == QueryRelationStatus.Prepared)
                        TransferJoinRelation(node, jQuery.LeftRelation, jQuery, query);
                    if (jQuery.RightRelation.Status == QueryRelationStatus.Prepared)
                        TransferJoinRelation(node, jQuery.RightRelation, jQuery, query);
                }
            }
        }

        private double CalculateJoinRelationsDataVolume(List<JoinQuery> queries)
        {
            double dataVolume = 0;
            foreach (var joinQuery in queries)
            {
                var leftRelationLenght = joinQuery.LeftRelation.Status == QueryRelationStatus.Prepared
                    ? QueryBufferManager.GetBufferLenght(joinQuery.LeftRelation.RelationId)
                    : 0;
                var rightRelationLenght = joinQuery.RightRelation.Status == QueryRelationStatus.Prepared
                    ? QueryBufferManager.GetBufferLenght(joinQuery.RightRelation.RelationId)
                    : 0;
                var sum = leftRelationLenght + rightRelationLenght;

                QueryManager.SetRelationDataVolume(joinQuery.LeftRelation.RelationId,
                    leftRelationLenght / 1024f / 1024f);
                QueryManager.SetRelationDataVolume(joinQuery.RightRelation.RelationId,
                    rightRelationLenght / 1024f / 1024f);

                dataVolume += sum / 1024d / 1024d;
            }
            return dataVolume;
        }
    }
}