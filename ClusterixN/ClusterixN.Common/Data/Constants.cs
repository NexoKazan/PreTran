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

﻿namespace ClusterixN.Common.Data
{
    /// <summary>
    /// Константы для приложения
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Тег имени отношения
        /// </summary>
        public const string RelationNameTag = "{RELATIONNAME}";

        /// <summary>
        /// Тег имени левого отношения
        /// </summary>
        public const string LeftRelationNameTag = "{LEFTRELATION}";

        /// <summary>
        /// Тег имени правого отношения
        /// </summary>
        public const string RightRelationNameTag = "{RIGHTRELATION}";

        /// <summary>
        /// Окончание названия временных отношений
        /// </summary>
        public const string RelationPostfix = "_tmp";
    }
}