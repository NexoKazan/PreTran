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
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace ClusterixN.Common.Utils.PerformanceCounters
{
    public sealed class PerformanceMonitor : IDisposable
    {
        private static PerformanceMonitor _performanceMonitor;
        public static PerformanceMonitor Instance => _performanceMonitor ?? (_performanceMonitor = new PerformanceMonitor());

        readonly bool _isRunningOnMono = (Type.GetType("Mono.Runtime") != null);

        public float CpuUsage { get; private set; }
        public float RamAvaible { get; private set; }
        public float NetworkSendSpeed { get; private set; }
        public float NetworkReceiveSpeed { get; private set; }

        private readonly List<ICounter> _counter;
        private readonly List<ICounter> _networkSendCounters;
        private readonly List<ICounter> _networkReceiveCounters;
        private readonly Timer _timer;

        public PerformanceMonitor()
        {
            _networkSendCounters = new List<ICounter>();
            _networkReceiveCounters = new List<ICounter>();
            _counter = new List<ICounter> {new FakeCounter("Processor", "% Processor Time", "_Total", true)};

            if (_isRunningOnMono)
            {
                _counter.Add(new LinuxAvaibleRamCounter());
            }
            else
            {
                _counter.Add(new FakeCounter("Memory", "Available MBytes", true));
            }

            try
            {
                var netifs = GetNetworkInterfacesNames();
                foreach (var netif in netifs)
                {
                    _networkSendCounters.Add(new FakeCounter("Network Interface", "Bytes Sent/sec", netif, true));
                    _networkReceiveCounters.Add(new FakeCounter("Network Interface", "Bytes Received/sec", netif, true));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //ignored
            }

            _timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds) {AutoReset = true};
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            CpuUsage = _counter[0].CurrentValue;
            RamAvaible = _counter[1].CurrentValue;
            NetworkSendSpeed = _networkSendCounters.Count > 0 ? _networkSendCounters.Sum(c => c.CurrentValue) : 0;
            NetworkReceiveSpeed = _networkReceiveCounters.Count > 0 ? _networkReceiveCounters.Sum(c => c.CurrentValue) : 0;
            OnNewValueAvaible();
        }

        public event EventHandler NewValueAvaible;

        private void OnNewValueAvaible()
        {
            NewValueAvaible?.Invoke(this, EventArgs.Empty);
        }

        private string[] GetNetworkInterfacesNames()
        {
            var category = new PerformanceCounterCategory("Network Interface");
            return category.GetInstanceNames();
        }

        /// <summary>Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.</summary>
        public void Dispose()
        {
            _timer?.Dispose();
            _counter.Clear();
        }
    }
}
