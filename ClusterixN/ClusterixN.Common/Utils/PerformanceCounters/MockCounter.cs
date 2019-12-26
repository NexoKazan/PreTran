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
using System.Diagnostics;

namespace ClusterixN.Common.Utils.PerformanceCounters
{
    class MockCounter : ICounter
    {
        private readonly int _min;
        private readonly int _max;
        private Random _random;

        protected MockCounter()
        {
            _random = new Random();
        }

        public MockCounter(int min, int max) : this()
        {
            _min = min;
            _max = max;
        }
        
        public float CurrentValue => _random.Next(_min,_max);
    }
}
