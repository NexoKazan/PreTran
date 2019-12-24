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
    public class ParallelQeueHelper : IDisposable
    {
        private readonly int _queueCount;
        private readonly Thread _thread;
        private readonly object _newTasksSyncObject = new object();
        private readonly List<QueueTask> _tasks;
        private readonly List<System.Threading.Tasks.Task> _tasksInRun;
        private readonly List<QueueTask> _newTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly List<int> _queueNumbers;

        public ParallelQeueHelper(int queueCount) : this()
        {
            _queueCount = queueCount;
            _tasksInRun = new List<System.Threading.Tasks.Task>(_queueCount);
            _queueNumbers = new List<int>(_queueCount);

            for (int i = 0; i < _queueCount; i++)
            {
                _tasksInRun.Add(null);
                _queueNumbers.Add(i);
            }

            _thread = new Thread(Process);
            _thread.Start(_cancellationTokenSource.Token);
        }

        public ParallelQeueHelper(int[] queueNumbers) : this()
        {
            _queueCount = queueNumbers.Length;
            _tasksInRun = new List<System.Threading.Tasks.Task>(_queueCount);
            _queueNumbers = new List<int>(_queueCount);

            for (int i = 0; i < _queueCount; i++)
            {
                _tasksInRun.Add(null);
                _queueNumbers.Add(queueNumbers[i]);
            }

            _thread = new Thread(Process);
            _thread.Start(_cancellationTokenSource.Token);
        }

        private ParallelQeueHelper()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _cancellationTokenSource = new CancellationTokenSource();
            _tasks = new List<QueueTask>();
            _newTasks = new List<QueueTask>();
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
                        ProcessTask(task);
                        var tasksToWait = _tasksInRun.Where(t => t != null).ToArray();
                        System.Threading.Tasks.Task.WaitAny(tasksToWait);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Ошибка распределения очередей" + ex);
                    }

                    RemoveTask(task);
                }
                else
                {
                    if (AddTasksToQueue() == 0)
                        token.WaitHandle.WaitOne(100);
                }
            }
        }

        private void ProcessTask(QueueTask task)
        {
            for (int i = 0; i < _queueCount; i++)
            {
                if (_tasksInRun[i] != null)
                {
                    var runTask = _tasksInRun[i];
                    if (runTask.IsCompleted)
                    {
                        runTask.Dispose();
                        _tasksInRun[i] = RunQueueTask(task, _queueNumbers[i]);
                        break;
                    }
                    else if (runTask.IsFaulted)
                    {
                        _logger.Error("Ошибка выполнения в очереди" + runTask.Exception);
                    }
                }
                else
                {
                    _tasksInRun[i] = RunQueueTask(task, i);
                    break;
                }
            }
        }

        private System.Threading.Tasks.Task RunQueueTask(QueueTask queueTask, int queueNumber)
        {
            var task = new System.Threading.Tasks.Task(obj =>
            {
                queueTask.Invoke((int)obj);
            }, queueNumber);
            task.Start();
            return task;
        }

        private void RemoveTask(QueueTask task)
        {
            _tasks.Remove(task);
        }

        private QueueTask GetTask()
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
        
        private int AddTasksToQueue()
        {
            List<QueueTask> newTasks;

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

        public void AddToQueue(QueueTask action)
        {
            lock (_newTasksSyncObject)
            {
                _newTasks.Add(action);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            if (!_thread.Join(1000))
            {
                _thread.Abort();
            }
            _tasks.Clear();
            _newTasks.Clear();
        }
    }
}
