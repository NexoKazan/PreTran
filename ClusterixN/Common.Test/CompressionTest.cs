#region Copyright
/*
 * Copyright 2019 Roman Klassen
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Compression;
using ClusterixN.Common.Utils.Hasher;
using ClusterixN.Manager.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Test
{
    [TestClass]
    public class CompressionTest
    {
        private TestContext _testContextInstance;
        private ICompressionProvider _compressionProvider;
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
            _compressionProvider = new GZipCompressionProvider();
            _data = File.ReadAllBytes("orders.tbl");
        }
        
        [TestMethod]
        public void TestCompress()
        {
            var sw = new Stopwatch();
            var originalLength = _data.Length;

            sw.Start();

            var bytes = _compressionProvider.CompressBytes(_data);

            sw.Stop();

            var compressedLength = bytes.Length;
            var memUsed = GC.GetTotalMemory(true);

            TestContext.WriteLine(
                $"Compression ratio: {originalLength / (float) compressedLength} {originalLength / 1024 / 1024} MB / {compressedLength / 1024 / 1024} MB");
            TestContext.WriteLine($"Time: {sw.Elapsed}");
            TestContext.WriteLine($"Used Memory: {memUsed/1024/1024} MB");
        }
        
        [TestMethod]
        public void TestCompressSmall()
        {
            byte[] data = {
                34, 65, 34, 124, 34, 70, 34, 124, 34, 51, 55, 54, 56, 50, 57, 57, 51, 46, 48, 48, 34, 124, 34, 53, 54,
                53, 48, 57, 52, 53, 52, 52, 56, 49, 46, 51, 55, 34, 124, 34, 53, 51, 54, 56, 53, 48, 50, 48, 52, 55, 49,
                46, 49, 56, 34, 124, 34, 53, 53, 56, 51, 50, 57, 49, 53, 50, 48, 56, 46, 49, 48, 34, 124, 34, 50, 53,
                46, 53, 50, 34, 124, 34, 51, 56, 50, 55, 50, 46, 55, 56, 34, 124, 34, 48, 46, 48, 53, 34, 124, 34, 49,
                52, 55, 54, 52, 57, 50, 34, 10, 34, 78, 34, 124, 34, 70, 34, 124, 34, 57, 56, 57, 57, 55, 53, 46, 48,
                48, 34, 124, 34, 49, 52, 56, 53, 50, 55, 51, 49, 56, 55, 46, 57, 55, 34, 124, 34, 49, 52, 49, 48, 57,
                54, 53, 54, 55, 49, 46, 48, 50, 34, 124, 34, 49, 52, 54, 55, 52, 52, 48, 55, 56, 49, 46, 57, 51, 34,
                124, 34, 50, 53, 46, 53, 50, 34, 124, 34, 51, 56, 50, 56, 52, 46, 49, 56, 34, 124, 34, 48, 46, 48, 53,
                34, 124, 34, 51, 56, 55, 57, 54, 34, 10, 34, 78, 34, 124, 34, 79, 34, 124, 34, 55, 52, 51, 55, 50, 53,
                49, 55, 46, 48, 48, 34, 124, 34, 49, 49, 49, 53, 52, 54, 57, 48, 52, 52, 50, 51, 46, 56, 52, 34, 124,
                34, 49, 48, 53, 57, 55, 48, 57, 57, 55, 54, 53, 52, 46, 48, 50, 34, 124, 34, 49, 49, 48, 50, 49, 51, 57,
                54, 55, 49, 55, 48, 46, 51, 51, 34, 124, 34, 50, 53, 46, 53, 48, 34, 124, 34, 51, 56, 50, 52, 57, 46,
                53, 57, 34, 124, 34, 48, 46, 48, 53, 34, 124, 34, 50, 57, 49, 54, 50, 57, 48, 34, 10, 34, 82, 34, 124,
                34, 70, 34, 124, 34, 51, 55, 54, 54, 56, 55, 56, 57, 46, 48, 48, 34, 124, 34, 53, 54, 52, 57, 50, 49,
                54, 55, 54, 51, 51, 46, 57, 50, 34, 124, 34, 53, 51, 54, 54, 57, 49, 56, 55, 55, 52, 55, 46, 55, 52, 34,
                124, 34, 53, 53, 56, 49, 52, 54, 51, 57, 53, 51, 51, 46, 52, 50, 34, 124, 34, 50, 53, 46, 53, 49, 34,
                124, 34, 51, 56, 50, 53, 49, 46, 50, 56, 34, 124, 34, 48, 46, 48, 53, 34, 124, 34, 49, 52, 55, 54, 56,
                55, 48, 34, 10
            };
            var sw = new Stopwatch();
            var originalLength = data.Length;

            sw.Start();

            var bytes = _compressionProvider.CompressBytes(data);

            sw.Stop();

            var compressedLength = bytes.Length;
            var memUsed = GC.GetTotalMemory(true);

            TestContext.WriteLine(
                $"Compression ratio: {originalLength / (float) compressedLength} {originalLength}  / {compressedLength } ");
            TestContext.WriteLine($"Time: {sw.Elapsed}");
            TestContext.WriteLine($"Used Memory: {memUsed/1024/1024} MB");


            originalLength = compressedLength;

            sw.Start();

            var bytesDec = _compressionProvider.DecompressBytes(bytes);

            sw.Stop();

            var decompressedLength = bytesDec.Length;
            memUsed = GC.GetTotalMemory(true);
            TestContext.WriteLine(
                $"Compression ratio: {originalLength / (float)decompressedLength} {originalLength }  / {decompressedLength } ");
            TestContext.WriteLine($"Time: {sw.Elapsed}");
            TestContext.WriteLine($"Used Memory: {memUsed / 1024 / 1024} MB");
        }

        [TestMethod]
        public void TestDecompress()
        {
            var sw = new Stopwatch();
            var compressedBytes = _compressionProvider.CompressBytes(_data);
            var originalLength = compressedBytes.Length;

            sw.Start();

            var bytes = _compressionProvider.DecompressBytes(compressedBytes);

            sw.Stop();

            var compressedLength = bytes.Length;
            var memUsed = GC.GetTotalMemory(true);

            TestContext.WriteLine(
                $"Compression ratio: {originalLength / (float) compressedLength} {originalLength / 1024 / 1024} MB / {compressedLength / 1024 / 1024} MB");
            TestContext.WriteLine($"Time: {sw.Elapsed}");
            TestContext.WriteLine($"Used Memory: {memUsed / 1024 / 1024} MB");
        }
    }
}

