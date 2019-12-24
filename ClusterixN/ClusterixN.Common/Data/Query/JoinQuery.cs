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

﻿using ClusterixN.Common.Data.Query.Base;
using ClusterixN.Common.Data.Query.Relation;

namespace ClusterixN.Common.Data.Query
{
    /// <summary>
    /// Запрос JOIN (выполняется на узлах join)
    /// </summary>
    public class JoinQuery : TimeMeasureQueryBase
    {
        /// <summary>
        /// Левое отношение
        /// </summary>
        public Relation.Relation LeftRelation { get; set; }

        /// <summary>
        /// Правое отношение
        /// </summary>
        public Relation.Relation RightRelation { get; set; }

        /// <summary>
        /// Схема результата
        /// </summary>
        public RelationSchema ResultSchema { get; set; }

        /// <summary>
        /// Запрос на получение результата
        /// </summary>
        public string ResultSelectQuery { get; set; }
    }
}