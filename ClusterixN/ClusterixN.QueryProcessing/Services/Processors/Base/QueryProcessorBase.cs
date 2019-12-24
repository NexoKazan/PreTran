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
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.QueryProcessing.Data;

namespace ClusterixN.QueryProcessing.Services.Processors.Base
{
    abstract class QueryProcessorBase : IDisposable
    {
        protected readonly ILogger Logger;
        protected readonly QueryProcessConfig DbConfig;
        protected IDatabase Database;

        protected QueryProcessorBase(QueryProcessConfig config, Guid queryId)
        {
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            DbConfig = config;
            Logger.Trace("Инициализирован обработчик запроса");
            Database = ServiceLocator.Instance.DatabaseService.GetDatabase(DbConfig.ConnectionString, queryId.ToString());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Database?.Dispose();
            }
        }
        
        public abstract void Pause(bool pause);

        public abstract void StopQuery(Guid id);

        /// <summary>Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
