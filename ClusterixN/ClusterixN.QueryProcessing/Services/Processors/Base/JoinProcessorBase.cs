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
using System.Threading;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Interfaces;

namespace ClusterixN.QueryProcessing.Services.Processors.Base
{
    internal abstract class JoinProcessorBase : QueryProcessorBase
    {
        protected readonly ICommunicator Client;
        protected readonly IRelationService RelationService;
        protected readonly List<Guid> Relations;

        public JoinProcessorBase(QueryProcessConfig config, Guid queryId, ICommunicator client, IRelationService relationService) : base(config, queryId)
        {
            Client = client;
            RelationService = relationService;
            Relations = new List<Guid>();
        }
        
        protected void WaitPause()
        {
            while (IsPaused)
            {
                Thread.Sleep(100);
            }
        }

        protected void CleanUp(List<Guid> relationIds)
        {
            foreach (var relation in RelationService.GetRelations(relationIds.ToArray()))
            {
                if (relation != null)
                    RelationService.DropRealtion(relation);
            }
        }

        protected bool IsPaused { get; set; }
        protected bool IsStopped { get; set; }

        public override void Pause(bool pause)
        {
            IsPaused = pause;
        }

        public override void StopQuery(Guid id)
        {
            if (Relations.Contains(id))
            {
                IsStopped = true;
            }
        }
    }
}