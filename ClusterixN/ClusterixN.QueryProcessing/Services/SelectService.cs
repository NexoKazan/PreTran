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
using ClusterixN.Common.Data.EventArgs.Base;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Base;
using ClusterixN.QueryProcessing.Services.Interfaces;
using ClusterixN.QueryProcessing.Services.Processors;

namespace ClusterixN.QueryProcessing.Services
{
    internal class SelectService : QueryProcessingServiceBase, ISelectService
    {
        protected IRelationService RelationService;
        private readonly Action<QueryPacket> _processSelectAction;

        public SelectService(ICommunicator client, IRelationService relationService, ICommandService commandService,
            QueryProcessConfig dbConfig) : base(client, dbConfig)
        {
            commandService.Subscribe(this);
            RelationService = relationService;
            Client.SubscribeToPacket<QueryPacket>(QueryPacketReceived);

            if (relationService is HashRelationService)
            {
                _processSelectAction = HashSelectProcess;
            }
            else
            {
                _processSelectAction = SelectProcess;
            }
        }

        public void Pause(bool pause)
        {
            IsPaused = pause;
        }

        public void CancelQuery(Guid queryId, Guid subQueryId, Guid relationId)
        {
            OnStopQuery(subQueryId);
        }

        private void QueryPacketReceived(PacketBase packetBase)
        {
            var packet = packetBase as QueryPacket;
            if (packet != null)
                StartTask(obj => { _processSelectAction.Invoke((QueryPacket) obj); }, packet);
        }

        private void HashSelectProcess(QueryPacket packet)
        {
            using (var qh = new HashSelectQueryProcessor(Config, packet.QueryId, RelationService))
            {
                PauseSelect += qh.Pause;
                StopQuery += qh.StopQuery;
                qh.OnBlockReaded += SendSelectResult;
                qh.Pause(IsPaused);
                qh.StartQueryProcess(packet);
                PauseSelect -= qh.Pause;
                StopQuery -= qh.StopQuery;
            }
        }

        private void SelectProcess(QueryPacket packet)
        {
            using (var qh = new SelectQueryProcessor(Config, packet.QueryId, RelationService))
            {
                PauseSelect += qh.Pause;
                StopQuery += qh.StopQuery;
                qh.OnBlockReaded += SendSelectResult;
                qh.Pause(IsPaused);
                qh.StartQueryProcess(packet);
                PauseSelect -= qh.Pause;
                StopQuery -= qh.StopQuery;
            }
        }

        private void SendSelectResult(object sender, SimpleEventArgs<SelectResult> selectResultArg)
        {
            var queryBlockResult = selectResultArg.Value;

            Logger.Trace(
                $"Готов блок данных для {queryBlockResult.SubQueryId} {queryBlockResult.QueryId}");

            Client.SendAsyncQueue(new SelectResult
            {
                QueryId = queryBlockResult.QueryId,
                SubQueryId = queryBlockResult.SubQueryId,
                RelationId = queryBlockResult.RelationId,
                OrderNumber = queryBlockResult.OrderNumber,
                IsLast = queryBlockResult.IsLast,
                Result = queryBlockResult.Result
            });

            if (queryBlockResult.IsLast)
            {
                var relation = RelationService.GetRelation(queryBlockResult.RelationId);
                if (relation != null)
                    RelationService.DropRealtion(relation);
            }
        }

        protected override void OnIsPausedChanged(bool isPaused)
        {
            OnPauseSelect(IsPaused);
        }

        #region Events

        protected event Action<Guid> StopQuery;

        protected event Action<bool> PauseSelect;

        private void OnPauseSelect(bool obj)
        {
            PauseSelect?.Invoke(obj);
        }

        private void OnStopQuery(Guid obj)
        {
            StopQuery?.Invoke(obj);
        }

        #endregion
    }
}