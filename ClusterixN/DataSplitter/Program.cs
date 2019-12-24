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
using System.IO;
using DataSplitter.Data.EventArgs;
using DataSplitter.Properties;

namespace DataSplitter
{
    internal static class Program
    {
        private static readonly Dictionary<string, int[]> Keys = new Dictionary<string, int[]>();
        private static object _syncObject = new object();

        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(@"Программа разделения данных по IO");
                Console.WriteLine($@"Использование: {AppDomain.CurrentDomain.FriendlyName} [количество узлов] [папка с исходными данными]");
                Console.WriteLine($@"Например: {AppDomain.CurrentDomain.FriendlyName} 3 D:\tpch");
                return;
            }

            var paramIndex = 0;
            var nodeCount = int.Parse(args[paramIndex++]);
            var dir = args[paramIndex];
            InitKeys();
            
            var cpuCount = 0;
            if (args.Length > 2) cpuCount = int.Parse(args[2]);
            Console.WriteLine(@"Generate scripts");
            GenerateLoadScript(nodeCount, dir, cpuCount);

            foreach (var file in Directory.EnumerateFiles(dir, "*.tbl"))
            {
                var fileName = Path.GetFileName(file);
                if (fileName != null)
                {
                    fileName = fileName.ToLower();
                    if (Keys.ContainsKey(fileName))
                    {
                        Console.WriteLine(fileName);
                        var sh = new SplitHelper(file, nodeCount);
                        sh.ProcessingEvent += ShOnProcessingEvent;
                        sh.ProcessingCompleteEvent += ShOnProcessingCompleteEvent;
                        sh.Process(Keys[fileName]);
                    }
                }
            }
        }

        private static void GenerateLoadScript(int nodeCount, string dir, int cpuCount = 0)
        {
            for (int i = 0, j = 0; i < nodeCount; i++)
            {
                var script = string.Format(Resources.loadDataBase, i, dir.Replace("\\", "/"), j);
                File.WriteAllText(dir + "//loadDataBase_" + i + (cpuCount == 0 ? "" : "_" + j++) + ".txt", script);
                if (j >= cpuCount) j = 0;
            }
        }

        private static void InitKeys()
        {
            Keys.Add("partsupp.tbl", new[] { 0, 1 });
            Keys.Add("part.tbl", new[] { 0 });
            Keys.Add("customer.tbl", new[] { 0 });
            Keys.Add("nation.tbl", new[] { 0 });
            Keys.Add("orders.tbl", new[] { 0 });
            Keys.Add("region.tbl", new[] { 0 });
            Keys.Add("supplier.tbl", new[] { 0 });
            Keys.Add("lineitem.tbl", new[] { 0, 3 });
        }

        private static void ShOnProcessingCompleteEvent(object sender,
            ProcessingCompleteEventArgs processingCompleteEventArgs)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine(@"Processed: {0} lines", processingCompleteEventArgs.ProcessedLines);

            foreach (var buf in processingCompleteEventArgs.ProcessedLinesByNodes)
                Console.WriteLine(@"Node {1}: {0} lines", buf.Value, buf.Key);
        }

        private static void ShOnProcessingEvent(object sender, ProcessingEventArgs processingEventArgs)
        {
            lock (_syncObject)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(@"Processing... " + processingEventArgs.ProcessedLines);
            }
        }
    }
}