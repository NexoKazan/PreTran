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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClusterixN.Common;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;

namespace ClusterixN.Manager.Managers
{
    public class QueryManager : IQueryManager
    {
        protected readonly object SyncObject = new object();
        protected readonly List<Query> Queries;
        protected readonly Dictionary<Guid, TimeMeasureHelper> QueryProcessTimeMeasureHelpers;
        protected readonly ILogger Logger;
        protected readonly ILogger TimeLogger;

        public QueryManager()
        {
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            TimeLogger = ServiceLocator.Instance.LogService.GetLogger("mgmQueryTime");
            Logger.Trace("Инициализация менеджера запросов");
            Queries = new List<Query>();
            QueryProcessTimeMeasureHelpers = new Dictionary<Guid, TimeMeasureHelper>();
        }

        public int GetQueryCount()
        {
            return Queries.Count;
        }

        public void AddQuery(Query query)
        {
            lock (SyncObject)
            {
                Logger.Trace("Добавлен запрос " + query.Id);
                Queries.Add(query);
                QueryProcessTimeMeasureHelpers.Add(query.Id,new TimeMeasureHelper());
                QueryProcessTimeMeasureHelpers[query.Id].Start();
            }
        }

        public void DeleteQuery(Query query)
        {
            lock (SyncObject)
            {
                Logger.Trace("Удален запрос " + query.Id);
                Queries.Remove(query);
                QueryProcessTimeMeasureHelpers[query.Id].Stop();
                var times = GetQueryTimes(query);
                Logger.Info(times);
                TimeLogger.Info(times);
                QueryProcessTimeMeasureHelpers.Remove(query.Id);
                OnSendResult(query);
            }
        }

        private string GetQueryTimes(Query query)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Завершен запрос " + query.Id + " (" + query.Number + ") за " +
                          QueryProcessTimeMeasureHelpers[query.Id].Elapsed.TotalMilliseconds + " мс");
            sb.AppendLine("\tОперация SELECT:");
            foreach (var selectQuery in query.SelectQueries)
            {
                sb.AppendLine($"\t\tid = {selectQuery.QueryId}");
                foreach (var timeMeasure in selectQuery.GetTimeMeasures())
                {
                    sb.AppendLine(
                        $"\t\t\t{timeMeasure.MeasureDate}\t{timeMeasure.MeasureMessage}\t{timeMeasure.Time.TotalMilliseconds} мс");
                }
            }
            sb.AppendLine("\tОперация JOIN:");
            foreach (var joinQuery in query.JoinQueries)
            {
                sb.AppendLine($"\t\tid = {joinQuery.QueryId}");
                foreach (var timeMeasure in joinQuery.GetTimeMeasures())
                {
                    sb.AppendLine(
                        $"\t\t\t{timeMeasure.MeasureDate}\t{timeMeasure.MeasureMessage}\t{timeMeasure.Time.TotalMilliseconds} мс");
                }
            }
            sb.AppendLine("\tОперация SORT:");
            sb.AppendLine($"\t\tid = {query.SortQuery.QueryId}");
            foreach (var timeMeasure in query.SortQuery.GetTimeMeasures())
            {
                sb.AppendLine(
                    $"\t\t\t{timeMeasure.MeasureDate}\t{timeMeasure.MeasureMessage}\t{timeMeasure.Time.TotalMilliseconds} мс");
            }

            return sb.ToString();
        }

        public Query GetQueryById(Guid queryId)
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.Id == queryId);
            }
        }

        public Query GetQueryBySelectQueryId(Guid selectQueryId)
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.SelectQueries.Any(sq => sq.QueryId == selectQueryId));
            }
        }

        public Query GetQueryByJoinQueryId(Guid joinQueryId)
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.JoinQueries.Any(sq => sq.QueryId == joinQueryId));
            }
        }

        public Query GetQueryBySortQueryId(Guid sortQueryId)
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.SortQuery.QueryId == sortQueryId);
            }
        }

        public void SetQueryStatus(Guid queryId, QueryProcessStatus status)
        {
            lock (SyncObject)
            {
                var query = GetQueryById(queryId);
                if (query != null)
                {
                    if (query.Status != status) Logger.Trace($"Новый статус запроса {queryId} -> {status}");
                    query.Status = status;
                }
            }
        }

        public void SetSubQueryStatus(Guid queryId, QueryStatus status)
        {
            lock (SyncObject)
            {
                var selectQuery = GetQueryBySelectQueryId(queryId);
                if (selectQuery != null)
                {
                    var select = selectQuery.SelectQueries.FirstOrDefault(q => q.QueryId == queryId);
                    if (select != null)
                    {
                        if (select.Status > status)
                        {
                            Logger.Error($"Уже был получен более высокий статус для запроса {queryId}: {select.Status}");
                        }
                        else
                        {
                            if (select.Status != status) Logger.Trace($"Новый статус подзапроса SELECT {queryId}  {select.Status} -> {status}");
                            select.Status = status;
                        }

                        if (selectQuery.SelectQueries.All(s => s.Status == QueryStatus.SelectProcessed) &&
                            selectQuery.Status < QueryProcessStatus.SelectComplete)
                        {
                            selectQuery.Status = QueryProcessStatus.SelectComplete;
                            Logger.Trace($"Новый статус запроса {queryId} -> {status}");

                            if (selectQuery.JoinQueries.Count == 0)
                                SetQueryStatus(selectQuery.Id, QueryProcessStatus.JoinComplete);
                        }
                    }
                }
                else
                {
                    var joinQuery = GetQueryByJoinQueryId(queryId);
                    var join = joinQuery?.JoinQueries.FirstOrDefault(q => q.QueryId == queryId);
                    if (join != null)
                    {
                        if (join.Status > status)
                        {
                            Logger.Error($"Уже был получен более высокий статус для запроса {queryId}: {join.Status}");
                        }
                        else
                        {
                            if (join.Status != status) Logger.Trace($"Новый статус подзапроса JOIN {queryId} {join.Status} -> {status}");
                            join.Status = status;
                        }

                        if (joinQuery.JoinQueries.All(s => s.Status == QueryStatus.JoinResultTransfered) &&
                            joinQuery.Status < QueryProcessStatus.JoinComplete)
                        {
                            joinQuery.Status = QueryProcessStatus.JoinComplete;
                            Logger.Trace($"Новый статус запроса {queryId} -> {status}");
                        }
                    }
                    else
                    {
                        var sortQuery = GetQueryBySortQueryId(queryId);
                        var sort = sortQuery?.SortQuery;
                        if (sort != null)
                        {
                            if (sort.Status > status)
                            {
                                Logger.Error($"Уже был получен более высокий статус для запроса {queryId}: {sort.Status}");
                            }
                            else
                            {
                                if (sort.Status != status) Logger.Trace($"Новый статус подзапроса SORT {queryId} {sort.Status} -> {status}");
                                sort.Status = status;
                            }

                            if (sortQuery.SortQuery.Status == QueryStatus.SortResultTransfered && 
                                sortQuery.Status < QueryProcessStatus.SortComplete)
                            {
                                sortQuery.Status = QueryProcessStatus.SortComplete;
                                Logger.Trace($"Новый статус запроса {queryId} -> {status}");
                            }
                        }
                    }
                }
            }
        }

        public List<Query> GetAllQueries()
        {
            lock (SyncObject)
            {
                return Queries;
            }
        }

        public SelectQuery GetNextSelectQuery(Guid queryId)
        {
            lock (SyncObject)
            {
                var query = GetQueryById(queryId);
                return query?.SelectQueries.FirstOrDefault(q => q.Status == QueryStatus.Wait);
            }
        }

        public virtual JoinQuery GetNextJoinQuery(Guid queryId)
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
                        if ((leftSelect.Status == QueryStatus.SelectProcessed ||
                            leftSelect.Status == QueryStatus.TransferSelectResult) &&
                            (rightSelect.Status == QueryStatus.SelectProcessed ||
                             rightSelect.Status == QueryStatus.TransferSelectResult))
                            return joinQuery;
                    }
                    else
                    {
                        if (leftSelect != null)
                        {
                            if (leftSelect.Status == QueryStatus.SelectProcessed ||
                                leftSelect.Status == QueryStatus.TransferSelectResult)
                                return joinQuery;
                        }
                        else
                        {
                            if (rightSelect?.Status == QueryStatus.SelectProcessed ||
                                rightSelect?.Status == QueryStatus.TransferSelectResult)
                                return joinQuery;
                        }
                    }
                }
                return null;
            }
        }

        public virtual bool CheckJoinQueryReady(JoinQuery joinQuery)
        {
            var query = GetQueryByJoinQueryId(joinQuery.QueryId);

            var leftSelect =
                query.SelectQueries.FirstOrDefault(s => s.QueryId == joinQuery.LeftRelation.RelationId);
            var rightSelect =
                query.SelectQueries.FirstOrDefault(s => s.QueryId == joinQuery.RightRelation.RelationId);

            if (leftSelect != null && rightSelect != null)
            {
                if ((leftSelect.Status == QueryStatus.SelectProcessed ||
                     leftSelect.Status == QueryStatus.TransferSelectResult) &&
                    (rightSelect.Status == QueryStatus.SelectProcessed ||
                     rightSelect.Status == QueryStatus.TransferSelectResult))
                    return true;
            }
            else
            {
                if (leftSelect != null)
                {
                    if (leftSelect.Status == QueryStatus.SelectProcessed ||
                        leftSelect.Status == QueryStatus.TransferSelectResult)
                        return true;
                }
                else
                {
                    if (rightSelect?.Status == QueryStatus.SelectProcessed ||
                        rightSelect?.Status == QueryStatus.TransferSelectResult)
                        return true;
                }
            }
            return false;
        }

        public Query GetQueryToProcess()
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.Status == QueryProcessStatus.Wait);
            }
        }

        public virtual Query GetQueryToJoin()
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => (q.Status == QueryProcessStatus.SelectComplete ||
                                                     (q.SelectQueries.Any(
                                                          s => s.Status == QueryStatus.TransferSelectResult || s.Status == QueryStatus.SelectProcessed) &&
                                                      q.Status < QueryProcessStatus.Join)));
            }
        }

        public void SetRelationDataVolume(Guid relationId, float dataVolume)
        {
            lock (SyncObject)
            {
                Relation joinRelation = null;
                foreach (var query in Queries)
                foreach (var joinQuery in query.JoinQueries)
                    if (joinQuery.LeftRelation.RelationId == relationId)
                    {
                        joinRelation = joinQuery.LeftRelation;
                        break;
                    }
                    else if (joinQuery.RightRelation.RelationId == relationId)
                    {
                        joinRelation = joinQuery.RightRelation;
                        break;
                    }

                if (joinRelation != null)
                {
                    joinRelation.DataAmount = dataVolume;
                }
            }
        }

        public Relation GetRelationById(Guid relationId)
        {
            lock (SyncObject)
            {
                foreach (var query in Queries)
                {

                    foreach (var joinQuery in query.JoinQueries)
                    {
                        if (joinQuery.LeftRelation.RelationId == relationId)
                        {
                            return joinQuery.LeftRelation;
                        }
                        if (joinQuery.RightRelation.RelationId == relationId)
                        {
                            return joinQuery.RightRelation;
                        }
                    }

                    foreach (var relation in query.SortQuery.SortRelation)
                    {
                        if (relation.RelationId == relationId)
                        {
                            return relation;
                        }
                    }
                }
                return null;
            }
        }

        public bool SetJoinRelationStatus(Guid relationId, QueryRelationStatus queryStatus)
        {
            lock (SyncObject)
            {
                Relation joinRelation = null;
                JoinQuery fJoinQuery = null;
                foreach (var query in Queries)
                foreach (var joinQuery in query.JoinQueries)
                    if (joinQuery.LeftRelation.RelationId == relationId)
                    {
                        joinRelation = joinQuery.LeftRelation;
                        fJoinQuery = joinQuery;
                        break;
                    }
                    else if (joinQuery.RightRelation.RelationId == relationId)
                    {
                        joinRelation = joinQuery.RightRelation;
                        fJoinQuery = joinQuery;
                        break;
                    }

                if (joinRelation != null)
                {
                    if (joinRelation.Status > queryStatus)
                    {
                        Logger.Error($"Уже был получен более высокий статус для отношения {relationId}");
                    }
                    else
                    {
                        if (joinRelation.Status != queryStatus)
                            Logger.Trace($"Новый статус отношения {relationId} -> {queryStatus}");
                        joinRelation.Status = queryStatus;
                    }
                    if (fJoinQuery.LeftRelation.Status == QueryRelationStatus.Processed &&
                        fJoinQuery.RightRelation.Status == QueryRelationStatus.Processed)
                    {
                        SetSubQueryStatus(fJoinQuery.QueryId, QueryStatus.JoinProcessed);
                    }
                    return true;
                }
            }
            return false;
        }

        public virtual bool SetSortRelationStatus(Guid relationId, QueryRelationStatus queryStatus)
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
                    if (sortQuery.SortRelation.All(r => r.Status == QueryRelationStatus.Transfered))
                    {
                        SetSubQueryStatus(sortQuery.QueryId, QueryStatus.TransferedToSort);
                    }
                    return true;
                }
            }
            return false;
        }

        public void RemoveCompletedQueries()
        {
            lock (SyncObject)
            {
                var queriesToRemove = Queries.Where(q => q.Status == QueryProcessStatus.SortComplete).ToList();
                foreach (var query in queriesToRemove)
                {
                    DeleteQuery(query);
                }
            }
        }

        public virtual Query GetQueryToSort()
        {
            lock (SyncObject)
            {
                return Queries.FirstOrDefault(q => q.Status == QueryProcessStatus.JoinComplete);
            }
        }

        #region Events

        public event Action<Query> SendResult;

        protected virtual void OnSendResult(Query obj)
        {
            SendResult?.Invoke(obj);
        }

        #endregion
    }
}