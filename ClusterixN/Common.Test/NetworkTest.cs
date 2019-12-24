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
using System.Diagnostics;
using System.IO;
using System.Threading;
using ClusterixN.Common.Utils;
using ClusterixN.Network;
using ClusterixN.Network.Packets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

namespace Common.Test
{
    [TestClass]
    public class NetworkTest
    {
        private TestContext _testContextInstance;
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
            _data = File.ReadAllBytes("orders.tbl");
        }
        
        [TestMethod]
        public void TestSerializer()
        {
            var mstream = new MemoryStream();
            var server = new NetworkServer();
            server.Start(1234);
            var client = new NetworkClient();
            client.Connect("localhost", 1234);

            var sw = new Stopwatch();
            sw.Start();

            client.SendPacket(
                new RelationDataPacket()
                {
                    RelationId = Guid.NewGuid(),
                    QueryId = Guid.NewGuid(),
                    SubQueryId = Guid.NewGuid(),
                    OrderNumber = 1,
                    QueryNumber = 12,
                    Data = _data,
                    IsLast = false
                });

            sw.Stop();
            TestContext.WriteLine($"ProcessingTime : {sw.Elapsed}");
            sw.Reset();
            Thread.Sleep(10000);
            client.Disconnect();
            server.Stop();
        }
    }
}

