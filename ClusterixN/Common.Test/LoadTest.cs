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
    public class LoadTest
    {
        private TestContext _testContextInstance;

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

        }

        private List<int> GetPacketCounts(List<int> lastNumbers)
        {
            var packetsCount = new List<int>();
            var packetCounter = lastNumbers.Count;
            var counter = lastNumbers.Count;

            for (int i = 0; i < lastNumbers.Count - 1; i++)
            {
                packetsCount.Add(counter);
                packetCounter--;
                if (lastNumbers[i] != lastNumbers[i + 1])
                {
                    counter = packetCounter;
                }
            }

            packetsCount.Add(counter); // для последнего пакета
            return packetsCount;
        }

        [TestMethod]
        public void TestLastJoin()
        {
            var list = GetPacketCounts(new List<int>() {1, 1, 3});
            var result = new List<int>() {3, 3, 1};
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], list[i]);
            }

            list = GetPacketCounts(new List<int>() { 1, 2, 3 });
            result = new List<int>() { 3, 2, 1 };
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], list[i]);
            }

            list = GetPacketCounts(new List<int>() { 3, 3, 3 });
            result = new List<int>() { 3, 3, 3 };
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], list[i]);
            }

            list = GetPacketCounts(new List<int>() { 2, 3, 3 });
            result = new List<int>() { 3, 2, 2 };
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], list[i]);
            }
        }
    }
}

