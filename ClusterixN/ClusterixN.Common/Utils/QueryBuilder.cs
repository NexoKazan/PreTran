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
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;

namespace ClusterixN.Common.Utils
{
    public class QueryBuilder
    {
        public static QueryRelationStatus DefaultJoinRelationStatus = QueryRelationStatus.Wait;
        public static QueryRelationStatus DefaultRelationStatus = QueryRelationStatus.Wait;

        private readonly Query _query;
        
        public QueryBuilder(int number)
        {
            _query = new Query() {Number = number};
        }

        public SelectQuery CreateSelectQuery(string query, int order)
        {
            var selectQuery = new SelectQuery() {Query = TrimQuery(query), Order = order};
            return selectQuery;
        }

        public RelationSchema CreateRelationSchema(List<Field> fields, List<Index> indexes)
        {
            return new RelationSchema()
            {
                Fields = fields,
                Indexes = indexes
            };
        }

        private Relation CreateRelation(Guid dataQueryId, string name, RelationSchema schema, QueryRelationStatus initialStatus = QueryRelationStatus.Wait, bool isEmpty = false)
        {
            if (initialStatus == QueryRelationStatus.Wait) initialStatus = DefaultRelationStatus;

            var relation = new Relation()
            {
                RelationId = dataQueryId,
                Name = name,
                Shema = schema,
                Status = initialStatus,
                IsEmpty = isEmpty
            };
            return relation;
        }

        public Relation CreateEmptyRelation()
        {
            return CreateRelation(Guid.NewGuid(), "empty",
                CreateRelationSchema(new List<Field>() {new Field() {Name = "empty", Params = "INT"}},
                    new List<Index>()), QueryRelationStatus.Wait, true);
        }

        public Relation CreateRelation(JoinQuery joinQuery, QueryRelationStatus? startStatusNullable = null)
        {
            QueryRelationStatus startStatus = startStatusNullable ?? DefaultJoinRelationStatus;

            return CreateRelation(joinQuery.QueryId, joinQuery.LeftRelation.Name + "_" + joinQuery.RightRelation.Name,
                joinQuery.ResultSchema, startStatus);
        }

        public Relation CreateRelation(SelectQuery selectQuery, string name, RelationSchema schema)
        {
            return CreateRelation(selectQuery.QueryId, name, schema);
        }

        public JoinQuery CreateJoinQuery(string query, RelationSchema schema, int order, Relation leftRelation, Relation rightRelation, string resultSelect = "")
        {
            var joinQuery = new JoinQuery()
            {
                Query = TrimQuery(query),
                ResultSchema = schema,
                Order = order,
                LeftRelation = leftRelation,
                RightRelation = rightRelation,
                ResultSelectQuery = resultSelect
            };
            return joinQuery;
        }

        public SortQuery CreateSortQuery(string query, RelationSchema resultSchema, int order, string resultSelect, params Relation[] sortRelation)
        {
            var sortQuery = new SortQuery()
            {
                Query = TrimQuery(query),
                Order = order,
                SortRelation = sortRelation.ToList(),
                ResultSelectQuery = TrimQuery(resultSelect),
                ResultSchema = resultSchema
            };
            return sortQuery;
        }

        public SelectQuery AddSelectQuery(SelectQuery query)
        {
            _query.SelectQueries.Add(query);
            return query;
        }

        public JoinQuery AddJoinQuery(JoinQuery query)
        {
            _query.JoinQueries.Add(query);
            return query;
        }

        public SortQuery SetSortQuery(SortQuery query)
        {
            _query.SortQuery = query;
            return query;
        }

        public Query GetQuery(bool isHard = false)
        {
            _query.IsHard = isHard;
            return _query;
        }

        private string TrimQuery(string query)
        {
            query = query.Replace('\r', ' ');
            query = query.Replace('\t', ' ');
            return query;
        }
    }
}
