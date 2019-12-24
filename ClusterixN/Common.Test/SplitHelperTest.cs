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

﻿using System.Diagnostics;
using System.IO;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.Hasher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Test
{
    [TestClass]
    public class SplitHelperTest
    {
        private byte[] _data;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            _data = File.ReadAllBytes("orders.tbl");
        }
        
        [TestMethod]
        public void TestParallel()
        {
            var sw = new Stopwatch();
            sw.Start();

            IHasher splitHelper = new ParallelHashHelper();
            splitHelper.ProcessData(_data, 40, new[] {0});

            sw.Stop();
            TestContext.WriteLine($"ProcessingTime : {sw.Elapsed}");
            sw.Reset();
        }

        [TestMethod]
        public void TestSequenced()
        {
            var sw = new Stopwatch();
            sw.Start();

            IHasher splitHelper = new SequenceHashHelper();
            splitHelper.ProcessData(_data, 4, new[] { 0 });

            sw.Stop();
            TestContext.WriteLine($"ProcessingTime : {sw.Elapsed}");
            sw.Reset();
        }

        [TestMethod]
        public void CheckResults()
        {
            IHasher parallelSplitHelper = new ParallelHashHelper();
            IHasher splitHelper = new SequenceHashHelper();
            var parallelResult = parallelSplitHelper.ProcessData(_data, 4, new[] { 0 });
            var seqResult = splitHelper.ProcessData(_data, 4, new[] { 0 });
            for (var i = 0; i < parallelResult.Count; i++)
            {
                File.WriteAllBytes(nameof(ParallelHashHelper) + "_" + i, parallelResult[i]);
            }
            for (var i = 0; i < seqResult.Count; i++)
            {
                File.WriteAllBytes(nameof(SequenceHashHelper) + "_" + i, seqResult[i]);
            }
            for (int i = 0; i < seqResult.Count; i++)
            {
                for (int j = 0; j < seqResult[i].Length; j++)
                {
                    if (seqResult[i][j] != parallelResult[i][j])
                    {
                        TestContext.WriteLine($"file: {i}");
                        TestContext.WriteLine($"seqResult :");
                        var str = string.Empty;
                        for (int k = j-5; k < j+5; k++)
                        {
                            str += $"{seqResult[i][k]} ";
                        }
                        TestContext.WriteLine(str);

                        TestContext.WriteLine($"parallelResult :");
                        str = string.Empty;
                        for (int k = j - 5; k < j + 5; k++)
                        {
                            str += $"{parallelResult[i][k]} ";
                        }
                        TestContext.WriteLine(str);

                        Assert.Fail("Получены разные результаты");
                    }
                }
            }
        }

    }
}

