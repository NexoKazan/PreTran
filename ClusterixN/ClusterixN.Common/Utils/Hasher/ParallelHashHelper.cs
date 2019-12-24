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
using System.Text;
using System.Threading.Tasks;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Utils.Hasher
{
    public class ParallelHashHelper : HashHelperBase, IHasher
    {
        public static int GetNodeNumberByLine(byte[] line, int nodeCount, int[] keys, int maxField = -1)
        {
            var index = FindIndex(line, '|', maxField);
            var buf = new byte[index];
            Array.Copy(line, 0, buf, 0, index);
            var fields = Split(buf, '|');
            var intFields = new int[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                var field = Encoding.UTF8.GetString(fields[keys[i]]);
                intFields[i] = ParseField(field);
            }
            return GetNodeNumber(nodeCount, intFields);
        }
        
        public List<byte[]> ProcessData(byte[] data, int nodeCount, int[] keys)
        {
            var buffers = new List<byte[]>(nodeCount);
            var tableData = data;
            var cpuCount = Environment.ProcessorCount - 2;
            var tasks = new List<System.Threading.Tasks.Task>();

            for (var cpuNumber = 0; cpuNumber < cpuCount; cpuNumber++)
            {
                var task = ProcessDataPartTask(nodeCount, keys, tableData, cpuNumber, cpuCount);
                tasks.Add(task);
                task.Start();
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            var bufLeghts = new List<int>(nodeCount);

            for (int i = 0; i < nodeCount; i++)
            {
                bufLeghts.Add(0);
            }

            for (var i = 0; i < cpuCount; i++)
            {
                var task = (Task<List<byte[]>>) tasks[i];
                for (var j = 0; j < nodeCount; j++)
                {
                    bufLeghts[j] += task.Result[j].Length;
                }
            }

            for (var i = 0; i < nodeCount; i++)
            {
                buffers.Add(new byte[bufLeghts[i]]);
                bufLeghts[i] = 0;
            }

            for (var i = 0; i < cpuCount; i++)
            {
                var task = (Task<List<byte[]>>) tasks[i];
                for (var j = 0; j < nodeCount; j++)
                {
                    Array.Copy(task.Result[j], 0, buffers[j], bufLeghts[j], task.Result[j].Length);
                    bufLeghts[j] += task.Result[j].Length;
                }
                task.Dispose();
            }
            GC.Collect();

            return buffers;
        }

        private Task<List<byte[]>> ProcessDataPartTask(int nodeCount, int[] keys, byte[] tableData,
            int cpuNumber, int cpuCount)
        {
            return new Task<List<byte[]>>(obj =>
            {
                var tup = (Tuple<int, int[], byte[], int, int>) obj;
                return ProcessDataPart(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item5);
            }, new Tuple<int, int[], byte[], int, int>(nodeCount, keys, tableData, cpuNumber, cpuCount));
        }

        private static List<byte[]> ProcessDataPart(int nodeCount, int[] keys, byte[] tableData, int cpuNumber, int cpuCount)
        {
            var chunk = tableData.Length / cpuCount;
            var chunkStart = cpuNumber * chunk;
            var chunkStop = chunk * (cpuNumber + 1);
            while (chunkStop < tableData.Length && tableData[chunkStop] != '\n') chunkStop++;
            while (chunkStart != 0 && tableData[chunkStart] != '\n' && chunkStart < chunkStop) chunkStart++;

            var dataLen = chunkStop - chunkStart;
            var localdata = new byte[dataLen];
            Array.Copy(tableData,chunkStart,localdata,0, dataLen);

            var localBuffers = new List<byte[]>(nodeCount);
            var localBufferIndexes = new List<int>(nodeCount);
            for (var i = 0; i < nodeCount; i++)
            {
                localBuffers.Add(new byte[tableData.Length]);
                localBufferIndexes.Add(0);
            }
            
            var lastEnd = 0;
            for (var i = 0; i < dataLen; i++)
            {
                if (localdata[i] == '\n' || i == dataLen - 1)
                {
                    var lineLen = i - lastEnd + (i > 0 && localdata[i - 1] == '\r'
                                      ? -1
                                      : (i == dataLen - 1 && !(localdata[i] == '\r' || localdata[i] == '\n') ? 1 : 0));
                    if (lineLen <= 0)
                    {
                        lastEnd = i + 1;
                        continue;
                    }

                    var line = new byte[lineLen];
                    Array.Copy(localdata, lastEnd, line, 0, lineLen);
                    lastEnd = i + 1;

                    var bufIndex = GetNodeNumberByLine(line, nodeCount, keys);
                    var buf = localBuffers[bufIndex];
                    Array.Copy(line, 0, buf, localBufferIndexes[bufIndex], lineLen);
                    localBufferIndexes[bufIndex] += lineLen;
                    buf[localBufferIndexes[bufIndex]++] = (byte) '\n';
                }
            }

            var resultBuffers = new List<byte[]>(nodeCount);
            for (var i = 0; i < nodeCount; i++)
            {
                var data = new byte[localBufferIndexes[i]];
                Array.Copy(localBuffers[i],data,localBufferIndexes[i]);
                resultBuffers.Add(data);
            }
            localBufferIndexes.Clear();
            localBuffers.Clear();
            
            return resultBuffers;
        }
        
        private static int FindIndex(byte[] line, char sep, int maxEnter)
        {
            if (maxEnter < 0) return line.Length;
            var enter = 0;
            for (var i = 0; i < line.Length; i++)
                if (line[i] == sep)
                {
                    enter++;
                    if (enter >= maxEnter) return i;
                }
            return 0;
        }

        private static List<byte[]> Split(byte[] line, char sep)
        {
            var result = new List<byte[]>();
            var enter = 0;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == sep || i == line.Length - 1)
                {
                    var len = i - enter + (i == line.Length - 1 ? 1 : 0);
                    var buf = new byte[len];
                    Array.Copy(line, enter, buf, 0, len);
                    enter = i + 1;
                    result.Add(buf);
                }
            }
            return result;
        }
    }
}
