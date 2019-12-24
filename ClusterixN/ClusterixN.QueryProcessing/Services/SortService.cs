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
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Base;
using ClusterixN.QueryProcessing.Services.Interfaces;
using ClusterixN.QueryProcessing.Services.Processors;
using Relation = ClusterixN.QueryProcessing.Data.Relation;

namespace ClusterixN.QueryProcessing.Services
{
    class SortService : QueryProcessingServiceBase, ISortService
    {
        private readonly IRelationService _relationService;

        public SortService(ICommunicator client, IRelationService relationService, ICommandService commandService, QueryProcessConfig dbConfig) : base(client, dbConfig)
        {
            commandService.Subscribe(this);
            _relationService = relationService;
            Client.SubscribeToPacket<SortStartPacket>(SortStartPacketReceivedHandler);
        }
        
        private void ProcessSort(Tuple<List<Relation>, string, RelationSchema, Guid> tup)
        {
            using (var sortProcessor = new SortQueryProcessor(Config, tup.Item1.First().QueryId, Client, _relationService))
            {
                PauseAction += sortProcessor.Pause;
                StopQueryAction += sortProcessor.StopQuery;
                sortProcessor.Pause(IsPaused);
                sortProcessor.ProcessSort(tup.Item1, tup.Item2, tup.Item3, tup.Item4);
                PauseAction -= sortProcessor.Pause;
                StopQueryAction -= sortProcessor.StopQuery;
            }
        }
        
        private void SortStartPacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as SortStartPacket;
            if (packet == null) return;
            var relationService = QueryProcessorServiceLocator.Instance.GetService<RelationService>();

            var relations = new List<Relation>();
            foreach (var realtionId in packet.RelationIds)
            {
                var relation = relationService.GetRelation(realtionId);
                if (relation == null) Logger.Error($"Нет отношения {realtionId}");
                relations.Add(relation);
            }

            Logger.Trace(
                $"Подготовка операции SORT для {string.Join(".", relations.Select(r => r.RelationId.ToString()))} в {packet.RelationId}");
            StartTask(obj =>
                {
                    var tup = (Tuple<List<Relation>, string, RelationSchema, Guid>) obj;
                    ProcessSort(tup);
                },
                new Tuple<List<Relation>, string, RelationSchema, Guid>(relations, packet.Query,
                    packet.ResultSchema.ToRelationSchema(), packet.RelationId));
        }

        public void Pause(bool pause)
        {
            IsPaused = pause;
        }

        public void CancelQuery(Guid queryId, Guid subQueryId, Guid relationId)
        {
            OnStopQuery(relationId);
        }

        protected override void OnIsPausedChanged(bool isPaused)
        {
            OnPauseSelect(IsPaused);
        }

        #region Events

        private event Action<Guid> StopQueryAction;

        private event Action<bool> PauseAction;

        protected virtual void OnPauseSelect(bool obj)
        {
            PauseAction?.Invoke(obj);
        }

        protected virtual void OnStopQuery(Guid obj)
        {
            StopQueryAction?.Invoke(obj);
        }

        #endregion
    }
}
