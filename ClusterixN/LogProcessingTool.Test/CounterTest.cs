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
    public class CounterTest
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

        [TestMethod]
        public void CountTest()
        {
            var qec = new QueryExecutionCounter();
            var str = qec.Count("20180117_114319.db");
            TestContext.WriteLine(str);
        }
    }
}
