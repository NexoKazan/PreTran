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
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Interfaces;
using ClusterixN.QueryProcessing.Services.Utils;

namespace ClusterixN.QueryProcessing.Services.Base
{
    public abstract class ServiceBase : IService
    {
        protected readonly ICommunicator Client;
        protected readonly QueryProcessConfig Config;
        protected readonly ILogger Logger;
        private readonly List<TaskHelper> _tasks;
        private readonly TaskScheduler _taskScheduler;
        private readonly object _taskSync = new object();

        public ServiceBase(ICommunicator client, QueryProcessConfig dbConfig)
        {
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            Client = client;
            Config = dbConfig;
            _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
            _tasks = new List<TaskHelper>();
        }
        
        protected Guid StartTask(Action<object> action, object param)
        {
            var task = new TaskHelper(_taskScheduler); 
            lock (_taskSync)
            {
                _tasks.Add(task);
            }
            task.StartTask(action,param, TaskComplete);
            return task.Id;
        }

        private void TaskComplete(Task task)
        {
            lock (_taskSync)
            {
                TaskHelper t = null;
                foreach (var runningTask in _tasks)
                {
                    if (runningTask.Task == task)
                    {
                        t = runningTask;
                        break;
                    }
                }
                if (t != null)
                {
                    _tasks.Remove(t);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_taskSync)
                {
                    foreach (var task in _tasks)
                    {
                        task.Stop();
                    }
                    _tasks.Clear();
                }
            }
        }

        /// <summary>Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
