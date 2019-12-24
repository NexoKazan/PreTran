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
using ClusterixN.Common.Data.Query.Enum;

namespace ClusterixN.Common.Data.Query.Relation
{
    /// <summary>
    /// Отношение
    /// </summary>
    public class Relation
    {
        /// <summary>
        /// Уникальный идентификатор отношения
        /// </summary>
        public Guid RelationId { get; set; }
        
        /// <summary>
        /// Схема отношения
        /// </summary>
        public RelationSchema Shema { get; set; }

        /// <summary>
        /// Имя отношения
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Текущий статус отношения
        /// </summary>
        public QueryRelationStatus Status { get; set; }

        /// <summary>
        /// Объем данных в отношении
        /// </summary>
        public float DataAmount { get; set; }

        /// <summary>
        /// Признак пустого отношения
        /// </summary>
        public bool IsEmpty { get; set; }
    }
}