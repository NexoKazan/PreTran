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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Interfaces;
using ClusterixN.QueryProcessing.Services.Processors.Base;
using Relation = ClusterixN.QueryProcessing.Data.Relation;

namespace ClusterixN.QueryProcessing.Services.Processors
{
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    internal sealed class HashJoinQueryProcessor : JoinProcessorBase
    {
        private readonly int _hashCount;
        private readonly Action<List<Task>, Relation> _processJoinResultAction;

        public HashJoinQueryProcessor(QueryProcessConfig config, Guid queryId, ICommunicator client,
            IRelationService relationService) : base(config, queryId, client, relationService)
        {
            var hashRelationService = relationService as HashRelationService;
            if (hashRelationService == null)
            {
                throw new Exception($"{nameof(HashJoinQueryProcessor)} может работать только с {nameof(HashRelationService)}, однако инициализирован {relationService.GetType()}");
            }

            _hashCount = hashRelationService.HashCount;
            _processJoinResultAction = hashRelationService.MergeBlocksIntoOneFile ? (Action<List<Task>, Relation>) ProcessJoinResultInOneFile : ProcessJoinResult;
        }

        private Relation Join(Relation leftRelation, Relation rightRelation, string joinQuery,
            RelationSchema resultSchema, Guid newRelationId)
        {
            Logger.Trace($"Запуск JOIN для {leftRelation.RelationName} и {rightRelation.RelationName}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingJoin, Guid.Empty,
                Guid.Empty,
                newRelationId);

            RelationService.SetRelationStatus(leftRelation.RelationId, RelationStatus.Join);
            RelationService.SetRelationStatus(rightRelation.RelationId, RelationStatus.Join);

            var newRelation = new Relation
            {
                RelationId = newRelationId,
                RelationOriginalName = $"{leftRelation.RelationOriginalName}_{rightRelation.RelationOriginalName}",
                Status = RelationStatus.Preparing,
                Shema = resultSchema
            };

            var tasks = new List<Task>();
            for (var i = 0; i < _hashCount; i++)
            {
                tasks.Add(StartJoin(leftRelation, rightRelation, joinQuery, resultSchema, i, newRelation));
            }

            Task.WaitAll(tasks.ToArray());

            RelationService.SetRelationStatus(leftRelation.RelationId, RelationStatus.JoinComplete);
            RelationService.SetRelationStatus(rightRelation.RelationId, RelationStatus.JoinComplete);
            CleanUp(new List<Guid>() { leftRelation.RelationId, rightRelation.RelationId }); //удаляем ненужные отношения

            timeLog.Stop();
            Logger.Trace(
                $"JOIN для {leftRelation.RelationName} и {rightRelation.RelationName} в {newRelation.RelationName} выполнен за {timeLog.Duration} мс");

            Logger.Trace($"Запуск подготовки данных для {newRelation.RelationId}...");
            RelationService.AddRelation(newRelation);
            _processJoinResultAction.Invoke(tasks, newRelation);

            tasks.Clear();

            return newRelation;
        }

        private void ProcessJoinResultInOneFile(List<Task> tasks, Relation newRelation)
        {
            var relationService = (HashRelationService) RelationService;
            string dir = string.Empty;
            for (var taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
            {
                var t = (Task<List<byte[]>>) tasks[taskIndex];
                for (var resultIndex = 0; resultIndex < t.Result.Count; resultIndex++)
                {
                    var bytese = t.Result[resultIndex];
                    dir = relationService.SaveInOneFile(bytese, newRelation);
                }
                t.Dispose();
            }
            if (dir != string.Empty)
                relationService.LoadRelation(newRelation, dir, true, 0);
        }

        private void ProcessJoinResult(List<Task> tasks, Relation newRelation)
        {
            var relationService = (HashRelationService)RelationService;
            var blockIndex = 0;
            var blockCount = tasks.Cast<Task<List<byte[]>>>().Sum(t => t.Result.Count);
            for (var taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
            {
                var t = (Task<List<byte[]>>)tasks[taskIndex];
                for (var resultIndex = 0; resultIndex < t.Result.Count; resultIndex++)
                {
                    var bytese = t.Result[resultIndex];
                    var dir = relationService.SaveFile(bytese, blockIndex, newRelation);
                    relationService.LoadRelation(newRelation, dir, blockCount - 1 == blockIndex, blockIndex);
                    blockIndex++;
                }
                t.Dispose();
            }

            tasks.Clear();
        }

        private Task<List<byte[]>> StartJoin(Relation leftRelation, Relation rightRelation, string joinQuery, RelationSchema resultSchema,
            int hashNumber, Relation newRelation)
        {
            var task = new Task<List<byte[]>>(JoinTask,
                new Tuple<Relation, Relation, string, RelationSchema, int, Relation>(leftRelation, rightRelation,
                    joinQuery, resultSchema, hashNumber, newRelation));
            task.Start();
            return task;
        }

        private List<byte[]> JoinTask(object obj)
        {
            var tup = (Tuple<Relation, Relation, string, RelationSchema, int, Relation>) obj;

            var leftRelation = tup.Item1;
            var rightRelation = tup.Item2;
            var joinQuery = tup.Item3;
            var resultSchema = tup.Item4;
            var hashNumber = tup.Item5;
            var newRelation = tup.Item6;

            var database = ServiceLocator.Instance.DatabaseService.GetDatabase(DbConfig.ConnectionString, newRelation.QueryId.ToString(), true);
            var query = joinQuery.Replace(Constants.LeftRelationNameTag, leftRelation.RelationName + "_" + hashNumber)
                .Replace(Constants.RightRelationNameTag, rightRelation.RelationName + "_" + hashNumber);
            var relationName = newRelation.RelationName + "_" + hashNumber;
            database.CreateRelation(relationName,
                resultSchema.Fields.Select(f => f.Name).ToArray(),
                resultSchema.Fields.Select(f => f.Params).ToArray());
            RelationService.IndexRelation(relationName, resultSchema.Indexes, database);
            return database.Select(query, DbConfig.BlockLength);
        }

        public void ProcessJoin(Relation leftRelation, Relation rightRelation, string query,
            RelationSchema resultSchema, Guid newRelationId)
        {
            Relations.Add(leftRelation.RelationId);
            Relations.Add(rightRelation.RelationId);
            Relations.Add(newRelationId);

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

            var newRelation = Join(leftRelation, rightRelation, query, resultSchema, newRelationId);
            WaitLoad(newRelation);

            if (!IsStopped)
            {
                Client.Send(new JoinCompletePacket()
                {
                    NewRelationId = newRelation.RelationId,
                    LeftRelationId = leftRelation.RelationId,
                    RightRelationId = rightRelation.RelationId
                });
            }
            else
            {
                CleanUp(new List<Guid>() { newRelationId }); //удаляем отменный join
            }
        }

        private void WaitLoad(Relation relation)
        {
            var relationStatus = relation.Status;
            while (relationStatus != RelationStatus.DataTransfered)
            {
                Thread.Sleep(100);
                var r = RelationService.GetRelation(relation.RelationId);
                if (IsStopped)
                    return;
                if (r == null)
                {
                    Logger.Error($"Нет отношения {relation.RelationId}");
                    return;
                }
                relationStatus = r.Status;
            }
        }
    }
}