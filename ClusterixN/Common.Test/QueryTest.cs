#region Copyright
/*
 * Copyright 2018 Roman Klassen
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

using System.Collections.Generic;
using System.Linq;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Test
{
    [TestClass]
    public class QueryTest
    {
        private TestContext _testContextInstance;
        private HashQuerySourceManager _querySource;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return _testContextInstance; }
            set { _testContextInstance = value; }
        }
        
        [TestInitialize]
        public void TestInit()
        {
            _querySource = new HashQuerySourceManager();
//            string dump = ObjectDumper.Dump(_query);
//            TestContext.WriteLine(dump);
        }

        private bool IsLastJoin(Query query, JoinQuery joinQuery)
        {
            return !query.JoinQueries
                .Any(j => j.LeftRelation.RelationId == joinQuery.QueryId ||
                          j.RightRelation.RelationId == joinQuery.QueryId);
        }

        [TestMethod]
        public void TestLastJoin()
        {
            for (int i = 1; i < 15; i++)
            {
                var query = _querySource.GetQueryByNumber(i);
                
                foreach (var joinQuery in query.JoinQueries)
                {
                    var isLast = IsLastJoin(query, joinQuery);
                    TestContext.WriteLine($"{query.Number}\t{joinQuery.QueryId}\t{isLast}");
                }
            }
        }

        [TestMethod]
        public void TestSerialization()
        {
            var queries = new List<Query>();
            var xmls = new List<string>();
            for (int i = 1; i < 15; i++)
            {
                var query = _querySource.GetQueryByNumber(i);
                queries.Add(query);
                xmls.Add(query.SaveToString());
            }
            for (int i = 0; i < 14; i++)
            {
                var query = Query.LoadFromString(xmls[i]);
                Assert.AreEqual(query.Id, queries[i].Id);
            }

        }
    }
}

