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
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;

namespace ClusterixN.QueryProcessing.Data
{
    public class Relation : Common.Data.Query.Relation.Relation
    {
        public Guid QueryId { get; set; }

        public new RelationStatus Status { get; set; }

        public string RelationOriginalName { get; set; }

        public string RelationName => "a_"+RelationId.ToString("N") + Constants.RelationPostfix;
    }
}