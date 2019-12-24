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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Database.FastMySQL;

namespace ClusterixN.Database.ParallelFastMySQL
{
    public class Database : FastMySQL.Database, IDatabase
    {
        private readonly LimitedConcurrencyLevelTaskScheduler _taskScheduler;

        public Database()
        {
            var coreCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkCoresCount"));
            _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(coreCount);
        }

        /// <summary>
        ///     Выборка по блокам из БД
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <param name="blockSize">Размер блока в строках</param>
        public new void SelectBlocks(string query, int blockSize)
        {
#if DEBUG
            Logger.Trace(query);
#endif
            try
            {
                var threadCount = _taskScheduler.MaximumConcurrencyLevel;
                int rowCount;
                var page = 0;
                query = query.Replace(";", "");
                do
                {
                    rowCount = 0;
                    WaitPause();
                    if (IsStopSelect)
                    {
                        IsStopSelect = false;
                        return;
                    }

                    var tasks = new Task[threadCount];
                    for (var i = 0; i < threadCount; i++)
                    {
                        tasks[i] = SelectAsync(new SelectTaskInfo()
                        {
                            Query = query,
                            ConnectionString = ConnectionString,
                            StartRow = blockSize * (page + i),
                            BlockSize = blockSize,
                        });
                        tasks[i].Start();
                    }

                    Task.WaitAll(tasks);

                    for (var i = 0; i < threadCount; i++)
                    {
                        var res = ((Task<SelectTaskResult>)tasks[i]).Result;
                        var isLast = res.RowCount != blockSize;
                        OnBlockReaded(res.Result, isLast, orderNumber: page + i);
                        rowCount += res.RowCount;
                        if (isLast) break;
                    }

                    page += threadCount;
                } while (rowCount == blockSize * threadCount);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        private Task<SelectTaskResult> SelectAsync(SelectTaskInfo taskInfo)
        {
            return new Task<SelectTaskResult>(obj => Select((SelectTaskInfo) obj), taskInfo);
        }
        
        private SelectTaskResult Select(SelectTaskInfo selectTaskInfo)
        {
            const int numberOfRetries = 10;
            var rowCount = 0;
            var lenght = 0;
            var selQuery = selectTaskInfo.Query + $" LIMIT {selectTaskInfo.StartRow}, {selectTaskInfo.BlockSize};";
#if DEBUG
            Logger.Trace(selQuery);
#endif
            var handle = Init();
            var isProcessed = false;
            byte[] buffer = null;
            var exp = new Exception();

            for (var i = 1; i <= numberOfRetries; ++i)
            {
                try
                {
                    var buf = FastSelect(handle,
                        ConnectionStringParser.GetAddress(selectTaskInfo.ConnectionString) + ":" +
                        ConnectionStringParser.GetPort(selectTaskInfo.ConnectionString),
                        ConnectionStringParser.GetUser(selectTaskInfo.ConnectionString),
                        ConnectionStringParser.GetPassword(selectTaskInfo.ConnectionString),
                        ConnectionStringParser.GetDatabase(selectTaskInfo.ConnectionString),
                        selQuery, ref lenght, ref rowCount);
                    buffer = new byte[lenght];
                    Marshal.Copy(buf, buffer, 0, buffer.Length);
                    Marshal.FreeHGlobal(buf);
                    isProcessed = true;
                    break;
                }
                catch (Exception ex) when (i <= numberOfRetries)
                {
                    exp = ex;
                    Logger.Warning($"Ошибка обработки запроса c началом в {selectTaskInfo.StartRow}: {ex}. Повтор...");
                    Thread.Sleep(100);
                    DESTROY(handle);
                    handle = Init();
                }
            }
			
			DESTROY(handle);

            if (!isProcessed)
            {
                Logger.Error($"Ошибка обработки запроса c началом в {selectTaskInfo.StartRow}: {exp}");
                throw new Exception($"Ошибка обработки запроса c началом в { selectTaskInfo.StartRow }: {exp}");
            }

            var result = new SelectTaskResult()
            {
                Result = buffer,
                RowCount = rowCount
            };
            return result;
        }

        private class SelectTaskInfo
        {
            public string Query { get; set; }
            public string ConnectionString { get; set; }

            public int StartRow { get; set; }

            public int BlockSize { get; set; }
        }

        private class SelectTaskResult
        {
            public byte[] Result { get; set; }
            public int RowCount { get; set; }
        }
    }
}