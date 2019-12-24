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
using ClusterixN.Common.Data.Query.JoinTree;
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
    internal sealed class IntegratedJoinQueryProcessor : JoinProcessorBase
    {
        public IntegratedJoinQueryProcessor(QueryProcessConfig config, Guid queryId, ICommunicator client,
            IRelationService relationService) : base(config, queryId, client, relationService)
        {
        }

        private Relation IntegratedJoin(JoinTreeLeaf joinTree, RelationSchema resultSchema)
        {
            var relationIds = GetRelationIdsFromJoinTree(joinTree);
            var relations = RelationService.GetRelations(relationIds);
            Logger.Trace($"Запуск JOIN для {string.Join(", ", relationIds)}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingJoin, Guid.Empty,
                Guid.Empty,
                joinTree.RelationId);

            foreach (var relation in relations)
            {
                RelationService.SetRelationStatus(relation.RelationId, RelationStatus.Join);
            }

            var newRelation = new Relation
            {
                RelationId = joinTree.RelationId,
                RelationOriginalName = $"{string.Join("_", relations.Select(r => r.RelationOriginalName))}",
                Status = RelationStatus.Preparing,
                Shema = resultSchema
            };

            var query = BuildQueryJoinTreeRecursive(joinTree);
            
            Database.CreateRelation(newRelation.RelationName,
                resultSchema.Fields.Select(f => f.Name).ToArray(),
                resultSchema.Fields.Select(f => f.Params).ToArray());
            Database.QueryIntoRelation(newRelation.RelationName, query);

            foreach (var relation in relations)
            {
                RelationService.SetRelationStatus(relation.RelationId, RelationStatus.JoinComplete);
            }

            newRelation.Status = RelationStatus.DataTransfered;

            timeLog.Stop();
            Logger.Trace(
                $"JOIN для {string.Join(", ", relations.Select(r => r.RelationName))} в {newRelation.RelationName} выполнен за {timeLog.Duration} мс");

            return newRelation;
        }

        private string BuildQueryJoinTreeRecursive(JoinTreeLeaf joinTree)
        {
            if (joinTree == null) return null;
            if (string.IsNullOrWhiteSpace(joinTree.Query))
                return RelationService.GetRelation(joinTree.RelationId).RelationName;

            string query = joinTree.Query;
            query = query.Replace(Constants.LeftRelationNameTag, BuildQueryJoinTreeRecursive(joinTree.LeftRelation));
            query = query.Replace(Constants.RightRelationNameTag, BuildQueryJoinTreeRecursive(joinTree.RightRelation));
            return "(" + query + ")";
        }

        public void ProcessIntegratedJoin(JoinTreeLeaf treeLeaf, RelationSchema resultSchema)
        {
            var relationIds = GetRelationIdsFromJoinTree(treeLeaf);
            Relations.AddRange(relationIds);
            Relations.Add(treeLeaf.RelationId);
            var relationStatuses = new RelationStatus[relationIds.Length];
            while (relationStatuses.Any(s => s != RelationStatus.DataTransfered))
            {
                Thread.Sleep(100);
                var rls = RelationService.GetRelations(relationIds);
                relationStatuses = rls.Select(r => r.Status).ToArray();
                if (IsStopped)
                    CleanUp(relationIds.ToList());
            }

            WaitPause(); //ждем разрешения продолжения

            if (IsStopped)
                CleanUp(relationIds.ToList());

            var newRelation = IntegratedJoin(treeLeaf, resultSchema);
            RelationService.AddRelation(newRelation);
            CleanUp(relationIds.ToList()); //удаляем ненужные отношения

            if (!IsStopped)
            {
                Client.Send(new IntegratedJoinCompletePacket()
                {
                    NewRelationId = newRelation.RelationId,
                    Relations = GetRelationIdsFromJoinTree(treeLeaf, false),
                });
            }
            else
            {
                CleanUp(new List<Guid>() { newRelation.RelationId }); //удаляем отменный join
            }

        }

        public static Guid[] GetRelationIdsFromJoinTree(JoinTreeLeaf joinTree, bool onlyBaseRelations = true)
        {
            return GetRelationIdsFromJoinTreeRecursive(joinTree, onlyBaseRelations)
                .Except(new List<Guid>() { joinTree.RelationId })
                .ToArray();
        }

        private static IEnumerable<Guid> GetRelationIdsFromJoinTreeRecursive(JoinTreeLeaf joinTree, bool onlyBaseRelations)
        {
            if (joinTree == null) return null;

            var ids = new List<Guid>();
            if (string.IsNullOrWhiteSpace(joinTree.Query) || !onlyBaseRelations) ids.Add(joinTree.RelationId);
            var leftTree = GetRelationIdsFromJoinTreeRecursive(joinTree.LeftRelation, onlyBaseRelations);
            if (leftTree != null) ids.AddRange(leftTree);
            var rightTree = GetRelationIdsFromJoinTreeRecursive(joinTree.RightRelation, onlyBaseRelations);
            if (rightTree != null) ids.AddRange(rightTree);
            return ids.ToArray();
        }
    }
}