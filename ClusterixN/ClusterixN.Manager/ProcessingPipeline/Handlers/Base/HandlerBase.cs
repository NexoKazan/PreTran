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

using System.Collections.Generic;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Interfaces;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers.Base
{
    internal abstract class HandlerBase : IPipelineNode
    {
        protected IServerCommunicator Server { get; }
        protected IQueryManager QueryManager { get; }
        protected NodesManager NodesManager { get; }
        protected PauseLogManager PauseLogManager { get; }
        protected QueryBufferManager QueryBufferManager { get; }
        protected QueueManager QueueManager { get; }
        protected readonly ILogger Logger;

        protected HandlerBase(IServerCommunicator server, IQueryManager queryManager, NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueryBufferManager queryBufferManager, QueueManager queueManager)
        {
            Server = server;
            QueryManager = queryManager;
            NodesManager = nodesManager;
            PauseLogManager = pauseLogManager;
            QueryBufferManager = queryBufferManager;
            QueueManager = queueManager;
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
        }

        protected string[] GetNodeAddresses(List<Node> nodes)
        {
            var addresses = new List<string>();

            foreach (var node in nodes)
            {
                var address = Server.GetAddressByNodeId(node.Id).Address.ToString();
                addresses.Add($"{address}:{node.ServerPort}");
            }

            return addresses.ToArray();
        }

        public abstract void DoAction();
    }
}
