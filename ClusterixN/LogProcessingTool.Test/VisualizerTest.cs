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
using System.Collections.Generic;
using System.Diagnostics;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Utils;
using LogProcessingTool.Visualizer;
using LogProcessingTool.Visualizer.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogProcessingTool.Test
{
    [TestClass]
    public class VisualizerTest
    {
        [TestMethod]
        public void GenerateTest()
        {
            OperationVisualizer ovz = new OperationVisualizer(0.1f,5,string.Empty);
            var random = new Random();
            var logevents = new List<LogData>()
            {
                new LogData()
                {
                    Duration = random.Next(10, 200),
                    Module = "IO",
                    Operation = MeasuredOperation.ProcessingSelect,
                    Time = SystemTime.Now + TimeSpan.FromMinutes(random.Next(1, 5))
                },
                new LogData()
                {
                    Duration = random.Next(50, 150),
                    Module = "IO",
                    Operation = MeasuredOperation.DataTransfer,
                    Time = SystemTime.Now + TimeSpan.FromMinutes(random.Next(4, 10))
                },
                new LogData()
                {
                    Duration = random.Next(30, 100),
                    Module = "IO",
                    Operation = MeasuredOperation.DataTransfer,
                    Time = SystemTime.Now + TimeSpan.FromMinutes(random.Next(2, 5))
                },
                new LogData()
                {
                    Duration = random.Next(10, 200),
                    Module = "IO",
                    Operation = MeasuredOperation.ProcessingSelect,
                    Time = SystemTime.Now + TimeSpan.FromMinutes(random.Next(1, 5))
                },
            };
            ovz.Visualize(logevents, "result.bmp");
        }
        [TestMethod]
        public void GenerateFromDbTest()
        {
            OperationVisualizer ovz = new OperationVisualizer(0.1f, 5, "test.db");
            ovz.Visualize("result.png");
            Process.Start("result.png");
        }
        [TestMethod]
        public void GenerateRamCounterFromDbTest()
        {
            var ovz = new RamVisualizer(0.1f, 128000, 100);
            ovz.Visualize("test.db", "result.png");
            Process.Start("result.png");
        }
        [TestMethod]
        public void GenerateCpuCounterFromDbTest()
        {
            var ovz = new CpuVisualizer(0.1f, 100, TimeSpan.FromMinutes(1));
            ovz.Visualize("test.db", "result.png");
            Process.Start("result.png");
        }
    }
}
