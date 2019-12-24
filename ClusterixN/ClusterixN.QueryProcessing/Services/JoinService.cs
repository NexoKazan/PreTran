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
using System.Linq;
using ClusterixN.Common;
using ClusterixN.Common.Data.Query.JoinTree;
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
    // ReSharper disable once ClassNeverInstantiated.Global
    class JoinService : QueryProcessingServiceBase, IJoinService
    {
        private readonly IRelationService _relationService;
        private readonly Action<Relation, Relation, string, RelationSchema, Guid> _processJoinAction;

        public JoinService(ICommunicator client, IRelationService relationService, ICommandService commandService,
            QueryProcessConfig dbConfig) : base(client, dbConfig)
        {
            commandService.Subscribe(this);
            _relationService = relationService;
            Client.SubscribeToPacket<JoinStartPacket>(JoinStartPacketReceivedHandler);
            Client.SubscribeToPacket<MultiNodeJoinStartPacket>(MultiNodeJoinStartPacketReceivedHandler);
            Client.SubscribeToPacket<IntegratedJoinStartPacket>(IntegratedJoinStartPacketReceivedHandler);

            if (relationService is HashRelationService)
            {
                if (ServiceLocator.Instance.ConfigurationService.GetAppSetting("IsManagedJoin") == "1")
                {
                    _processJoinAction = ProcessManagedHashJoin;
                }
                else
                {
                    _processJoinAction = ProcessHashJoin;
                }
            }
            else
            {
                _processJoinAction = ProcessJoin;
            }
        }

        private void ProcessJoin(Relation leftRelation, Relation rightRelation, string query,
            RelationSchema resultSchema, Guid newRelationId)
        {
            using (var joinProcessor = new JoinQueryProcessor(Config, newRelationId, Client, _relationService))
            {
                PauseAction += joinProcessor.Pause;
                StopQueryAction += joinProcessor.StopQuery;
                joinProcessor.Pause(IsPaused);
                joinProcessor.ProcessJoin(leftRelation, rightRelation, query, resultSchema, newRelationId);
                PauseAction -= joinProcessor.Pause;
                StopQueryAction -= joinProcessor.StopQuery;
            }
        }

        private void ProcessManagedHashJoin(Relation leftRelation, Relation rightRelation, string query,
            RelationSchema resultSchema, Guid newRelationId)
        {
            using (var joinProcessor = new ManagedHashJoinQueryProcessor(Config, newRelationId, Client, _relationService))
            {
                PauseAction += joinProcessor.Pause;
                StopQueryAction += joinProcessor.StopQuery;
                joinProcessor.Pause(IsPaused);
                joinProcessor.ProcessJoin(leftRelation, rightRelation, query, resultSchema, newRelationId);
                PauseAction -= joinProcessor.Pause;
                StopQueryAction -= joinProcessor.StopQuery;
            }
        }

        private void ProcessHashJoin(Relation leftRelation, Relation rightRelation, string query,
            RelationSchema resultSchema, Guid newRelationId)
        {
            using (var joinProcessor = new HashJoinQueryProcessor(Config, newRelationId, Client, _relationService))
            {
                PauseAction += joinProcessor.Pause;
                StopQueryAction += joinProcessor.StopQuery;
                joinProcessor.Pause(IsPaused);
                joinProcessor.ProcessJoin(leftRelation, rightRelation, query, resultSchema, newRelationId);
                PauseAction -= joinProcessor.Pause;
                StopQueryAction -= joinProcessor.StopQuery;
            }
        }

        private void ProcessIntegratedJoin(JoinTreeLeaf treeLeaf, RelationSchema resultSchema)
        {
            using (var joinProcessor = new IntegratedJoinQueryProcessor(Config, treeLeaf.RelationId, Client, _relationService))
            {
                PauseAction += joinProcessor.Pause;
                StopQueryAction += joinProcessor.StopQuery;
                joinProcessor.Pause(IsPaused);
                joinProcessor.ProcessIntegratedJoin(treeLeaf, resultSchema);
                PauseAction -= joinProcessor.Pause; 
                StopQueryAction -= joinProcessor.StopQuery;
            }
        }

        private void ProcessMultiNodeHashJoin(Relation leftRelation, Relation rightRelation, MultiNodeJoinStartPacket multiNodeJoinStartPacket)
        {
            using (var joinProcessor = new MultiNodeHashJoinQueryProcessor(Config, multiNodeJoinStartPacket.RelationId, Client, _relationService, multiNodeJoinStartPacket))
            {
                PauseAction += joinProcessor.Pause;
                StopQueryAction += joinProcessor.StopQuery;
                joinProcessor.Pause(IsPaused);
                joinProcessor.ProcessJoin(leftRelation, rightRelation);
                PauseAction -= joinProcessor.Pause;
                StopQueryAction -= joinProcessor.StopQuery;
            }
        }

        private void JoinStartPacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as JoinStartPacket;
            if (packet == null) return;

            var rightRelation = _relationService.GetRelation(packet.RelationRight);
            var leftRelation = _relationService.GetRelation(packet.RelationLeft);
            if (rightRelation != null && leftRelation != null)
            {
                Logger.Trace(
                    $"Подготовка операции JOIN для {leftRelation.RelationId} и {rightRelation.RelationId} {packet.RelationId}");
                StartTask(obj =>
                    {
                        var tup = (Tuple<Relation, Relation, string, RelationSchema, Guid>) obj;
                        _processJoinAction.Invoke(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item5);
                    },
                    new Tuple<Relation, Relation, string, RelationSchema, Guid>(leftRelation, rightRelation,
                        packet.Query, packet.ResultSchema.ToRelationSchema(), packet.RelationId));
            }
            else
            {
                Logger.Error(
                    $"Ни одно из отношений не найдено id = {packet.RelationLeft} и id = {packet.RelationRight}");
            }
        }

        private void MultiNodeJoinStartPacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as MultiNodeJoinStartPacket;
            if (packet == null) return;

            var rightRelation = _relationService.GetRelation(packet.RelationRight);
            var leftRelation = _relationService.GetRelation(packet.RelationLeft);
            if (rightRelation != null && leftRelation != null)
            {
                Logger.Trace(
                    $"Подготовка операции JOIN для {leftRelation.RelationId} и {rightRelation.RelationId} {packet.RelationId}");
                StartTask(obj =>
                    {
                        var tup = (Tuple<Relation, Relation, MultiNodeJoinStartPacket>) obj;
                        ProcessMultiNodeHashJoin(tup.Item1, tup.Item2, tup.Item3);
                    },
                    new Tuple<Relation, Relation, MultiNodeJoinStartPacket>(leftRelation, rightRelation, packet));
            }
            else
            {
                Logger.Error(
                    $"Ни одно из отношений не найдено id = {packet.RelationLeft} и id = {packet.RelationRight}");
            }
        }

        private void IntegratedJoinStartPacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as IntegratedJoinStartPacket;
            if (packet == null) return;
            
            var relationIds = IntegratedJoinQueryProcessor.GetRelationIdsFromJoinTree(packet.JoinTree.ToJoinTreeLeaf());
            var relations = _relationService.GetRelations(relationIds);
            if (relations.All(r => r != null))
            {
                Logger.Trace(
                    $"Подготовка операции JOIN для {string.Join(", ", relations.Select(r => r.RelationName))} {packet.JoinTree.RelationId}");
                StartTask(obj =>
                    {
                        var tup = (Tuple<JoinTreeLeaf, RelationSchema>) obj;
                        ProcessIntegratedJoin(tup.Item1, tup.Item2);
                    },
                    new Tuple<JoinTreeLeaf, RelationSchema>(packet.JoinTree.ToJoinTreeLeaf(),
                        packet.ResultSchema.ToRelationSchema()));
            }
            else
            {
                Logger.Error($"Ни одно из отношений не найдено {string.Join(", ", relationIds)}");
            }
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
