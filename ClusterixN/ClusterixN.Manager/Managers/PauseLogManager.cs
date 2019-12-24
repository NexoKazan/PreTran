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
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Utils.LogServices;

namespace ClusterixN.Manager.Managers
{
    internal class PauseLogManager
    {
        private readonly Dictionary<string, TimeLogHelper> _logs;
        private readonly object _syncObject = new object();

        public PauseLogManager()
        {
            _logs = new Dictionary<string, TimeLogHelper>();
        }

        private string CalcKey(Guid nodeId, Guid relationId)
        {
            return $"{nodeId}_{relationId}";
        }

        public bool PauseNode(Guid nodeId, Guid relationId)
        {
            lock (_syncObject)
            {
                var key = CalcKey(nodeId, relationId);
                if (!_logs.ContainsKey(key))
                {
                    _logs.Add(key,TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.Pause, Guid.Empty, Guid.Empty, relationId));
                    return true;
                }
            }
            return false;
        }

        public void ResumeNode(Guid nodeId, Guid relationId)
        {
            lock (_syncObject)
            {
                var key = CalcKey(nodeId, relationId);
                if (_logs.ContainsKey(key))
                {
                    _logs[key].Stop();
                    _logs.Remove(key);
                }
            }
        }
    }
}