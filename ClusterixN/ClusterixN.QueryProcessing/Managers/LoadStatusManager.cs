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
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Data.Enums;
using ClusterixN.QueryProcessing.Data.EventArgs;

namespace ClusterixN.QueryProcessing.Managers
{
    public class LoadStatusManager
    {
        private readonly Dictionary<Guid, LoadStatus> _loadStatuses;
        private readonly object _syncObject = new object();
        private ILogger _logger;

        public LoadStatusManager()
        {
            _loadStatuses = new Dictionary<Guid, LoadStatus>();
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
        }

        public void StartLoad(Guid relation, Guid taskId, int order, int nodeCount)
        {
            lock (_syncObject)
            {
                if (!_loadStatuses.ContainsKey(relation))
                {
                    _loadStatuses.Add(relation, new LoadStatus(nodeCount));
                }
                _loadStatuses[relation].SetStatus(taskId, LoadStatusEnum.Loading, order);
            }
        }
        
        public void EndLoad(Guid relation, Guid taskId, int order, bool isLast, int hashCount = 1)
        {
            lock (_syncObject)
            {
                if (!_loadStatuses.ContainsKey(relation))
                {
                    _logger.Error($"Не найдена задача загрузки для {relation}");
                    return;
                }

                _loadStatuses[relation].SetStatus(taskId, LoadStatusEnum.Complete, order, isLast);

                if (_loadStatuses[relation].CheckStatuses(LoadStatusEnum.Complete) &&
                    _loadStatuses[relation].IsLastPacketReceived &&
                    _loadStatuses[relation].CheckOrder())
                {
                    var eventArg = new LoadCompleteEventArg()
                    {
                        RelationId = relation,
                        SendNodeCount = _loadStatuses[relation].NodeCount,
                        HashCount = hashCount
                    };
                    LoadComplete(relation);
                    OnLoadComplete(eventArg);
                }
            }
        }
        public void CancelLoad(Guid relation)
        {
            lock (_syncObject)
            {
                if (!_loadStatuses.ContainsKey(relation))
                {
                    _logger.Error($"Не найдена задача для {relation}");
                    return;
                }

                LoadComplete(relation);
            }
        }

        private void LoadComplete(Guid relation)
        {
            lock (_syncObject)
            {
                if (_loadStatuses.ContainsKey(relation)) _loadStatuses.Remove(relation);
            }
        }
        
        public event EventHandler<LoadCompleteEventArg> LoadCompleteEvent;

        protected virtual void OnLoadComplete(LoadCompleteEventArg e)
        {
            LoadCompleteEvent?.BeginInvoke(this, e, null, null);
        }
    }
}