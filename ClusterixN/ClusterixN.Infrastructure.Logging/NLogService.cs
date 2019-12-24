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
using ClusterixN.Common.Interfaces;
using NLog;
using ILogger = ClusterixN.Common.Interfaces.ILogger;

namespace ClusterixN.Infrastructure.Logging
{
    /// <summary>
    ///     Реализация сервиса логгирования на основании библиотеки Log4Net.
    /// </summary>
    public class NLogService
        : ILogService
    {
        /// <summary>
        ///     Справочник логгировщиков для различных источников.
        /// </summary>
        private readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();

        public ILogger GetLogger(string source)
        {
            return new NLogLogger(GetLog(source));
        }

        public ILogger GetLogger(Type sourceType)
        {
            return new NLogLogger(GetLog(sourceType.ToString()));
        }

        public ILogger GetLogger(Type sourceType, string instanceName)
        {
            var loggerName = string.Format("{0}[{1}]", sourceType, instanceName);
            return new NLogLogger(GetLog(loggerName));
        }

        private Logger GetLog(string name)
        {
            lock (this)
            {
                if (_loggers.ContainsKey(name))
                    return _loggers[name];
                var logger = LogManager.GetLogger(name);
                _loggers.Add(name, logger);
                return logger;
            }
        }
    }
}