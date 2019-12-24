#region Copyright
/*
 * Copyright 2018 Roman Klassen
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.EventArgs;
using ClusterixN.Common.Interfaces;
using Npgsql;

namespace ClusterixN.Database.Postgres
{
    public class Database : IDatabase
    {
        private readonly ILogger _logger;
        private NpgsqlConnection _connection;
        private readonly object _syncObject = new object();

        protected NpgsqlConnection Connection
        {
            get
            {
                _connection = new NpgsqlConnection(ConnectionString);
                try
                {
                    _connection.Open();
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    _connection = null;
                }

                return _connection;
            }
        }

        public Database()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
        }

        public string ConnectionString { get; set; }

        public void LoadFile(string filePath, string tablenName)
        {
            lock (_syncObject)
            {
                ExecuteQuery($"COPY \"{tablenName.ToLowerInvariant()}\" FROM '{filePath.Replace("\\", "/")}' CSV DELIMITER '|' QUOTE '\"' null 'NULL';");
            }
        }

        public void DissableKeys(string tableName)
        {
            throw new NotImplementedException();
        }

        public void EnableKeys(string tableName)
        {
            throw new NotImplementedException();
        }

        public void DropTable(string tableName)
        {
            lock (_syncObject)
            {
                var relationDropQuery = $"DROP TABLE {tableName.ToLowerInvariant()};";
                ExecuteQuery(relationDropQuery);
            }
        }

        public void DropTmpRealtions()
        {
            lock (_syncObject)
            {
                try
                {
                    var query = $@"SELECT quote_ident(table_schema) || '.' || quote_ident(table_name)
                        FROM   information_schema.tables
                        WHERE  table_name LIKE '%' || '{Constants.RelationPostfix}'
                    AND table_schema NOT LIKE 'pg_%'";
                    using (var conn = Connection)
                    {
                        using (var command = new NpgsqlCommand(query, conn))
                        {
                            var reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                DropTable(reader[0].ToString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }

        public void CreateRelation(string name, string[] fields, string[] types)
        {
            lock (_syncObject)
            {
                var sb = new StringBuilder();
                sb.Append($"CREATE TABLE {name.ToLowerInvariant()} (");
                for (int i = 0; i < fields.Length; i++)
                {
                    sb.Append($"{fields[i]} {TranslateColumnType(types[i])}");
                    sb.Append(i + 1 == fields.Length ? "" : ",");
                }
                sb.Append(")");

                ExecuteQuery(sb.ToString());
            }
        }

        private string TranslateColumnType(string type)
        {
            if (type.ToLowerInvariant().Contains("int"))
            {
                return type.ToLowerInvariant().Contains("null") ? "INT NULL" : "INT";
            }
            return type;
        }

        public void AddIndex(string name, string relation, string[] fields)
        {
            lock (_syncObject)
            {
                var sb = new StringBuilder();
                sb.Append($"CREATE INDEX {name.ToLowerInvariant()}_{relation.ToLowerInvariant()} ON {relation.ToLowerInvariant()} ({string.Join(",", fields)})");
                
                ExecuteQuery(sb.ToString());
            }
        }

        public void AddPrimaryKey(string relation, string[] fields)
        {
            throw new NotImplementedException();
        }

        public void QueryIntoRelation(string relationName, string query)
        {
            lock (_syncObject)
            {
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {relationName.ToLowerInvariant()} {query}");
                ExecuteQuery(sb.ToString());
            }
        }

        /// <summary>
        ///     Выборка по блокам из БД
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <param name="blockSize">Размер блока в строках</param>
        public void SelectBlocks(string query, int blockSize)
        {
            lock (_syncObject)
            {
                try
                {
                    SelectQuery(query, blockSize);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }

        /// <summary>
        /// Получение данных из БД по блокам
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <param name="blockSize">Размер блока в строках</param>
        public List<byte[]> Select(string query, int blockSize)
        {
            throw new NotImplementedException();
        }

        public void ControlSelectBlocks(bool pause)
        {
            //ignored
        }

        private void SelectQuery(string query, int blockSize)
        {
            QDebug(query);
            using (var conn = Connection)
            {
                using (var command = new NpgsqlCommand(query, conn))
                {
                    var sb = new StringBuilder();
                    var reader = command.ExecuteReader();
                    int count = 0;
                    int sendcount = 0;
                    byte[] sendBuffer = null;

                    while (reader.Read())
                    {
                        var values = new object[reader.FieldCount];
                        var fieldCount = reader.GetValues(values);
                        var maxFieldCount = fieldCount - 1;
                        for (var i = 0; i < fieldCount; i++)
                        {
                            if (values[i] is DBNull)
                            {
                                sb.Append("NULL");
                            }
                            else
                            {
                                sb.Append(values[i]);
                            }

                            sb.Append(i != maxFieldCount ? "|" : "\n");
                        }

                        count++;

                        if (count % blockSize == 0) //отправка блока
                        {
                            if (sendBuffer != null)
                            {
                                OnBlockReaded(sendBuffer, orderNumber: sendcount++);
                            }

                            var dest = new char[sb.Length];
                            sb.CopyTo(0, dest, 0, sb.Length);
                            sendBuffer = Encoding.UTF8.GetBytes(dest);
                            sb = new StringBuilder();
                        }
                    }

                    //отправка последнего блока
                    if (sb.Length > 0)
                    {
                        if (sendBuffer != null)
                        {
                            OnBlockReaded(sendBuffer, orderNumber: sendcount++);
                        }

                        OnBlockReaded(Encoding.UTF8.GetBytes(sb.Remove(sb.Length - 1, 1).ToString()), true, sendcount);
                    }
                    else
                    {
                        OnBlockReaded(sendBuffer, true, sendcount);
                    }
                }
            }
        }

        public void ExecuteQuery(string query)
        {
            lock (_syncObject)
            {
                QDebug(query);
                try
                {
                    using (var conn = Connection)
                    {
                        using (var command = new NpgsqlCommand(query, conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    _logger.Error(query);
                }
            }
        }

#region Events

        public event EventHandler<SelectResultEventArg> BlockReaded;
        public void StopSelectQuery()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnBlockReaded(byte[] rows, bool isLast = false, int orderNumber = 0)
        {
            BlockReaded?.Invoke(this, new SelectResultEventArg { Result = rows, IsLast = isLast, OrderNumber = orderNumber });
        }

        #endregion

        [Conditional("DEBUG")]
        private void QDebug(string query)
        {
            _logger.Trace(query);
        }

        public void Dispose()
        {
        }
    }
}