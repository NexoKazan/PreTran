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
    public class SizeLogService
    {
        private static SizeLogService _sizeLogService;
        private static string _module = "NA";
        private static string _loggerName = "NA";
        private static ILogger _logger;
        private readonly SizeLogEvent _sizeLogEvent;

        public static SizeLogService Instance => _sizeLogService ?? (_sizeLogService = new SizeLogService());

        public static void Initialize(string module, string logger, string dbName = "size.db")
        {
            _module = module;
            _loggerName = logger;
            _logger = ServiceLocator.Instance.LogService.GetLogger(_loggerName);

            try
            {
                File.WriteAllBytes(dbName, Properties.Resources.size);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        public SizeLogService()
        {
            _sizeLogEvent = new SizeLogEvent()
            {
                Module = _module
            };
        }

        public void LogSize(Guid relationId, string relationName, long size)
        {
            _sizeLogEvent.Time = SystemTime.Now;
            _sizeLogEvent.RelationId = relationId;
            _sizeLogEvent.RelationName = relationName;
            _sizeLogEvent.Size = size;
            _logger.LogEvent(_sizeLogEvent);
        }
    }
}
