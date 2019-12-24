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
using System.Threading;
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
    internal sealed class JoinQueryProcessor : JoinProcessorBase
    {
        public JoinQueryProcessor(QueryProcessConfig config, Guid queryId, ICommunicator client,
            IRelationService relationService) : base(config, queryId, client, relationService)
        {
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

            var query = joinQuery.Replace(Constants.LeftRelationNameTag, leftRelation.RelationName)
                .Replace(Constants.RightRelationNameTag, rightRelation.RelationName);

            Database.CreateRelation(newRelation.RelationName,
                resultSchema.Fields.Select(f => f.Name).ToArray(),
                resultSchema.Fields.Select(f => f.Params).ToArray());
            RelationService.IndexRelation(newRelation.RelationName, resultSchema.Indexes, Database);
            Database.QueryIntoRelation(newRelation.RelationName, query);

            RelationService.SetRelationStatus(leftRelation.RelationId, RelationStatus.JoinComplete);
            RelationService.SetRelationStatus(rightRelation.RelationId, RelationStatus.JoinComplete);
            newRelation.Status = RelationStatus.DataTransfered;

            timeLog.Stop();
            Logger.Trace(
                $"JOIN для {leftRelation.RelationName} и {rightRelation.RelationName} в {newRelation.RelationName} выполнен за {timeLog.Duration} мс");

            return newRelation;
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
            RelationService.AddRelation(newRelation);
            CleanUp(joinRelations); //удаляем ненужные отношения

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
    }
}