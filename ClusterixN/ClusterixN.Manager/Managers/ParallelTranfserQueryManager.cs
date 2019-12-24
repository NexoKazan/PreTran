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

﻿using System;
using System.Linq;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
 using ClusterixN.Common.Data.Query.Relation;

namespace ClusterixN.Manager.Managers
{
    public class ParallelTranfserQueryManager : QueryManager
    {
        public override JoinQuery GetNextJoinQuery(Guid queryId)
        {
            lock (SyncObject)
            {
                var query = GetQueryById(queryId);

                var jQueries = query?.JoinQueries.Where(
                    q =>
                        q.LeftRelation.Status == QueryRelationStatus.Wait ||
                        q.RightRelation.Status == QueryRelationStatus.Wait ||
                        q.LeftRelation.Status == QueryRelationStatus.WaitAnotherRelation ||
                        q.RightRelation.Status == QueryRelationStatus.WaitAnotherRelation);

                return jQueries?.FirstOrDefault();
            }
        }

        public override bool CheckJoinQueryReady(JoinQuery joinQuery)
        {
            var query = GetQueryByJoinQueryId(joinQuery.QueryId);

            var leftSelect =
                query.SelectQueries.FirstOrDefault(s => s.QueryId == joinQuery.LeftRelation.RelationId);
            var rightSelect =
                query.SelectQueries.FirstOrDefault(s => s.QueryId == joinQuery.RightRelation.RelationId);

            if (leftSelect != null && rightSelect != null)
            {
                if ((leftSelect.Status == QueryStatus.SelectProcessed ||
                     leftSelect.Status == QueryStatus.TransferSelectResult ||
                     leftSelect.Status == QueryStatus.SelectProcessing) &&
                    (rightSelect.Status == QueryStatus.SelectProcessed ||
                     rightSelect.Status == QueryStatus.TransferSelectResult ||
                     rightSelect.Status == QueryStatus.SelectProcessing))
                    return true;
            }
            else
            {
                if (leftSelect != null)
                {
                    if (leftSelect.Status == QueryStatus.SelectProcessed ||
                        leftSelect.Status == QueryStatus.TransferSelectResult ||
                        leftSelect.Status == QueryStatus.SelectProcessing)
                        return true;
                }
                else
                {
                    if (rightSelect?.Status == QueryStatus.SelectProcessed ||
                        rightSelect?.Status == QueryStatus.TransferSelectResult ||
                        rightSelect?.Status == QueryStatus.SelectProcessing)
                        return true;
                }
            }
            return false;
        }
        
        public override Query GetQueryToJoin()
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q =>
                    q.Status == QueryProcessStatus.Select &&
                    q.JoinQueries.All(j =>
                        (j.LeftRelation.Status == QueryRelationStatus.Wait ||
                         j.LeftRelation.Status == QueryRelationStatus.WaitAnotherRelation) &&
                        (j.RightRelation.Status == QueryRelationStatus.Wait ||
                         j.RightRelation.Status == QueryRelationStatus.WaitAnotherRelation)));
            }
        }


        public override Query GetQueryToSort()
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.Status > QueryProcessStatus.Wait && q.SortQuery.Status == QueryStatus.Wait);
            }
        }

        public override bool SetSortRelationStatus(Guid relationId, QueryRelationStatus queryStatus)
        {
            lock (SyncObject)
            {
                Relation sortRelation = null;
                SortQuery sortQuery = null;
                foreach (var query in Queries)
                foreach (var relation in query.SortQuery.SortRelation)
                    if (relation.RelationId == relationId)
                    {
                        sortRelation = relation;
                        sortQuery = query.SortQuery;
                        break;
                    }

                if (sortRelation != null)
                {
                    if (sortRelation.Status > queryStatus)
                    {
                        Logger.Error($"Уже был получен более высокий статус для отношения {relationId}");
                    }
                    else
                    {
                        if (sortRelation.Status != queryStatus)
                            Logger.Trace($"Новый статус отношения {relationId} -> {queryStatus}");
                        sortRelation.Status = queryStatus;
                    }
                    if (sortRelation.Status == QueryRelationStatus.Processed)
                    {
                        SetSubQueryStatus(sortQuery.QueryId, QueryStatus.SortProcessed);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}