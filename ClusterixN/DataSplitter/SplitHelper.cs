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
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ClusterixN.Common.Utils.Hasher;
using DataSplitter.Data.EventArgs;

namespace DataSplitter
{
    internal class SplitHelper : IDisposable
    {
        private readonly Dictionary<int, string[]> _buffer = new Dictionary<int, string[]>();
        private readonly Dictionary<int, long> _bufferCounter = new Dictionary<int, long>();
        private readonly Dictionary<int, int> _bufferIndex = new Dictionary<int, int>();
        private readonly int _bufferLenght;
        private readonly SequenceHashHelper _hash;
        private readonly string _inFile;
        private readonly int _nodeCount;
        private readonly Timer _timer;
        long _lineCount;

        public SplitHelper(string filePath, int nodeCount, int bufferLenght = 1000000)
        {
            _inFile = filePath;
            _nodeCount = nodeCount;
            _bufferLenght = bufferLenght;
            _hash = new SequenceHashHelper();
            _timer = new Timer(500);
            _timer.Elapsed += ProcessingUpdate;
            Init(_nodeCount);
        }

        private void ProcessingUpdate(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnProcessingEvent(_lineCount);
        }

        public void Dispose()
        {
            _buffer.Clear();
            _bufferCounter.Clear();
            _bufferIndex.Clear();
            _timer.Dispose();
        }

        public void Process(int[] keyIndexes)
        {
            var maxField = keyIndexes.Max() + 1;
            _timer.Start();
            object syncObj = new object();

            Parallel.ForEach(File.ReadLines(_inFile), line =>
            {
                var currentNode = _hash.GetNodeNumberByLine(line, _nodeCount, keyIndexes, maxField);

                lock (syncObj)
                {
                    Write(_inFile, currentNode, line);
                }
                _lineCount++;
            });

            _timer.Stop();
            FlushBuffer(_inFile);
            OnProcessingCompleteEvent(_lineCount, _bufferCounter);
        }

        private void Init(int nodeCount)
        {
            for (var node = 0; node < nodeCount; node++)
            {
                _buffer.Add(node, new string[_bufferLenght]);
                _bufferIndex.Add(node, 0);
                _bufferCounter.Add(node, 0);
            }
        }

        private void Write(string filePath, int node, string line)
        {
            _bufferCounter[node]++;
            _buffer[node][_bufferIndex[node]++] = line;

            if (_bufferIndex[node] == _bufferLenght)
            {
                File.AppendAllLines(filePath + "_" + node, _buffer[node]);
                _bufferIndex[node] = 0;
            }
        }

        private void FlushBuffer(string filePath)
        {
            foreach (var buf in _buffer)
            {
                var bufferEnd = new List<string>();
                for (var i = 0; i < _bufferIndex[buf.Key]; i++)
                    bufferEnd.Add(buf.Value[i]);
                File.AppendAllLines(filePath + "_" + buf.Key, bufferEnd);
            }
        }

        public event EventHandler<ProcessingEventArgs> ProcessingEvent;
        public event EventHandler<ProcessingCompleteEventArgs> ProcessingCompleteEvent;

        protected virtual void OnProcessingEvent(long count)
        {
            ProcessingEvent?.BeginInvoke(this, new ProcessingEventArgs {ProcessedLines = count}, null, null);
        }

        protected virtual void OnProcessingCompleteEvent(long count, Dictionary<int, long> linesByNode)
        {
            ProcessingCompleteEvent?.Invoke(this,
                new ProcessingCompleteEventArgs {ProcessedLines = count, ProcessedLinesByNodes = linesByNode});
        }
    }
}