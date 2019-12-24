#region Copyright
/*
 * Copyright 2019 Roman Klassen
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
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Infrastructure.Logging
{
    /// <summary>
    ///     Реализация сервиса логгирования для отладочного вывода.
    ///     Выводит все сообщения в консоль.
    /// </summary>
    public class DefaultLogService
        : ILogService
    {
        public ILogger GetLogger(string source)
        {
            return new DefaultLogger(source);
        }

        public ILogger GetLogger(Type sourceType)
        {
            return new DefaultLogger(sourceType.ToString());
        }

        public ILogger GetLogger(Type sourceType, string instanceName)
        {
            var loggerName = $"{sourceType}[{instanceName}]";
            return new DefaultLogger(loggerName);
        }
    }
}