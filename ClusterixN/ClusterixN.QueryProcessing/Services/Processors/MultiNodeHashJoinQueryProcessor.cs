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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;
 using ClusterixN.Common.Utils.Task;
 using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Interfaces;
using ClusterixN.QueryProcessing.Services.Processors.Base;
using Relation = ClusterixN.QueryProcessing.Data.Relation;

namespace ClusterixN.QueryProcessing.Services.Processors
{
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    internal sealed class MultiNodeHashJoinQueryProcessor : JoinProcessorBase
    {
        private readonly MultiNodeJoinStartPacket _startPacket;
        private readonly string[] _joinAddresses;
        private readonly int[] _hashCounts;
        private readonly int _hashCount;
        private int _taskCounter;
        private TaskSequenceHelper _taskSequenceHelper;

        public MultiNodeHashJoinQueryProcessor(QueryProcessConfig config, Guid queryId, ICommunicator client,
            IRelationService relationService, MultiNodeJoinStartPacket startPacket) : base(config, queryId, client, relationService)
        {
            _taskSequenceHelper = new TaskSequenceHelper();
            _startPacket = startPacket;
            _joinAddresses = startPacket.NextNodeAddresses;
            _hashCounts = startPacket.HashCount;
            var hashRelationService = relationService as HashRelationService;
            if (hashRelationService == null)
            {
                throw new Exception($"{nameof(HashJoinQueryProcessor)} может работать только с {nameof(HashRelationService)}, однако инициализирован {relationService.GetType()}");
            }

            _hashCount = hashRelationService.HashCount;
        }

        private void Join(Relation leftRelation, Relation rightRelation, string joinQuery, Relation newRelation)
        {
            Logger.Trace($"Запуск JOIN для {leftRelation.RelationName} и {rightRelation.RelationName}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingJoin, Guid.Empty,
                Guid.Empty,
                newRelation.RelationId);

            RelationService.SetRelationStatus(leftRelation.RelationId, RelationStatus.Join);
            RelationService.SetRelationStatus(rightRelation.RelationId, RelationStatus.Join);


            var tasks = new List<Task>();
            for (var i = 0; i < _hashCount; i++)
            {
                tasks.Add(StartJoin(leftRelation, rightRelation, joinQuery, i, newRelation));
            }

            Task.WaitAll(tasks.ToArray());

            RelationService.SetRelationStatus(leftRelation.RelationId, RelationStatus.JoinComplete);
            RelationService.SetRelationStatus(rightRelation.RelationId, RelationStatus.JoinComplete);

            timeLog.Stop();
            Logger.Trace(
                $"JOIN для {leftRelation.RelationName} и {rightRelation.RelationName} в {newRelation.RelationName} выполнен за {timeLog.Duration} мс");

            CleanUp(new List<Guid>() { leftRelation.RelationId, rightRelation.RelationId }); //удаляем ненужные отношения

            ProcessJoinResult(tasks, newRelation);
            tasks.Clear();
        }
        
        private void ProcessJoinResult(List<Task> tasks, Relation newRelation)
        {
            Logger.Trace($"Отправка результата JOIN {newRelation.RelationId}...");
            var blockIndex = 0;
            var blockCount = tasks.Cast<Task<List<byte[]>>>().Sum(t => t.Result.Count);
#if DEBUG
            Logger.Trace($"Подготовка к отправке {blockCount} блоков...");
#endif
            for (var taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
            {
#if DEBUG
                Logger.Trace($"Начало отправки {newRelation.RelationId}...");
#endif
                var t = (Task<List<byte[]>>)tasks[taskIndex];
                for (var resultIndex = 0; resultIndex < t.Result.Count; resultIndex++)
                {
#if DEBUG
                    Logger.Trace($"Отправка результата {resultIndex+1}/{t.Result.Count}...");
#endif
                    var bytese = t.Result[resultIndex];
                    HashAndSend(new SelectResult()
                    {
                        RelationId = newRelation.RelationId,
                        OrderNumber = blockIndex,
                        Result = bytese,
                        IsLast = blockCount - 1 == blockIndex,
                    }, newRelation);
                    blockIndex++;
                }
                t.Dispose();
            }

            if (blockCount == 0)
            {
                Logger.Warning("Пустой результат join");

                HashAndSend(new SelectResult()
                {
                    RelationId = newRelation.RelationId,
                    OrderNumber = 0,
                    Result = new byte[0],
                    IsLast = true,
                }, newRelation);
            }

            tasks.Clear();
        }

        private void HashAndSend(SelectResult result, Relation relation)
        {
            if (_startPacket.IsLast)
            {
#if DEBUG
                Logger.Trace($"Отправка последнего пакета в SORT...");
#endif
                Client.Send(IPEndPointParser.Parse(_startPacket.NextNodeAddresses.First()),
                    new RelationDataPacket()
                {
                    RelationId = result.RelationId,
                    QueryId = _startPacket.QueryId,
                    SubQueryId = _startPacket.SubQueryId,
                    QueryNumber = _startPacket.QueryNumber,
                    OrderNumber = result.OrderNumber,
                    IsLast = result.IsLast,
                    Data = result.Result,
                    SourceNodeCount = _startPacket.JoinCount 
                });

                if (result.IsLast)
                {
                    Client.Send(new QueryStatusPacket()
                    {
                        RelationId = result.RelationId,
                        QueryId = _startPacket.QueryId,
                        SubQueryId = _startPacket.SubQueryId,
                        QueryNumber = _startPacket.QueryNumber,
                        Status = QueryStatus.TransferedToSort
                    });
                }
            }
            else
            {
#if DEBUG
                Logger.Trace($"Отправка пакета в JOIN...");
#endif
                var hashCount = _hashCounts.Sum();
                _taskCounter++;
#if DEBUG
                Logger.Trace($"Запуск хеширования...");
#endif
                HashHelper.Instance.HashDataAsync(relation, result.Result, hashCount, SendHashedDataAsync, result);
            }
        }

        private void SendHashedDataAsync(List<byte[]> hashedData, object obj)
        {
            _taskSequenceHelper.AddTask(new Task(o =>
            {
                var tup = (Tuple<List<byte[]>, object>)o;
                SendHashedData(tup.Item1, tup.Item2);
            }, new Tuple<List<byte[]>, object>(hashedData, obj)));
        }

        private void SendHashedData(List<byte[]> hashedData, object obj)
        {
#if DEBUG
            Logger.Trace($"Хеширование завершено.");
#endif
            var selectResult = (SelectResult)obj;
            var coreIndex = 0;

            for (var i = 0; i < _joinAddresses.Length; i++)
            {
                for (var j = 0; j < _hashCounts[i]; j++)
                {
#if DEBUG
                    Logger.Trace($"Отправка хешированного результата {j+1}/{_hashCounts[i]} в {i+1}/{_joinAddresses.Length}");
#endif
                    Client.Send(IPEndPointParser.Parse(_joinAddresses[i]),
                        new HashedRelationDataPacket()
                        {
                            RelationId = selectResult.RelationId,
                            QueryId = _startPacket.QueryId,
                            SubQueryId = _startPacket.SubQueryId,
                            QueryNumber = _startPacket.QueryNumber,
                            Data = hashedData[coreIndex++],
                            HashNumber = j,
                            OrderNumber = selectResult.OrderNumber * _hashCounts[i] + j,
                            IsLast = selectResult.IsLast && _hashCounts[i] == j + 1,
                            SourceNodeCount = _startPacket.JoinCount
                        });
                }
            }

            if (selectResult.IsLast)
            {
                Client.Send(new QueryStatusPacket()
                {
                    RelationId = selectResult.RelationId,
                    QueryId = _startPacket.QueryId,
                    SubQueryId = _startPacket.SubQueryId,
                    QueryNumber = _startPacket.QueryNumber,
                    Status = QueryStatus.JoinProcessed
                });
            }
            else
            {
                Client.Send(new QueryStatusPacket()
                {
                    RelationId = selectResult.RelationId,
                    QueryId = _startPacket.QueryId,
                    SubQueryId = _startPacket.SubQueryId,
                    QueryNumber = _startPacket.QueryNumber,
                    Status = QueryStatus.JoinProcessing
                });
            }
            _taskCounter--;
        }

        private Task<List<byte[]>> StartJoin(Relation leftRelation, Relation rightRelation, string joinQuery, 
            int hashNumber, Relation newRelation)
        {
            var task = new Task<List<byte[]>>(JoinTask,
                new Tuple<Relation, Relation, string, int, Relation>(leftRelation, rightRelation,
                    joinQuery, hashNumber, newRelation));
            task.Start();
            return task;
        }

        private List<byte[]> JoinTask(object obj)
        {
            var tup = (Tuple<Relation, Relation, string, int, Relation>) obj;

            var leftRelation = tup.Item1;
            var rightRelation = tup.Item2;
            var joinQuery = tup.Item3;
            var hashNumber = tup.Item4;
            var newRelation = tup.Item5;

            var database = ServiceLocator.Instance.DatabaseService.GetDatabase(DbConfig.ConnectionString, newRelation.QueryId.ToString(), true);
            var query = joinQuery.Replace(Constants.LeftRelationNameTag, leftRelation.RelationName + "_" + hashNumber)
                .Replace(Constants.RightRelationNameTag, rightRelation.RelationName + "_" + hashNumber);
            return database.Select(query, DbConfig.BlockLength);
        }

        private void CreateHashedRelation(Relation newRelation)
        {
            using (var database = ServiceLocator.Instance.DatabaseService.GetDatabase(DbConfig.ConnectionString, newRelation.QueryId.ToString(), true))
            {
                for (var i = 0; i < _hashCount; i++)
                {
                    CreateRelation(newRelation, i, database, newRelation.Shema);
                }
            }
        }

        private void CreateRelation(Relation newRelation, int hashNumber, IDatabase database, RelationSchema resultSchema)
        {
            var relationName = newRelation.RelationName + "_" + hashNumber;
            database.CreateRelation(relationName,
                resultSchema.Fields.Select(f => f.Name).ToArray(),
                resultSchema.Fields.Select(f => f.Params).ToArray());
            RelationService.IndexRelation(relationName, resultSchema.Indexes, database);
        }

        public void ProcessJoin(Relation leftRelation, Relation rightRelation)
        {
            Relations.Add(leftRelation.RelationId);
            Relations.Add(rightRelation.RelationId);
            Relations.Add(_startPacket.RelationId);

            var newRelation = RelationService.GetRelation(_startPacket.RelationId);
            if (newRelation == null && _startPacket.IsLast)
            {
                newRelation = new Relation()
                {
                    RelationId = _startPacket.RelationId,
                    RelationOriginalName = $"{leftRelation.RelationOriginalName}_{rightRelation.RelationOriginalName}",
                    Status = RelationStatus.Preparing,
                    Shema = _startPacket.ResultSchema.ToRelationSchema()
                };
                CreateHashedRelation(newRelation);
                RelationService.AddRelation(newRelation);
            }

            var joinRelations = new List<Guid> {leftRelation.RelationId, rightRelation.RelationId};

            var rightRelationStatus = rightRelation.Status;
            var leftRelationStatus = leftRelation.Status;
            while (rightRelationStatus != RelationStatus.DataTransfered ||
                   leftRelationStatus != RelationStatus.DataTransfered)
            {
                Thread.Sleep(100);
                var rl = RelationService.GetRelation(rightRelation.RelationId);
                var ll = RelationService.GetRelation(leftRelation.RelationId);
                if (IsStopped)
                    CleanUp(joinRelations);
                if (rl == null || ll == null)
                {
                    Logger.Error($"Нет отношений {rightRelation.RelationId} и {leftRelation.RelationId}");
                    return;
                }
                rightRelationStatus = rl.Status;
                leftRelationStatus = ll.Status;
            }

            WaitPause(); //ждем разрешения продолжения

            if (IsStopped)
                CleanUp(joinRelations);

            Join(leftRelation, rightRelation, _startPacket.Query, newRelation);

            while (_taskCounter > 0) //ждем завершения хеширования и отправки
            {
                Thread.Sleep(100);
            }

            if (!IsStopped)
            {
                Client.Send(new JoinCompletePacket()
                {
                    NewRelationId = newRelation.RelationId,
                    LeftRelationId = leftRelation.RelationId,
                    RightRelationId = rightRelation.RelationId,
                    QueryId = _startPacket.QueryId,
                    SubQueryId = _startPacket.SubQueryId,
                    RelationId = _startPacket.RelationId
                });
            }
            else
            {
                CleanUp(new List<Guid>() { _startPacket.RelationId }); //удаляем отменный join
            }

            if (_startPacket.IsLast) CleanUp(new List<Guid>() {_startPacket.RelationId});
        }

        protected override void Dispose(bool disposing)
        {
            _taskSequenceHelper.Dispose();

            base.Dispose(disposing);
        }
    }
}