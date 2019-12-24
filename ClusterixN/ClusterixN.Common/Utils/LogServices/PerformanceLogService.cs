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
using System.IO;
using ClusterixN.Common.Data.Log;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Utils.LogServices
{
    public class PerformanceLogService
    {
        private static PerformanceLogService _performanceLogService;
        private static string _module = "NA";
        private static string _loggerName = "NA";
        private static ILogger _logger;
        private readonly PerformanceLogEvent _performanceLogEvent;

        public static PerformanceLogService Instance => _performanceLogService ?? (_performanceLogService = new PerformanceLogService());

        public static void Initialize(string module, string logger, string dbName = "performance.db")
        {
            _module = module;
            _loggerName = logger;
            _logger = ServiceLocator.Instance.LogService.GetLogger(_loggerName);

            try
            {
                File.WriteAllBytes(dbName, Properties.Resources.performance);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        public PerformanceLogService()
        {
            _performanceLogEvent = new PerformanceLogEvent()
            {
                Module = _module
            };
        }

        public void LogPerformance(string counter, double value)
        {
            _performanceLogEvent.Time = SystemTime.Now;
            _performanceLogEvent.Counter = counter;
            _performanceLogEvent.Value = value;
            _logger.LogEvent(_performanceLogEvent);
        }
    }
}
