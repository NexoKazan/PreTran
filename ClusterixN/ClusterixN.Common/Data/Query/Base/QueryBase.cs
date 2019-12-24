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

namespace ClusterixN.Common.Data.Query.Base
{
    /// <summary>
    /// Запрос
    /// </summary>
    public class QueryBase
    {
        /// <summary>
        /// Запрос
        /// </summary>
        public QueryBase()
        {
            QueryId = Guid.NewGuid();
            Query = string.Empty;
        }

        /// <summary>
        /// Порядковый номер запроса
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Статус запроса
        /// </summary>
        public QueryStatus Status { get; set; }

        /// <summary>
        /// Уникальный идентификатор запроса
        /// </summary>
        public Guid QueryId { get; set; }

        /// <summary>
        /// Запрос
        /// </summary>
        public string Query { get; set; }
    }
}