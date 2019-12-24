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

namespace ClusterixN.Common.Data.Query.Relation
{
    /// <summary>
    /// Индекс для отношения
    /// </summary>
    public class Index
    {
        /// <summary>
        /// Индекс для отношения 
        /// </summary>
        /// <param name="isPrimary">создать первичный ключ</param>
        public Index(bool isPrimary)
        {
            FieldNames = new List<string>();
            IsPrimary = isPrimary;
        }

        /// <summary>
        /// Индекс для отношения 
        /// </summary>
        public Index() : this(false)
        { }

        /// <summary>
        /// Название индекса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// индекс является первичным ключом
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Названия столбцов, входящих в индекс
        /// </summary>
        public List<string> FieldNames { get; set; }
    }
}
