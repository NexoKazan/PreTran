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
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Base;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Handlers.Base;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class TimeoutProcessing : HandlerBase
    {
        private readonly ILogger _dropLogger;
        private readonly TimeSpan _timeoutTimeSpan;

        public TimeoutProcessing(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            _dropLogger = ServiceLocator.Instance.LogService.GetLogger("dropLogger");
            _timeoutTimeSpan = TimeSpan.FromMinutes(double.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("TimeoutMinutes")));
        }

        private void ProcessTimeout()
        {
            var queries = QueryManager.GetAllQueries().ToList();
            foreach (var query in queries)
            {
                if (IsSubqueryTimeout(query.SelectQueries) || IsSubqueryTimeout(query.JoinQueries) ||
                    IsSubqueryTimeout(new List<TimeMeasureQueryBase>() { query.SortQuery }))
                {
                    DropQuery(query);
                }
            }
        }

        private bool IsSubqueryTimeout(IEnumerable<TimeMeasureQueryBase> queries)
        {
            foreach (var selectQuery in queries.Where(s => s.Status > QueryStatus.Wait))
            {
                if (selectQuery.CurrentStatusDuration > _timeoutTimeSpan) return true;
            }
            return false;
        }

        private void DropQuery(Query query)
        {
            _dropLogger.Info($"QueryId={query.Id}");
            DropQueryFromBuffer(query);
            DropQueryFromNodes(query);
            QueryManager.DeleteQuery(query);
        }

        private void DropQueryFromBuffer(Query query)
        {
            QueryBufferManager.RemoveData(query);
        }

        private void DropQueryFromNodes(Query query)
        {
            foreach (var node in NodesManager.GetNodes())
            {
                var nodeQuery = node.QueriesInProgress.FirstOrDefault(q => q.Id == query.Id);
                if (nodeQuery != null)
                {
                    node.QueriesInProgress.Remove(nodeQuery);
                    node.QueryInProgress--;
                }

                var dropQueryPacket = new DropQueryPacket()
                {
                    Id = new Identify() { ClientId = node.Id },
                    QueryId = query.Id,
                };

                switch (node.NodeType)
                {
                    case NodeType.Io:
                        foreach (var selectQuery in query.SelectQueries)
                        {
                            dropQueryPacket.SubQueryId = selectQuery.QueryId;
                            dropQueryPacket.RelationId = Guid.Empty;

                            Server.Send(dropQueryPacket);
                        }
                        break;
                    case NodeType.Join:
                        foreach (var joinQuery in query.JoinQueries)
                        {
                            dropQueryPacket.SubQueryId = joinQuery.QueryId;
                            dropQueryPacket.RelationId = joinQuery.QueryId;
                            Server.Send(dropQueryPacket);

                            dropQueryPacket.RelationId = joinQuery.LeftRelation.RelationId;
                            Server.Send(dropQueryPacket);

                            dropQueryPacket.RelationId = joinQuery.RightRelation.RelationId;
                            Server.Send(dropQueryPacket);
                        }
                        break;
                    case NodeType.Sort:
                        dropQueryPacket.SubQueryId = query.SortQuery.QueryId;
                        dropQueryPacket.RelationId = Guid.Empty;
                        Server.Send(dropQueryPacket);
                        foreach (var relation in query.SortQuery.SortRelation)
                        {
                            dropQueryPacket.RelationId = relation.RelationId;
                            Server.Send(dropQueryPacket);
                        }
                        break;
                    case NodeType.Mgm:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void DoAction()
        {
            ProcessTimeout();
        }
    }
}