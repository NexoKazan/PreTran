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

namespace ClusterixN.Common.Utils.Task
{
    public class QueueTask
    {
        private readonly Action<int, object> _action;
        private readonly object _data;

        public QueueTask(Action<int, object> action, object data)
        {
            _action = action;
            _data = data;
        }

        public void Invoke(int queueNumber)
        {
            _action.Invoke(queueNumber, _data);
        }
    }
}
