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
 using System.Threading.Tasks;
 using ClusterixN.Common;
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
    internal sealed class HashSelectQueryProcessor : QueryProcessorBase
    {
        private readonly IRelationService _relationService;
        private Guid _queryId;
        private Guid _subQueryId;
        private Guid _relationId;
        private int _orderNumber;
        private readonly int _hashCount;

        public HashSelectQueryProcessor(QueryProcessConfig config, Guid queryId, IRelationService relationService) : base(config, queryId)
        {
            _relationService = relationService;
            Database.BlockReaded += DatabaseOnBlockReaded;

            var hashRelationService = relationService as HashRelationService;
            if (hashRelationService == null)
            {
                throw new Exception($"{nameof(HashSelectQueryProcessor)} может работать только с {nameof(HashRelationService)}, однако инициализирован {relationService.GetType()}");
            }

            _hashCount = hashRelationService.HashCount;
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
                    IsLast = false,
                    OrderNumber = _orderNumber++
                }
            });
        }

        private void SendEndPacket()
        {
            SendSelectResult(new SimpleEventArgs<SelectResult>()
            {
                Value = new SelectResult()
                {
                    QueryId = _queryId,
                    SubQueryId = _subQueryId,
                    RelationId = _relationId,
                    Result = new byte[0],
                    IsLast = true,
                    OrderNumber = _orderNumber++
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

            Logger.Trace($"Запущена обработка запроса {packet.SubQueryId}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingSelect, Guid.Empty, packet.SubQueryId,
                Guid.Empty);

            _queryId = packet.QueryId;
            _subQueryId = packet.SubQueryId;
            _relationId = packet.RelationId;
            for (var i = 0; i < _hashCount; i++)
            {
                var query = packet.Query.Replace(Constants.RelationNameTag, relation.RelationName + "_" + i);
                Database.SelectBlocks(query, DbConfig.BlockLength);
            }
            SendEndPacket();

            timeLog.Stop();
            Logger.Trace($"Завершена обработка запроса {packet.SubQueryId} за {timeLog.Duration} мс");
        }

        public void StartParallelQueryProcess(QueryPacket packet)
        {
            var relation = _relationService.GetRelation(packet.RelationId);

            Logger.Trace($"Запущена обработка запроса {packet.SubQueryId}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.ProcessingSelect, Guid.Empty, packet.SubQueryId,
                Guid.Empty);

            _queryId = packet.QueryId;
            _subQueryId = packet.SubQueryId;
            _relationId = packet.RelationId;
            Parallel.For(0, _hashCount, i =>
            {
                var query = packet.Query.Replace(Constants.RelationNameTag, relation.RelationName + "_" + i);
                using (var database = ServiceLocator.Instance.DatabaseService.GetDatabase(DbConfig.ConnectionString, (relation.RelationName + "_" + i).ToString()))
                {
                    database.BlockReaded += DatabaseOnBlockReaded;
                    database.SelectBlocks(query, DbConfig.BlockLength);
                }
            });
            SendEndPacket();

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