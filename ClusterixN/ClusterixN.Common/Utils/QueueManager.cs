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

using System;
using System.Collections;

namespace ClusterixN.Common.Utils
{
    public class QueueManager
    {
        private readonly Queue _functionsQueue;
        private readonly object _syncObject = new object();

        public QueueManager()
        {
            _functionsQueue = new Queue();
        }

        public bool IsEmpty
        {
            get
            {
                lock (_syncObject)
                {
                    return _functionsQueue.Count == 0;
                }
            }
        }

        public bool Contains(Action action)
        {
            lock (_syncObject)
            {
                if (_functionsQueue.Contains(action))
                    return true;
                return false;
            }
        }

        public Action Get()
        {
            lock (_syncObject)
            {
                return _functionsQueue.Dequeue() as Action;
            }
        }

        public void Add(Action function)
        {
            lock (_syncObject)
            {
                _functionsQueue.Enqueue(function);
            }
        }
    }
}