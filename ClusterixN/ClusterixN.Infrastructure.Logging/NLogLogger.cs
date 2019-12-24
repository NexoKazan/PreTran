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
using ClusterixN.Common.Interfaces;
using NLog;
using ILogger = ClusterixN.Common.Interfaces.ILogger;

namespace ClusterixN.Infrastructure.Logging
{
    public class NLogLogger
        : ILogger
    {
        private readonly Logger _log;

        public NLogLogger(Logger log)
        {
            _log = log;
        }

        public void Trace(string message)
        {
            _log.Trace(message);
        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Warning(string message)
        {
            _log.Warn(message);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Error(string message, Exception ex)
        {
            _log.Error(ex, message);
        }

        public void Error(Exception ex)
        {
            _log.Error(ex, ex.Message);
        }

        public void Fatal(string message, Exception ex)
        {
            _log.Fatal(ex, message);
        }

        public void Fatal(Exception ex)
        {
            _log.Fatal(ex, ex.Message);
        }

        public void Fatal(string message)
        {
            _log.Fatal(message);
        }

        public void LogEvent(ILogEvent logEvent)
        {
            LogEventInfo l = new LogEventInfo();
            l.Level = LogLevel.Info;
            foreach (var d in logEvent.GetLogEventProperties())
            {
                l.Properties[d.Key] = d.Value;
            }
            _log.Log(l);
        }
    }
}