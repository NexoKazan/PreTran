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
using ClusterixN.Common.Data.Log.Enum;

namespace ClusterixN.Common.Utils.LogServices
{
    public class TimeLogService
    {
        private static TimeLogService _timeLogService;
        private static string _module = "NA";
        private static string _logger = "NA";
        private static string _from = string.Empty;

        public static TimeLogService Instance => _timeLogService ?? (_timeLogService = new TimeLogService());

        public static void Initialize(string module, string logger, string from = "")
        {
            _module = module;
            _logger = logger;
            _from = from;
        }

        public TimeLogHelper GeTimeLogHelper()
        {
            return new TimeLogHelper(_module, _logger);
        }

        public TimeLogHelper GeTimeLogHelper(MeasuredOperation operation, Guid queryId, Guid subQueryId, Guid relationId, string @from, string to)
        {
            var log =  new TimeLogHelper(_module, _logger);
            log.StartLogTime(operation, queryId, subQueryId, relationId, @from, to);
            return log;
        }

        public TimeLogHelper GeTimeLogHelper(MeasuredOperation operation, Guid queryId, Guid subQueryId, Guid relationId)
        {
            var log = new TimeLogHelper(_module, _logger);
            log.StartLogTime(operation, queryId, subQueryId, relationId, _from, "");
            return log;
        }
    }
}
