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
using System.Linq;
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.JoinManager.Data;

namespace ClusterixN.JoinManager
{
    public sealed class HashedBufferManager
    {
        private readonly Dictionary<Guid, List<HashedQueryBuffer>> _buffer;
        private readonly object _syncObject = new object();
        private readonly ILogger _logger;

        public HashedBufferManager()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _buffer = new Dictionary<Guid, List<HashedQueryBuffer>>();
        }

        public void AddData(HashedQueryBuffer queryBuffer)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryBuffer.QueryId))
                    _buffer[queryBuffer.QueryId].Add(queryBuffer);
                else
                    _buffer.Add(queryBuffer.QueryId, new List<HashedQueryBuffer> { queryBuffer });
            }
        }
        
        public List<HashedQueryBuffer> GetQueryBuffer(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                    return _buffer[queryId].OrderBy(b=>b.OrderNumber).ToList();
                
                _logger.Error("Нет данных для Id = " + queryId);
                return new List<HashedQueryBuffer>();
            }
        }

        public bool CheckReady(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                {
                    var buf = _buffer[queryId];
                    var max = buf.Max(b => b.OrderNumber);
                    var min = buf.Min(b => b.OrderNumber);
                    var count = buf.Count - 1;
                    return buf.Any(b => b.IsLast) && 
                        (max - min == count && min == 0);
                }

                return false;
            }
        }

        public void MarkLastPacket(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                {
                    _buffer[queryId] = RecalcOrder(_buffer[queryId]);
                    var buf = _buffer[queryId].OrderBy(b => b.OrderNumber).Last();
                    buf.IsLast = true;
                }
                else
                {
                    _logger.Error("Нет данных для Id = " + queryId);
                }
            }
        }

        private List<HashedQueryBuffer> RecalcOrder(List<HashedQueryBuffer> buffers)
        {
            var index = 0;
            foreach (var buffer in buffers)
            {
                buffer.OrderNumber = index++;
                buffer.IsLast = false;
            }
            return buffers;
        }

        public void RemoveData(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                {
                    _buffer.Remove(queryId);
                    GC.Collect();
                }
                else
                {
                    _logger.Error("Нет данных для Id = " + queryId);
                }
            }
        }
    }
}