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
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.JoinTree;
using ClusterixN.Common.Utils;

namespace ClusterixN.Common.Data.Query
{
    /// <summary>
    /// Класс описания запроса
    /// </summary>
    public class Query : XmlSerializationBase<Query>
    {
        public Query()
        {
            Id = Guid.NewGuid();
            SelectQueries = new List<SelectQuery>();
            JoinQueries = new List<JoinQuery>();
        }

        /// <summary>
        /// Запросы для модуля IO (SELECT)
        /// </summary>
        public List<SelectQuery> SelectQueries { get; set; }

        /// <summary>
        /// Запросы для модуля JOIN
        /// </summary>
        public List<JoinQuery> JoinQueries { get; set; }

        /// <summary>
        /// Запросы для модуля SORT
        /// </summary>
        public SortQuery SortQuery { get; set; }

        /// <summary>
        /// Текущий статус запроса
        /// </summary>
        public QueryProcessStatus Status { get; set; }

        /// <summary>
        /// Номер запроса из теста TPC-H
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Последовательный номер (устанавливается при создании запроса)
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Уникальный идентификатор запроса
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Признак тяжелого запроса
        /// </summary>
        public bool IsHard { get; set; }

        /// <summary>
        /// Объем данных всех отношений для JOIN
        /// </summary>
        public float DataAmount => JoinQueries.Sum(j => j.LeftRelation.DataAmount + j.RightRelation.DataAmount);

        /// <summary>
        /// Получить дерево JOIN
        /// </summary>
        /// <returns></returns>
        public JoinTreeLeaf[] GetJoinTree()
        {
            var entities = new List<JoinTreeLeaf>();

            foreach (var joinQuery in JoinQueries)
            {
                entities.Add(new JoinTreeLeaf()
                {
                    RelationId = joinQuery.QueryId,
                    Query = joinQuery.Query,
                    LeftRelation = new JoinTreeLeaf()
                    {
                        RelationId = joinQuery.LeftRelation.RelationId,
                    },
                    RightRelation = new JoinTreeLeaf()
                    {
                        RelationId = joinQuery.RightRelation.RelationId,
                    }
                });
            }

            foreach (var ent in entities)
            {
                foreach (var listEnt in entities)
                {
                    if (listEnt.LeftRelation.RelationId == ent.RelationId) listEnt.LeftRelation = ent;
                    if (listEnt.RightRelation.RelationId == ent.RelationId) listEnt.RightRelation = ent;
                }
            }

            var result = new List<JoinTreeLeaf>();

            foreach (var ent in entities)
            {
                var found = false;
                foreach (var listEnt in entities)
                {
                    if (listEnt.LeftRelation.RelationId == ent.RelationId ||
                        listEnt.RightRelation.RelationId == ent.RelationId)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    result.Add(ent);
            }

            return result.ToArray();
        }
    }
}