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
using System.Threading;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Utils.Task
{
    public class TaskSequenceHelper : IDisposable
    {
        private readonly Thread _thread;
        private readonly object _newTasksSyncObject = new object();
        private readonly List<System.Threading.Tasks.Task> _tasks;
        private readonly List<System.Threading.Tasks.Task> _newTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        public TaskSequenceHelper()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _cancellationTokenSource = new CancellationTokenSource();
            _tasks = new List<System.Threading.Tasks.Task>();
            _newTasks = new List<System.Threading.Tasks.Task>();
            _thread = new Thread(Process);
            _thread.Start(_cancellationTokenSource.Token);
        }

        private void Process(object o)
        {
            if (!(o is CancellationToken)) return;
            var token = (CancellationToken) o;

            while (!token.IsCancellationRequested)
            {
                var task = GetTask();
                if (task != null)
                {
                    try
                    {
                        task.Start();
                        task.Wait(_cancellationTokenSource.Token);
                        task.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Ошибка последовательного выполнения" + ex);
                    }

                    RemoveTask(task);
                }
                else
                {
                    if (AddTasksToQueu() == 0)
                        token.WaitHandle.WaitOne(100);
                }
            }
        }

        private System.Threading.Tasks.Task GetTask()
        {
            if (_tasks.Count > 0)
            {
                if (_tasks[0] == null)
                {
                    _logger.Error("TASK IS NULL!!!");
                    _tasks.Remove(_tasks[0]);
                    return null;
                }

                return _tasks[0];
            }
            return null;
        }

        private void RemoveTask(System.Threading.Tasks.Task task)
        {
            _tasks.Remove(task);
        }

        private int AddTasksToQueu()
        {
            List<System.Threading.Tasks.Task> newTasks;

            lock (_newTasksSyncObject)
            {
                newTasks = _newTasks.ToList();
                _newTasks.Clear();
            }

            var count = newTasks.Count;
            if (count > 0)
            {
                _tasks.AddRange(newTasks);
            }
            return count;
        }

        public void AddTask(System.Threading.Tasks.Task task)
        {
            lock (_newTasksSyncObject)
            {
                _newTasks.Add(task);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            if (!_thread.Join(1000))
            {
                _thread.Abort();
            }
            foreach (var task in _tasks)
            {
                task.Dispose();
            }
            _tasks.Clear();
            foreach (var task in _newTasks)
            {
                task.Dispose();
            }
            _newTasks.Clear();
        }
    }
}
