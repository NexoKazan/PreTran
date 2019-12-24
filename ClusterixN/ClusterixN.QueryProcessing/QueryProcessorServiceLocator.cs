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
using System.Linq;
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.QueryProcessing.Services.Base;

namespace ClusterixN.QueryProcessing
{
    public class QueryProcessorServiceLocator
    {
        private readonly Dictionary<Type, ServiceBase> _services;
        private readonly Dictionary<Type, object> _commonObjects;
        private readonly ILogger _logger;

        public static QueryProcessorServiceLocator Instance => _instance ??
                                                               (_instance = new QueryProcessorServiceLocator());
        private static QueryProcessorServiceLocator _instance;

        public QueryProcessorServiceLocator()
        {
            _services = new Dictionary<Type, ServiceBase>();
            _commonObjects = new Dictionary<Type, object>();
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
        }

        public void RegisterService<T>() where T : ServiceBase
        {
            if (!_services.ContainsKey(typeof(T)))
            {
                _logger.Trace($"Зарегистрирован обработчик: {typeof(T)}");
                _services.Add(typeof(T), CreateInstance<T>());
            }
            else
            {
                _logger.Error($"Обработчик уже добавлен: {typeof(T)}");
            }
        }

        private T CreateInstance<T>() where T : ServiceBase
        {
            var ctorObjects = new List<object>();
            var type = typeof(T);
            foreach (var ctor in type.GetConstructors())
            {
                if (ctorObjects.Count > 0) break;
                foreach (var ctorParam in ctor.GetParameters())
                {
                    if (_commonObjects.ContainsKey(ctorParam.ParameterType))
                    {
                        ctorObjects.Add(_commonObjects[ctorParam.ParameterType]);
                    }
                    else if (_commonObjects.Keys.SelectMany(c => c.GetInterfaces()).Contains(ctorParam.ParameterType))
                    {
                        foreach (var commonObject in _commonObjects)
                        {
                            if (commonObject.Key.GetInterfaces().Contains(ctorParam.ParameterType))
                            {
                                ctorObjects.Add(commonObject.Value);
                            }
                        }
                    }
                    else if (_services.ContainsKey(ctorParam.ParameterType))
                    {
                        ctorObjects.Add(_services[ctorParam.ParameterType]);
                    }
                    else if (_services.Keys.SelectMany(c => c.GetInterfaces()).Contains(ctorParam.ParameterType))
                    {
                        foreach (var service in _services)
                        {
                            if (service.Key.GetInterfaces().Contains(ctorParam.ParameterType))
                            {
                                ctorObjects.Add(service.Value);
                            }
                        }
                    }
                    else
                    {
                        _logger.Warning($"Не найден объект для параметра конструктора {ctorParam.ParameterType}");
                        ctorObjects.Clear();
                        break;
                    }
                }
            }

            if (ctorObjects.Count == 0)
                throw new Exception($"Не найдены все необходимые объекты для создания шага конвейера {type}");

            return (T)Activator.CreateInstance(type, ctorObjects.ToArray());
        }

        public void RegisterType<T>()
        {
            if (!_commonObjects.ContainsKey(typeof(T)))
            {
                _logger.Trace($"Зарегистрирован общий объект: {typeof(T)}");
                _commonObjects.Add(typeof(T), CreateObject(typeof(T)));
            }
            else
            {
                _logger.Error($"Зарегистрирован дубль общего объекта: {typeof(T)}");
            }
        }

        public void RegisterObject(object obj)
        {
            if (!_commonObjects.ContainsKey(obj.GetType()))
            {
                _logger.Trace($"Зарегистрирован общий объект: {obj.GetType()}");
                _commonObjects.Add(obj.GetType(), obj);
            }
            else
            {
                _logger.Error($"Зарегистрирован дубль общего объекта: {obj.GetType()}");
            }
        }

        public T GetService<T>() where T : ServiceBase
        {
            if (_services.ContainsKey(typeof(T)))
            {
                return (T)_services[typeof(T)];
            }
            return null;
        }

        private object CreateObject(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
