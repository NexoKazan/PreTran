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

namespace ClusterixN.Common.Infrastructure.Base
{
    /// <summary>
    ///     Реализация сервиса БД
    /// </summary>
    public abstract class DatabaseServiceBase<TDatabase> : IDatabaseService
        where TDatabase : IDatabase

    {
        private object _syncObject = new object();

        /// <summary>
        ///     Справочник БД для различных строк соединения.
        /// </summary>
        private readonly Dictionary<string, IDatabase> _databases = new Dictionary<string, IDatabase>();

        public IDatabase GetDatabase(string connectionString, string connectionId, bool newInstance = false)
        {
            lock (_syncObject)
            {
                var key = connectionString + connectionId;

                if (newInstance)
                {
                    var newdb = (IDatabase)Activator.CreateInstance(typeof(TDatabase));
                    newdb.ConnectionString = connectionString;
                    return newdb;
                }

                if (_databases.ContainsKey(key))
                    return _databases[key];

                var db = (IDatabase)Activator.CreateInstance(typeof(TDatabase));
                db.ConnectionString = connectionString;
                _databases.Add(key, db);

                return db;
            }
        }
    }
}