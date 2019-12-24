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
using System.IO;
using System.Linq;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Test
{
    [TestClass]
    public class BufferManagerTest
    {
        private QueryBufferManager _bufferManager;
        private const string Dir = "tmp";

        [TestInitialize]
        public void TestInit()
        {
            _bufferManager = new QueryBufferManager(Dir);
            if (Directory.Exists(Dir)) 
                Directory.Delete(Dir, true);
            Directory.CreateDirectory(Dir);
        }
        
        [TestMethod]
        public void TestQueryBufferFlush()
        {
            var rand = new Random();
            var block = new QueryBuffer()
            {
                QueryId = Guid.Empty,
                IsLast = false,
                Data = new byte[4096],
                OrderNumber = 0
            };
            rand.NextBytes(block.Data);
            _bufferManager.AddData(block);

            block = new QueryBuffer()
            {
                QueryId = Guid.Empty,
                IsLast = true,
                Data = new byte[4096],
                OrderNumber = 1
            };
            rand.NextBytes(block.Data);
            _bufferManager.AddData(block);
            _bufferManager.FlushBlockToDisk();

            var buf = _bufferManager.GetQueryBuffer(Guid.Empty);

            Assert.AreEqual(block.Data, buf[buf.Count - 1].Data);

            _bufferManager.FlushBlockToDisk();
            _bufferManager.RemoveBlock(block);
            if (Directory.EnumerateFiles(Dir).Any()) Assert.Fail("Файл не удален");


        }
    }
}

