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
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;

namespace ClusterixN.Manager.Interfaces
{
    public interface IQueryManager
    {
        event Action<Query> SendResult;
        void AddQuery(Query query);
        bool CheckJoinQueryReady(JoinQuery joinQuery);
        void DeleteQuery(Query query);
        JoinQuery GetNextJoinQuery(Guid queryId);
        SelectQuery GetNextSelectQuery(Guid queryId);
        Query GetQueryById(Guid queryId);
        Query GetQueryByJoinQueryId(Guid joinQueryId);
        Query GetQueryBySelectQueryId(Guid selectQueryId);
        Query GetQueryBySortQueryId(Guid sortQueryId);
        int GetQueryCount();
        Query GetQueryToJoin();
        Query GetQueryToProcess();
        Query GetQueryToSort();
        void RemoveCompletedQueries();
        void SetQueryStatus(Guid queryId, QueryProcessStatus status);
        void SetRelationDataVolume(Guid relationId, float dataVolume);
        Relation GetRelationById(Guid relationId);
        bool SetSortRelationStatus(Guid relationId, QueryRelationStatus queryStatus);
        bool SetJoinRelationStatus(Guid relationId, QueryRelationStatus queryStatus);
        void SetSubQueryStatus(Guid queryId, QueryStatus status);
        List<Query> GetAllQueries();
    }
}