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
using System.Threading;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Data;

namespace ClusterixN.QueryProcessing.Services.Base
{
    public abstract class QueryProcessingServiceBase : ServiceBase
    {
        private bool _isPaused;
        private TimeLogHelper _pauseTimeLog;

        public QueryProcessingServiceBase(ICommunicator client, QueryProcessConfig dbConfig) : base(client, dbConfig)
        {
        }

        protected bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if (value != _isPaused)
                {
                    _isPaused = value;
                    if (_isPaused)
                    {
                            Logger.Warning("Ожидание разрешения работы...");
                        _pauseTimeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.Pause, Guid.Empty,
                            Guid.Empty, Guid.Empty);
                    }
                    else
                    {
                        _pauseTimeLog.Stop();
                    }
                    OnIsPausedChanged(_isPaused);
                }
            }
        }

        protected void WaitPause()
        {
            while (IsPaused)
            {
                Thread.Sleep(100);
            }
        }

        protected abstract void OnIsPausedChanged(bool isPaused);
    }
}
