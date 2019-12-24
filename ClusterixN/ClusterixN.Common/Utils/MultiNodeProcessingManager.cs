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
using ClusterixN.Common.Data;

namespace ClusterixN.Common.Utils
{
    public class MultiNodeProcessingManager
    {
        readonly List<SelectTable> _selectTable = new List<SelectTable>();
        private readonly object _syncObject = new object();

        public void AddTask(Guid nodeId, Guid queryId)
        {
            lock (_syncObject)
            {
                _selectTable.Add(new SelectTable(nodeId, queryId));
            }
        }

        public void SetComplete(Guid nodeId, Guid queryId)
        {
            lock (_syncObject)
            {
                var sel = _selectTable.FindIndex(s => s.QueryId == queryId && s.NodeId == nodeId);
                if (sel >= 0)
                {
                    _selectTable[sel].IsComplete = true;
                }
            }
        }

        public bool IsComplete(Guid nodeId, Guid queryId)
        {
            lock (_syncObject)
            {
                return _selectTable.Where(s => s.QueryId == queryId && s.NodeId == nodeId).Any(s=>s.IsComplete);
            }
        }

        public bool Check(Guid queryId)
        {
            lock (_syncObject)
            {
                var sels = _selectTable.Where(s => s.QueryId == queryId).ToList();
                return sels.All(s => s.IsComplete) || !sels.Any();
            }
        }

        public void Remove(Guid queryId)
        {
            lock (_syncObject)
            {
                _selectTable.RemoveAll(s=>s.QueryId == queryId);
            }
        }
    }
}
