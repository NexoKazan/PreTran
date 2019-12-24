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
using System.Threading.Tasks;

namespace ClusterixN.QueryProcessing.Services.Utils
{
    class TaskHelper
    {
        private readonly TaskScheduler _taskScheduler;
        public Guid Id { get; set; }
        public Task Task { get; private set; }
        public CancellationTokenSource TokenSource {
            get;
        }

        public TaskHelper(TaskScheduler taskScheduler)
        {
            _taskScheduler = taskScheduler;
            Id = Guid.NewGuid();
            TokenSource = new CancellationTokenSource();
        }

        public void StartTask(Action<object> action, object obj, Action<Task> callback)
        {
            if (Task!=null)
            {
                Stop();
            }

            Task = new Task(action, obj, TokenSource.Token);
            Task.ContinueWith(callback);
            Task.Start(_taskScheduler);
        }

        public void Stop()
        {
            if (Task != null && !Task.IsCanceled && !Task.IsCompleted)
            {
                TokenSource.Cancel();
                Task.Wait(100);
                Task.Dispose();
                Task = null;
            }
        }
    }
}
