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
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.EventArgs;
using ClusterixN.Common.Data.EventArgs.Base;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Network.Packets;
using ClusterixN.QueryProcessing.Services.Processors.Base;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Interfaces;

namespace ClusterixN.QueryProcessing.Services.Processors
{
    internal sealed class SelectQueryProcessor : QueryProcessorBase
    {
        private readonly IRelationService _relationService;
        private Guid _queryId;
        private Guid _subQueryId;
        private Guid _relationId;

        public SelectQueryProcessor(QueryProcessConfig config, Guid queryId, IRelationService relationService) : base(config, queryId)
        {
            _relationService = relationService;
            Database.BlockReaded += DatabaseOnBlockReaded;
        }
        
        private void DatabaseOnBlockReaded(object sender, SelectResultEventArg simpleEventArgs)
        {
            SendSelectResult(new SimpleEventArgs<SelectResult>()
            {
                Value = new SelectResult()
                {
                    QueryId = _queryId,
                    SubQueryId = _subQueryId,
                    RelationId = _relationId,
                    Result = simpleEventArgs.Result,
                    IsLast = simpleEventArgs.IsLast,
                    OrderNumber = simpleEventArgs.OrderNumber
                }
            });
        }

        private void SendSelectResult(SimpleEventArgs<SelectResult> e)
        {
            OnBlockReaded?.Invoke(this, e);
        }

        public void StartQueryProcess(QueryPacket packet)
        {
            var relation = _relationService.GetRelation(packet.RelationId);
            if (relation != null)
                packet.Query = packet.Query.Replace(Constants.RelationNameTag, relation.RelationName);

            Logger.Trace($"Запущена обработка запроса {packet.SubQueryId}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingSelect, Guid.Empty, packet.SubQueryId,
                Guid.Empty);

            _queryId = packet.QueryId;
            _subQueryId = packet.SubQueryId;
            _relationId = packet.RelationId;
            Database.SelectBlocks(packet.Query, DbConfig.BlockLength);

            timeLog.Stop();
            Logger.Trace($"Завершена обработка запроса {packet.SubQueryId} за {timeLog.Duration} мс");
        }

        public override void Pause(bool pause)
        {
            Database.ControlSelectBlocks(pause);
        }
        
        public override void StopQuery(Guid obj)
        {
            if (_subQueryId == obj)
            {
                Database.StopSelectQuery();
            }
        }
        
        #region Events

        public event EventHandler<SimpleEventArgs<SelectResult>> OnBlockReaded;

        #endregion
    }
}