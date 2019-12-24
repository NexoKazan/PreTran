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

﻿using System.Linq;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Handlers.Base;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class CleanUp : HandlerBase
    {
        public CleanUp(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
        }

        private void Clean()
        {
            DeleteCompletedQueryFromIo();
            DeleteCompletedQueryFromJoin();
            DeleteCompletedQuery();
        }

        private void DeleteCompletedQuery()
        {
            QueryManager.RemoveCompletedQueries();
        }

        private void DeleteCompletedQueryFromJoin()
        {
            var nodes = NodesManager.GetNodes(NodeType.Join)
                .Where(n => n.QueriesInProgress.Any(q => q.Status >= QueryProcessStatus.JoinComplete))
                .ToList();

            if (nodes.Count == 0) return;

            foreach (var node in nodes)
            {
                var completedQueries =
                    node.QueriesInProgress.Where(q => q.Status >= QueryProcessStatus.JoinComplete).ToList();

                foreach (var completedQuery in completedQueries)
                {
                    node.QueriesInProgress.Remove(completedQuery);
                }
            }
        }

        private void DeleteCompletedQueryFromIo()
        {
            var nodes = NodesManager.GetNodes(NodeType.Io)
                .Where(n => n.QueriesInProgress.Any(q => q.SelectQueries.All(s => s.Status >= QueryStatus.SelectProcessed)))
                .ToList();

            if (nodes.Count == 0) return;

            foreach (var node in nodes)
            {
                var completedQueries =
                    node.QueriesInProgress.Where(q => q.SelectQueries.All(s => s.Status >= QueryStatus.SelectProcessed)).ToList();

                foreach (var completedQuery in completedQueries)
                {
                    node.QueriesInProgress.Remove(completedQuery);
                }
            }
        }

        public override void DoAction()
        {
            Clean();
        }
    }
}