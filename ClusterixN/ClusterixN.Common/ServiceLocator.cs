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
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
 using ClusterixN.Common.Infrastructure.Logging;
 using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common
{
    /// <summary>
    ///     Реализация паттерна ServiceLocator <see cref="http://martinfowler.com/articles/injection.html" />
    ///     Использует паттерн Singleton.
    /// </summary>
    /// <example>
    ///     Для использования сервиса можно использовать следующий код
    ///     <code>
    /// ServiceLocator.Instance.Service.Method();
    /// </code>
    /// </example>
    public sealed class ServiceLocator
    {
        /// <summary>
        ///     Единственный экземпляр
        /// </summary>
        private static ServiceLocator _instance;

        /// <summary>
        ///     Экземпляр сервиса конфигурации.
        /// </summary>
        private IConfigurationService _configurationService;

        /// <summary>
        ///     Экземпляр сервиса БД.
        /// </summary>
        private IDatabaseService _databaseService;

        /// <summary>
        ///     Экземпляр сервиса логгирования.
        /// </summary>
        private ILogService _logService;

        private readonly object _syncObject = new object();
        private IQuerySourceManager _querySourceManager;

        private ServiceLocator()
        {
        }

        public static ServiceLocator Instance => _instance ?? (_instance = new ServiceLocator());

        /// <summary>
        ///     Сервис логгирования.
        /// </summary>
        public ILogService LogService
        {
            get
            {
                lock (_syncObject)
                {
                    if (_logService == null)
                    {
                        try
                        {
                            _logService = CreateService<ILogService>();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.WriteLine(@"Будет использован стандартный логировщик в консоль");
                            _logService = new DefaultLogService();
                        }
                    }
                    return _logService;
                }
            }
        }

        /// <summary>
        ///     Сервис БД.
        /// </summary>
        public IDatabaseService DatabaseService
        {
            get
            {
                lock (this)
                {
                    return _databaseService ?? (_databaseService = CreateService<IDatabaseService>());
                }
            }
        }

        /// <summary>
        ///     Сервис конфигурации.
        /// </summary>
        public IConfigurationService ConfigurationService
        {
            get
            {
                lock (_syncObject)
                {
                    return _configurationService ?? (_configurationService = CreateService<IConfigurationService>());
                }
            }
        }

        /// <summary>
        ///     Сервис источника запросов.
        /// </summary>
        public IQuerySourceManager QuerySourceManager
        {
            get
            {
                lock (_syncObject)
                {
                    return _querySourceManager ?? (_querySourceManager = CreateService<IQuerySourceManager>());
                }
            }
        }

        /// <summary>
        ///     Сервис хеширования.
        /// </summary>
        public IHasher HashService
        {
            get
            {
                lock (_syncObject)
                {
                    return CreateService<IHasher>();
                }
            }
        }

        /// <summary>
        ///     Создание сервиса указанного типа, используя данные из конфигурации.
        /// </summary>
        /// <typeparam name="TServiceType">Тип сервиса.</typeparam>
        /// <returns>
        ///     Экземпляр созданного сервиса.
        /// </returns>
        private TServiceType CreateService<TServiceType>()
            where TServiceType : class
        {
            var keyName = typeof(TServiceType).ToString();
            var conf = ConfigurationManager.GetSection("ClusterixN.Common") as NameValueCollection;
            if (conf != null && conf.AllKeys.Contains(keyName))
            {
                var className = conf[keyName];
                var type = Type.GetType(className);
                if (type != null)
                {
                    var serviceInstance = Activator.CreateInstance(type);
                    if (!(serviceInstance is TServiceType))
                        throw new ConfigurationErrorsException(
                            $"Класс {className} не реализует интерфейс {keyName}");
                    return serviceInstance as TServiceType;
                }
            }
            throw new ConfigurationErrorsException(
                $"В конфигурации не найден сервис реализующий интерфейс {keyName}");
        }
    }
}