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
using System.IO;
using ClusterixN.Common.Data.Log;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Utils.LogServices
{
    public class TimeLogHelper
    {
        private ILogger _logger;
        private TimeMeasureHelper _timeMeasure;
        private TimeLogEvent _timeLog;

        public double Duration { get { return _timeLog.Duration; } }

        public TimeLogHelper(string module, string logger)
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger(logger);
            _timeMeasure = new TimeMeasureHelper();
            _timeLog = new TimeLogEvent()
            {
                Module = module
            };
        }

        public void StartLogTime(MeasuredOperation operation, Guid queryId, Guid subQueryId, Guid relationId, string @from, string to)
        {
            _timeLog.Operation = operation;
            _timeLog.From = @from;
            _timeLog.To = to;
            _timeLog.QueryId = queryId;
            _timeLog.SubQueryId = subQueryId;
            _timeLog.RelationId = relationId;
            _timeLog.Time = SystemTime.Now;
            _timeMeasure.Start();
        }

        public void Stop()
        {
            _timeMeasure.Stop();
            _timeLog.Duration = _timeMeasure.Elapsed.TotalMilliseconds;
            _logger.LogEvent(_timeLog);
        }

        public static void InitTimeLogDb(string dbName = "timeLog.db")
        {
            File.WriteAllBytes(dbName, Properties.Resources.timeLog);
        }
    }
}
