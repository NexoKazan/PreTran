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
using System.Globalization;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;

namespace ClusterixN.Infrastructure.Logging
{
    public class SystemDebugLogger
        : ILogger
    {
        private readonly string _source;

        public SystemDebugLogger(string source)
        {
            _source = source;
        }

        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine("{0} DEBUG {1} - {2}", GetTimeString(), _source, message);
        }

        public void Info(string message)
        {
            System.Diagnostics.Debug.WriteLine("{0} INFO {1} - {2}", GetTimeString(), _source, message);
        }

        public void Warning(string message)
        {
            System.Diagnostics.Debug.WriteLine("{0} WARNING {1} - {2}", GetTimeString(), _source, message);
        }

        public void Error(string message)
        {
            System.Diagnostics.Debug.WriteLine("{0} ERROR {1} - {2}", GetTimeString(), _source, message);
        }

        public void Error(string message, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("{0} ERROR {1} - {4}\r\n{2}{3}", GetTimeString(), _source, ex,
                ex.Message, message);
        }

        public void Error(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("{0} ERROR {1}\r\n{2}{3}", GetTimeString(), _source, ex, ex.Message);
        }

        public void Trace(string message)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("{0} DEBUG {1} - {2}", GetTimeString(), _source, message));
        }

        public void Fatal(string message, Exception ex)
        {
            Error(message, ex);
        }

        public void Fatal(Exception ex)
        {
            Error(ex);
        }

        public void LogEvent(ILogEvent logEvent)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message)
        {
            Error(message);
        }

        private static string GetTimeString()
        {
            return SystemTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }
    }
}