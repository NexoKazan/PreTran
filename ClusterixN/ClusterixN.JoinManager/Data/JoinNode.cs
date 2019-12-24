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
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;

namespace ClusterixN.JoinManager.Data
{
    class JoinNode : Node
    {
        private readonly object _syncObj = new object();
        private readonly Dictionary<Guid, RelationStatus> _status;

        public JoinNode(Guid id, bool useHardDisk, float minRamAvaible) : base(id, useHardDisk, minRamAvaible)
        {
            _status = new Dictionary<Guid, RelationStatus>();
        }

        public new Dictionary<Guid, RelationStatus> Status
        {
            get
            {
                lock (_syncObj)
                {
                    return new Dictionary<Guid, RelationStatus>(_status);
                }
            }
        }

        public void AddRelation(Guid relationId)
        {
            lock (_syncObj)
            {
                if (!_status.ContainsKey(relationId))
                {
                    _status.Add(relationId, RelationStatus.Preparing);
                }

            }
        }

        public void SetRelationStatus(Guid relationId, RelationStatus status)
        {
            lock (_syncObj)
            {
                if (_status.ContainsKey(relationId))
                {
                    _status[relationId] = status;
                }
            }
        }

        public void RemoveRelationStatus(Guid relationId)
        {
            lock (_syncObj)
            {
                if (_status.ContainsKey(relationId)) _status.Remove(relationId);
            }
        }
    }
}
