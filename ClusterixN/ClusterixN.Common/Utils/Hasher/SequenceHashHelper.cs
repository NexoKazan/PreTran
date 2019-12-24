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

﻿using System.Collections.Generic;
using System.Text;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Utils.Hasher
{
    public class SequenceHashHelper : HashHelperBase, IHasher
    {
        public int GetNodeNumberByLine(string line, int nodeCount, int[] keys, int maxField = -1)
        {
            var fields = line.Substring(0, FindIndex(line, '|', maxField)).Split('|');
            var intFields = new int[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                var field = fields[keys[i]];
                intFields[i] = ParseField(field);
            }
            return GetNodeNumber(nodeCount, intFields);
        }
        
        public List<byte[]> ProcessData(byte[] data, int nodeCount, int[] keys)
        {
            var buffers = new List<StringBuilder>(nodeCount);
            for (var i = 0; i < nodeCount; i++)
            {
                buffers.Add(new StringBuilder(data.Length / nodeCount));
            }

            var str = Encoding.UTF8.GetString(data);
            var lines = str.Split('\r','\n');

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length > 0)
                {
                    var buf = buffers[GetNodeNumberByLine(line, nodeCount, keys)];
                    buf.Append(line + '\n');
                }
            }

            var result = new List<byte[]>(nodeCount);
            for (var i = 0; i < nodeCount; i++)
            {
                var dataBlock = Encoding.UTF8.GetBytes(buffers[i].ToString());
               result.Add(dataBlock);
                buffers[i].Clear();
            }

            return result;
        }
        
        private static int FindIndex(string line, char sep, int maxEnter)
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
        
    }
}
