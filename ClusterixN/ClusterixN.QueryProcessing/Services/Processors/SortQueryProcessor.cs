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
    internal sealed class SortQueryProcessor : JoinProcessorBase
    {
        public SortQueryProcessor(QueryProcessConfig config, Guid queryId, ICommunicator client,
            IRelationService relationService) : base(config, queryId, client, relationService)
        {
        }


        private string ReplaceFirst(string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private Relation Sort(Relation[] relations, string sortQuery, RelationSchema resultSchema, Guid newRelationId)
        {
            Logger.Trace(
                $"Запуск SORT для {string.Join(",", relations.Select(r => r.RelationId.ToString()).ToArray())}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingSort, Guid.Empty,
                newRelationId,
                Guid.Empty);
            string query = sortQuery;

            foreach (var relation in relations)
            {
                RelationService.SetRelationStatus(relation.RelationId, RelationStatus.Sort);
                query = ReplaceFirst(query, Constants.RelationNameTag, relation.RelationName);
            }

            var newRelation = new Relation
            {
                RelationId = newRelationId,
                RelationOriginalName = string.Join("_", relations.Select(r => r.RelationOriginalName.ToString())),
                Status = RelationStatus.Preparing,
                Shema = resultSchema
            };
            
            Database.CreateRelation(newRelation.RelationName,
                resultSchema.Fields.Select(f => f.Name).ToArray(),
                resultSchema.Fields.Select(f => f.Params).ToArray());
            Database.QueryIntoRelation(newRelation.RelationName, query);
            RelationService.IndexRelation(newRelation.RelationName, resultSchema.Indexes, Database);

            foreach (var relation in relations)
            {
                RelationService.SetRelationStatus(relation.RelationId, RelationStatus.SortComplete);
            }

            newRelation.Status = RelationStatus.DataTransfered;

            timeLog.Stop();
            Logger.Trace(
                $"SORT для {string.Join(", ", relations.Select(r => r.RelationId.ToString()).ToArray())} в {newRelation.RelationName} выполнен за {timeLog.Duration} мс");

            return newRelation;
        }

        public void ProcessSort(List<Relation> relations, string sortQuery, RelationSchema resultSchema, Guid newRelationId)
        {
            Relations.AddRange(relations.Select(r=>r.RelationId));
            Relations.Add(newRelationId);
            var sortRelations = new List<Guid>();
            sortRelations.AddRange(relations.Select(r => r.RelationId));

            var relationService = QueryProcessorServiceLocator.Instance.GetService<RelationService>();
            while (!(relations.All(r => r.Status == RelationStatus.DataTransfered)))
            {
                Thread.Sleep(100);
                if (IsStopped)
                    CleanUp(sortRelations);
                if (relationService.GetRelation(relations.First().RelationId) == null) return;
            }

            WaitPause(); //ждем разрешения продолжения

            if (IsStopped)
                CleanUp(sortRelations);

            var newRelation = Sort(relations.ToArray(), sortQuery, resultSchema, newRelationId);
            relationService.AddRelation(newRelation);
            if (!IsStopped)
            {
                Client.Send(new SortCompletePacket() {NewRelationId = newRelation.RelationId});
            }
            else
            {
                sortRelations.Add(newRelationId);
            }

            CleanUp(sortRelations);
        }
    }
}