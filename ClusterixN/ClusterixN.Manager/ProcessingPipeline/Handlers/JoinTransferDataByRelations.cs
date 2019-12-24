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
    internal class JoinTransferDataByRelations : JoinTransferDataSyncBase
    {
        public JoinTransferDataByRelations(IServerCommunicator server, IQueryManager queryManager,
            QueryBufferManager queryBufferManager, NodesManager nodesManager, PauseLogManager pauseLogManager, QueueManager queueManager) : base(
            server, queryManager, queryBufferManager, nodesManager, pauseLogManager, queueManager)
        {
        }

        private void TransferRelationsToJoin()
        {
            var nodes =
                NodesManager.GetNodes(NodeType.Join)
                    .Where(
                        n => n.CanTransferDataQuery &&
                             n.QueriesInProgress.Any(
                                 q =>
                                     q.JoinQueries.Any(
                                         j =>
                                             j.RightRelation.Status == QueryRelationStatus.Prepared ||
                                             j.LeftRelation.Status == QueryRelationStatus.Prepared)))
                    .ToList();

            if (nodes.Count == 0) return;

            foreach (var node in nodes)
            {
                TransferReadyRelationsToJoin(node, node.QueriesInProgress);
            }
        }
        
        public override void DoAction()
        {
            TransferRelationsToJoin();
        }
    }
}