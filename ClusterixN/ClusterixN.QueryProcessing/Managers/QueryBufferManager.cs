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
using ClusterixN.Common;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Interfaces;
using ClusterixN.QueryProcessing.Data;

namespace ClusterixN.QueryProcessing.Managers
{
    /// <summary>
    /// Реализация буфера в памяти
    /// </summary>
    public class QueryBufferManager
    {
        private readonly string _bufferDir;
        private readonly Dictionary<Guid, List<QueryBuffer>> _buffer;
        private readonly object _syncObject = new object();
        private readonly ILogger _logger;

        /// <summary>
        /// Буфер данных в памяти
        /// </summary>
        /// <param name="bufferDir">директория для сброса буфера на диск в случае нехватки памяти</param>
        public QueryBufferManager(string bufferDir)
        {
            _bufferDir = bufferDir;
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _buffer = new Dictionary<Guid, List<QueryBuffer>>();
        }

        /// <summary>
        /// Добавление блока данных
        /// </summary>
        /// <param name="queryBuffer">блок данных</param>
        public void AddData(QueryBuffer queryBuffer)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryBuffer.QueryId))
                    _buffer[queryBuffer.QueryId].Add(queryBuffer);
                else
                    _buffer.Add(queryBuffer.QueryId, new List<QueryBuffer> { queryBuffer });
            }
        }


        /// <summary>
        /// Получение списка буферов
        /// </summary>
        /// <param name="queryId">Идентификатор отношения</param>
        /// <returns>Список буферов</returns>
        public List<QueryBuffer> GetQueryBuffer(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                {
                    var diskBufs = _buffer[queryId].Where(b => b.IsFlushedToDisk);
                    foreach (var diskBuf in diskBufs)
                    {
                        ReadFromDisk(diskBuf);
                    }
                    return _buffer[queryId].OrderBy(b=>b.OrderNumber).ToList();
                }
                
                _logger.Error("Нет данных для Id = " + queryId);
                return new List<QueryBuffer>();
            }
        }

        /// <summary>
        /// Проверка готовности данных
        /// </summary>
        /// <param name="queryId">Идентификатор отношения</param>
        /// <returns></returns>
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

        /// <summary>
        /// Пометка последнего пакета как завершающего
        /// </summary>
        /// <param name="queryId">Идентификатор отношения</param>
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

        private List<QueryBuffer> RecalcOrder(List<QueryBuffer> buffers)
        {
            var index = 0;
            foreach (var buffer in buffers)
            {
                buffer.OrderNumber = index++;
            }
            return buffers;
        }

        /// <summary>
        /// удаление данных из буфера
        /// </summary>
        /// <param name="queryId"></param>
        public void RemoveData(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                {
                    var diskBufs = _buffer[queryId].Where(b => b.IsFlushedToDisk);
                    foreach (var diskBuf in diskBufs)
                    {
                        RemoveFromDisk(diskBuf);
                    }
                    _buffer.Remove(queryId);
                    GC.Collect();
                }
                else
                {
                    _logger.Error("Нет данных для Id = " + queryId);
                }
            }
        }

        /// <summary>
        /// Удаление данных
        /// </summary>
        /// <param name="query">запрос</param>
        public void RemoveData(Query query)
        {
            lock (_syncObject)
            {
                foreach (var selectQuery in query.SelectQueries)
                    RemoveData(selectQuery.QueryId);

                foreach (var joinQuery in query.JoinQueries)
                    RemoveData(joinQuery.QueryId);

                RemoveData(query.SortQuery.QueryId);

                GC.Collect();
            }
        }
        
        /// <summary>
        /// Удаление блока данных
        /// </summary>
        /// <param name="buf">блок данных</param>
        public void RemoveBlock(QueryBuffer buf)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(buf.QueryId))
                {
                    var diskBufs = _buffer[buf.QueryId].Where(b => b.IsFlushedToDisk);
                    foreach (var diskBuf in diskBufs)
                    {
                        RemoveFromDisk(diskBuf);
                    }
                    _buffer[buf.QueryId].Remove(buf);
                    GC.Collect();
                }
                else
                {
                    _logger.Error("Нет данных для Id = " + buf.QueryId);
                }
            }
        }

        /// <summary>
        /// Сброс части буферов на диск
        /// </summary>
        public void FlushBlockToDisk()
        {
            lock (_syncObject)
            {
                foreach (var buf in _buffer)
                {
                    var qbuf = buf.Value.LastOrDefault(b => b.IsFlushedToDisk == false);
                    if (qbuf != null)
                    {
                        SaveToDisk(qbuf);
                    }
                }
            }
        }

        private void SaveToDisk(QueryBuffer buffer)
        {
            if (buffer.IsFlushedToDisk) return;
            try
            {
                File.WriteAllBytes(GetFullFileName(buffer), buffer.Data);
                buffer.IsFlushedToDisk = true;
                buffer.Data = null;
                GC.Collect();
            }
            catch (Exception e)
            {
                _logger.Error("Ошибка сброса буфера на диск", e);
            }
        }

        private void ReadFromDisk(QueryBuffer buffer)
        {
            try
            {
                buffer.Data = File.ReadAllBytes(GetFullFileName(buffer));
                RemoveFromDisk(buffer);
                buffer.IsFlushedToDisk = false;
            }
            catch (Exception e)
            {
                _logger.Error("Ошибка чтения буфера с диска", e);
            }
        }

        private void RemoveFromDisk(QueryBuffer buffer)
        {
            try
            {
                File.Delete(GetFullFileName(buffer));
            }
            catch (Exception e)
            {
                _logger.Error("Ошибка удаления буфера с диска", e);
            }
        }

        private string GetFullFileName(QueryBuffer buffer)
        {
            return $"{_bufferDir}{Path.DirectorySeparatorChar}{buffer.BufferId}";
        }

        /// <summary>
        /// Размер данных для отношения
        /// </summary>
        /// <param name="queryId">Идентификатор отношения</param>
        /// <returns>размер в байтах</returns>
        public long GetBufferLenght(Guid queryId)
        {
            lock (_syncObject)
            {
                if (_buffer.ContainsKey(queryId))
                {
                    var memorySize = _buffer[queryId]
                        .Aggregate<QueryBuffer, long>(0, (current, b) => current + (b.Data?.Length ?? 0));
                    long diskSize = 0;
                    var diskBufs = _buffer[queryId].Where(b => b.IsFlushedToDisk);
                    foreach (var diskBuf in diskBufs)
                    {
                        diskSize += new FileInfo(GetFullFileName(diskBuf)).Length;
                    }
                    return memorySize + diskSize;
                }
                else
                {
                    _logger.Error("Нет данных для Id = " + queryId);
                }
            }
            return 0;
        }
    }
}