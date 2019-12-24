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
using System.Threading.Tasks;
using ClusterixN.Common.Utils.Task;

namespace ClusterixN.QueryProcessing.Managers
{
    public class TaskSequenceLoadManager
    {
        private readonly Dictionary<Guid, TaskSequenceHelper> _taskSequenceHelpers;
        private readonly object _syncObject = new object();

        public TaskSequenceLoadManager()
        {
            _taskSequenceHelpers = new Dictionary<Guid, TaskSequenceHelper>();
        }

        public void Add(Guid realtionId, Task task)
        {
            lock (_syncObject)
            {
                if (_taskSequenceHelpers.ContainsKey(realtionId))
                {
                    _taskSequenceHelpers[realtionId].AddTask(task);
                }
                else
                {
                    _taskSequenceHelpers.Add(realtionId, new TaskSequenceHelper());
                    _taskSequenceHelpers[realtionId].AddTask(task);
                }
            }
        }
        
        public void Remove(Guid realtionId)
        {
            lock (_syncObject)
            {
                if (_taskSequenceHelpers.ContainsKey(realtionId))
                {
                    var task = _taskSequenceHelpers[realtionId];
                    _taskSequenceHelpers.Remove(realtionId);
                    task.Dispose();
                }
            }
        }
    }
}