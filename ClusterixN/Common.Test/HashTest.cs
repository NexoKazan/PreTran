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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Hasher;
using ClusterixN.Manager.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Test
{
    [TestClass]
    public class HashTest
    {
        private TestContext _testContextInstance;
        private IHasher _hasher;
        private byte[] _data;

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
            _hasher = new GpuHashHelper();
            _data = File.ReadAllBytes("orders.tbl");
        }
        
        [TestMethod]
        public void TestManyHash()
        {
            List<byte[]> result;
            for (int i = 0; i < 10; i++)
            {
                result = _hasher.ProcessData(_data, 30, new[] { 0 });
            }

            var memUsed = GC.GetTotalMemory(true);
            TestContext.WriteLine($"Used Memory: {memUsed/1024/1024} MB");
        }
    }
}

