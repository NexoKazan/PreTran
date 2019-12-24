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
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;

namespace ClusterixN.Manager.Managers
{
    public class SequenceQueryManager : QueryManager
    {
        public override JoinQuery GetNextJoinQuery(Guid queryId)
        {
            lock (SyncObject)
            {
                var query = GetQueryById(queryId);
                if (query == null) return null;

                var jQueries = query.JoinQueries.Where(
                    q =>
                        q.Status == QueryStatus.Wait || q.LeftRelation.Status == QueryRelationStatus.Wait ||
                        q.RightRelation.Status == QueryRelationStatus.Wait);

                foreach (var joinQuery in jQueries)
                {
                    var leftSelect =
                        query.SelectQueries.FirstOrDefault(s => s.QueryId == joinQuery.LeftRelation.RelationId);
                    var rightSelect =
                        query.SelectQueries.FirstOrDefault(s => s.QueryId == joinQuery.RightRelation.RelationId);

                    if (leftSelect != null && rightSelect != null)
                    {
                        if (leftSelect.Status == QueryStatus.SelectProcessed &&
                            rightSelect.Status == QueryStatus.SelectProcessed)
                            return joinQuery;
                    }
                    else
                    {
                        if (leftSelect != null)
                        {
                            if (leftSelect.Status == QueryStatus.SelectProcessed)
                                return joinQuery;
                        }
                        else
                        {
                            if (rightSelect?.Status == QueryStatus.SelectProcessed)
                                return joinQuery;
                        }
                    }
                }
                return null;
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
                if (leftSelect.Status == QueryStatus.SelectProcessed &&
                    rightSelect.Status == QueryStatus.SelectProcessed)
                    return true;
            }
            else
            {
                if (leftSelect != null)
                {
                    if (leftSelect.Status == QueryStatus.SelectProcessed)
                        return true;
                }
                else
                {
                    if (rightSelect?.Status == QueryStatus.SelectProcessed)
                        return true;
                }
            }
            return false;
        }
        
        public override Query GetQueryToJoin()
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.Status == QueryProcessStatus.SelectComplete);
            }
        }
    }
}