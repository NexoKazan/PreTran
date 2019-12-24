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
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Handlers.Base;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class SelectOneByOne : SelectBase
    {
        private readonly int _concurentQueriesCount;

        public SelectOneByOne(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, queryBufferManager, nodesManager, pauseLogManager,  queueManager)
        {
            _concurentQueriesCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("SelectQueriesCount"));
        }

        private void StartSelect()
        {
            var nodes = NodesManager.GetNodes(NodeType.Io);

            if (nodes.Count == 0 || nodes.Any(n => !n.CanSendQuery))
            {
                foreach (var node in nodes)
                {
                    if ((node.Status & (uint)NodeStatus.Full) > 0) continue;
                    if ((node.Status & (uint)NodeStatus.LowMemory) > 0)
                        PauseLogManager.PauseNode(node.Id, Guid.Empty);
                }
                return;
            }

            foreach (var node in nodes)
                PauseLogManager.ResumeNode(node.Id, Guid.Empty);

            ProcessIoNodes(nodes);
        }
        
        private void ProcessIoNodes(List<Node> nodes)
        {
            var existQuery = GetNodesSharedExistQuery(nodes);

            if (existQuery.Count > 0)
            {
                var sended = false;
                foreach (var query in existQuery)
                {
                    if (!nodes.All(n => n.CanSendQuery)) break;
                    sended |= SendExitsQueryToIoNode(nodes, query);
                }
                if (!sended && nodes.All(n => n.CanSendQuery) && nodes.All(n=>n.QueriesInProgress.Count < _concurentQueriesCount))
                    SendNewQueryToIoNode(nodes);
            }
            else
            {
                SendNewQueryToIoNode(nodes);
            }
        }

        public override void DoAction()
        {
            StartSelect();
        }
    }
}