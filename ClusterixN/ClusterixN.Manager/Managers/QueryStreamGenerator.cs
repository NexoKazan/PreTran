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
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Interfaces;
using ClusterixN.Manager.Interfaces;

namespace ClusterixN.Manager.Managers
{
    internal class QueryStreamGenerator
    {
        private readonly int _count;
        private readonly IQuerySourceManager _querySourceManager;
        private readonly IQueryNumberGenerator _generator;
        private int _generatedCount;

        public QueryStreamGenerator(int count, IQuerySourceManager querySourceManager, IQueryNumberGenerator generator)
        {
            _count = count;
            _querySourceManager = querySourceManager;
            _generator = generator;
            _generatedCount = 0;
        }

        public List<Query> Generate(int count = 1)
        {
            if (_generatedCount >= _count) return new List<Query>();

            var queries = new List<Query>();
            for (var i = 0; i < count; i++)
            {
                queries.Add(_querySourceManager.GetQueryByNumber(_generator.GetNextQueryNumber()));
                _generatedCount++;
            }
            return queries;
        }
    }
}