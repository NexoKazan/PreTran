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

﻿namespace ClusterixN.Common.Data.Query.Relation
{
    /// <summary>
    /// Поле отношения
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Имя поля
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Новое имя в результирующем отношении (переименование)
        /// </summary>
        public string NewName { get; set; } = string.Empty;

        /// <summary>
        /// Параметры поля
        /// </summary>
        public string Params { get; set; } = string.Empty;
    }
}
