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

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
 using System.IO;
 using System.Reflection;
 using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ClusterixN.Common;
using ClusterixN.Common.Data.EventArgs;
using ClusterixN.Common.Interfaces;
using MySql.Data.MySqlClient;

namespace ClusterixN.Database.MySQL8
{
    public class Database : IDatabase
    {
        protected readonly ILogger Logger;
        private MySqlConnection _connection;
        private bool _isPause;
        private bool _isStopSelect;
        private readonly object _pauseWaitSyncObject = new object();
        private readonly string _engine;
        readonly bool _isRunningOnMono = (Type.GetType("Mono.Runtime") != null);

        public Database()
        {
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _engine = ServiceLocator.Instance.ConfigurationService.GetAppSetting("MySQL_Engine");
        }


        [DllImport("FastMysqlSelect.dll", EntryPoint = "FastSelectString")]
        protected static extern IntPtr FastSelect(IntPtr handle, string host, string user, string passwd, string db,
            string query, ref int lenght, ref int rowCount);

        [DllImport("FastMysqlSelect.dll", EntryPoint = "FastSelectBloksString")]
        protected static extern IntPtr FastBlocksMysqlSelect(IntPtr handle, string host, string user, string passwd, string db,
            string query, ref int lenght, int rowCount);

        [DllImport("FastMysqlSelect.dll", EntryPoint = "INIT")]
        protected static extern IntPtr Init();

        [DllImport("FastMysqlSelect.dll", EntryPoint = "DESTROY")]
        protected static extern void DESTROY(IntPtr ptr);

        [DllImport("FastMysqlSelect.dll", EntryPoint = "GetErrorMessage")]
        protected static extern IntPtr GetErrorMessage(IntPtr ptr, ref int lenght);

        [StructLayout(LayoutKind.Sequential)]
        struct DataBlock
        {
            public IntPtr Data;
            public int Length;
        }

        private MySqlConnection Connection
        {
            get
            {
                //if (_connection != null) return _connection;
                
                //всегда новое подключение
                _connection = new MySqlConnection(ConnectionString);
                try
                {
                    _connection.Open();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    _connection = null;
                }
                return _connection;
            }
        }

        public string ConnectionString { get; set; }

        private bool IsPause
        {
            get
            {
                lock (_pauseWaitSyncObject)
                {
                    return _isPause;
                }
            }
            set
            {
                lock (_pauseWaitSyncObject)
                {
                    _isPause = value;
                }
            }
        }
        protected bool IsStopSelect
        {
            get
            {
                lock (_pauseWaitSyncObject)
                {
                    return _isStopSelect;
                }
            }
            set
            {
                lock (_pauseWaitSyncObject)
                {
                    _isStopSelect = value;
                }
            }
        }
        
        public void LoadFile(string filePath, string tablenName)
        {
            try
            {
                var connection = Connection;
                ExecuteQuery(@"SET FOREIGN_KEY_CHECKS = 0;
SET UNIQUE_CHECKS = 0;
SET sql_log_bin = 0;", connection);
                MySqlBulkLoader bulkloader = new MySqlBulkLoader(connection)
                {
                    TableName = $"`{tablenName}`",
                    FileName = filePath,
                    Timeout = int.MaxValue / 1000,
                    NumberOfLinesToSkip = 0,
                    FieldTerminator = "|",
                    LineTerminator = "\n",
                    FieldQuotationCharacter = '\"',
                    Priority = MySqlBulkLoaderPriority.Concurrent
                };
                bulkloader.Load();
                ExecuteQuery(@"SET FOREIGN_KEY_CHECKS = 1;
SET UNIQUE_CHECKS = 1;", connection);
            }

            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
            }
        }

        public void DissableKeys(string tableName)
        {
            try
            {
                var connection = Connection;
                ExecuteQuery($@"ALTER TABLE `{tableName}` DISABLE KEYS;", connection);
            }
            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
            }
        }

        public void EnableKeys(string tableName)
        {
            try
            {
                var connection = Connection;
                ExecuteQuery($@"ALTER TABLE `{tableName}` ENABLE  KEYS;", connection);
            }

            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
            }
        }

        public void DropTable(string tableName)
        {
            var relationDropQuery = "DROP TABLE `" + tableName + "`;";
            ExecuteQuery(relationDropQuery);
        }

        public void DropTmpRealtions()
        {
            ExecuteQuery(@"SET GROUP_CONCAT_MAX_LEN = 1000000000;
            SET FOREIGN_KEY_CHECKS = 0;
            SET @tbls = (SELECT GROUP_CONCAT(CONCAT('`', TABLE_NAME, '`'))
            FROM information_schema.TABLES
                WHERE TABLE_NAME LIKE '%_tmp%' AND TABLE_SCHEMA = DATABASE());
            SET @delStmt = CONCAT('DROP TABLE ', @tbls);
            PREPARE stmt FROM @delStmt;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
            SET FOREIGN_KEY_CHECKS = 1;");
        }

        public void CreateRelation(string name, string[] fields, string[] types)
        {
            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE `{name}` (");
            for (int i = 0; i < fields.Length; i++)
            {
                sb.Append($"`{fields[i]}` {types[i]}");
                sb.Append(i + 1 == fields.Length ? "" : ",");
            }
            sb.Append(") COLLATE = 'utf8_general_ci' ENGINE = " + _engine);
            ExecuteQuery(sb.ToString());
        }

        public void AddIndex(string name, string relation, string[] fields)
        {
            var sb = new StringBuilder();
            sb.Append($"CREATE INDEX `{name}` ON `{relation}` ({string.Join(",",fields)}) USING BTREE");
            ExecuteQuery(sb.ToString());
        }

        public void AddPrimaryKey(string relation, string[] fields)
        {
            var sb = new StringBuilder($"ALTER TABLE `{relation}` ADD PRIMARY KEY({string.Join(",", fields)})");
            ExecuteQuery(sb.ToString());
        }

        public void QueryIntoRelation(string relationName, string query)
        {
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO `{relationName}` {query}");
            ExecuteQuery(sb.ToString());
        }

        /// <summary>
        ///     Выборка по блокам из БД
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <param name="blockSize">Размер блока в строках</param>
        public void SelectBlocks(string query, int blockSize)
        {
            if (_isRunningOnMono)
            {
                SelectBlocksManaged(query, blockSize);
            }
            else
            {
                SelectBlocksNative(query, blockSize);
            }
        }

        private void SelectBlocksManaged(string query, int blockSize)
        {
            MySqlDataReader rdr = null;
#if DEBUG
            Logger.Trace(query);
#endif

            try
            {
                var types = GetColumnTypes(Connection, query);

                var cmd = new MySqlCommand(query, Connection);
                rdr = cmd.ExecuteReader(CommandBehavior.SingleResult);
                var colNames = GetColumnNmaes(rdr);
                uint count = 0;
                var sendcount = 0;
                var sb = new StringBuilder();
                byte[] sendBuffer = null;

                while (rdr.Read())
                {
                    WaitPause();
                    if (IsStopSelect)
                    {
                        IsStopSelect = false;
                        return;
                    }

                    var values = new object[rdr.FieldCount];
                    var fieldCount = rdr.GetValues(values);
                    var maxFieldCount = fieldCount - 1;
                    for (var i = 0; i < fieldCount; i++)
                    {
                        if (values[i] is DBNull)
                        {
                            sb.Append("NULL");
                        }
                        else
                        {
                            sb.Append("\"");
                            sb.Append(ValueToStr(types, colNames, i, values));
                            sb.Append("\"");
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

            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
                Logger.Trace($"Query: {query}");
            }
            finally
            {
                rdr?.Close();
            }
        }

        private void SelectBlocksNative(string query, int blockSize)
        {
#if DEBUG
            Logger.Trace(query);
#endif
            try
            {
                var rowCount = 0;
                var page = 0;
                query = query.Replace(";", "");
                do
                {
                    WaitPause();
                    if (IsStopSelect)
                    {
                        IsStopSelect = false;
                        return;
                    }

                    var selQuery = query + $" LIMIT {blockSize * page}, {blockSize};";
                    var lenght = 0;
                    var handle = Init();
                    var buf = FastSelect(handle,
                        ConnectionStringParser.GetAddress(ConnectionString) + ":" +
                        ConnectionStringParser.GetPort(ConnectionString),
                        ConnectionStringParser.GetUser(ConnectionString),
                        ConnectionStringParser.GetPassword(ConnectionString),
                        ConnectionStringParser.GetDatabase(ConnectionString),
                        selQuery, ref lenght, ref rowCount);
                    var buffer = new byte[lenght];
                    Marshal.Copy(buf, buffer, 0, buffer.Length);
                    Marshal.FreeHGlobal(buf);
                    DESTROY(handle);

                    OnBlockReaded(buffer, rowCount != blockSize, orderNumber: page);

                    page++;
                } while (rowCount == blockSize);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Получение данных из БД по блокам
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <param name="blockSize">Размер блока в строках</param>
        public List<byte[]> Select(string query, int blockSize)
        {
            return _isRunningOnMono ? SelectManaged(query) : SelectNative(query, blockSize);
        }

        private List<byte[]> SelectManaged(string query)
        {
            MySqlDataReader rdr = null;
#if DEBUG
            Logger.Trace(query);
#endif

            try
            {
                var types = GetColumnTypes(Connection, query);

                var cmd = new MySqlCommand(query, Connection);
                rdr = cmd.ExecuteReader(CommandBehavior.SingleResult);
                var colNames = GetColumnNmaes(rdr);
                var sb = new StringBuilder();

                while (rdr.Read())
                {
                    WaitPause();
                    if (IsStopSelect)
                    {
                        IsStopSelect = false;
                        return new List<byte[]>();
                    }

                    var values = new object[rdr.FieldCount];
                    var fieldCount = rdr.GetValues(values);
                    var maxFieldCount = fieldCount - 1;
                    for (var i = 0; i < fieldCount; i++)
                    {
                        sb.Append(values[i] is DBNull ? "NULL" : ValueToStr(types, colNames, i, values));
                        sb.Append(i != maxFieldCount ? "|" : "\n");
                    }
                }

                return new List<byte[]>() {Encoding.UTF8.GetBytes(sb.ToString())};
            }

            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
                Logger.Trace($"Query: {query}");
            }
            finally
            {
                rdr?.Close();
            }

            return new List<byte[]>();
        }

        private List<byte[]> SelectNative(string query, int blockSize)
        {
#if DEBUG
            Logger.Trace(query);
#endif
            try
            {
                WaitPause();
                if (IsStopSelect)
                {
                    IsStopSelect = false;
                    return new List<byte[]>();
                }

                var selQuery = query;
                var lenght = 0;
                var handle = Init();
                var datablocks = FastBlocksMysqlSelect(handle,
                    ConnectionStringParser.GetAddress(ConnectionString) + ":" +
                    ConnectionStringParser.GetPort(ConnectionString),
                    ConnectionStringParser.GetUser(ConnectionString),
                    ConnectionStringParser.GetPassword(ConnectionString),
                    ConnectionStringParser.GetDatabase(ConnectionString),
                    selQuery, ref lenght, blockSize);

                if (datablocks == IntPtr.Zero)
                {
                    var messageLenght = 0;
                    var buf = GetErrorMessage(handle, ref messageLenght);
                    var buffer = new byte[messageLenght];
                    Marshal.Copy(buf, buffer, 0, buffer.Length);
                    var error = Encoding.ASCII.GetString(buffer);
                    Logger.Error($"Ошибка обработки запроса {query} \n {error}");
                    DESTROY(handle);
                    return new List<byte[]>();
                }

                var blockPointers = new IntPtr[lenght];
                for (int i = 0; i < lenght; i++)
                {
                    blockPointers[i] = Marshal.ReadIntPtr(datablocks, IntPtr.Size * i);
                }

                var resultData = new List<byte[]>();
                for (int i = 0; i < lenght; i++)
                {
                    var datablock = (DataBlock)Marshal.PtrToStructure(blockPointers[i], typeof(DataBlock));
                    var buffer = new byte[datablock.Length];
                    Marshal.Copy(datablock.Data, buffer, 0, buffer.Length);
                    resultData.Add(buffer);
                    Marshal.DestroyStructure(blockPointers[i], typeof(DataBlock));
                }

                DESTROY(handle);
                return resultData;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return new List<byte[]>();
        }

        public void ControlSelectBlocks(bool pause)
        {
            IsPause = pause;
        }

        protected void WaitPause()
        {
            while (IsPause && !IsStopSelect)
            {
                Thread.Sleep(100);
            }
        }

        private string ValueToStr(Dictionary<string, int> types, string[] colNames, int i, object[] values)
        {
            if (!types.ContainsKey(colNames[i])) return values[i].ToString();

            var type = types[colNames[i]];
            var str = string.Empty;
            var val = values[i];

            switch (type)
            {
                case 0:
                    str = val.ToString();
                    break;
                case 1:
                    str = ((DateTime)val).ToString("yyyy-MM-dd H:mm:ss");
                    break;
                case 2:
                    str = ((decimal)val).ToString(CultureInfo.InvariantCulture);
                    break;
            }
            return str;
        }
        
        private string[] GetColumnNmaes(MySqlDataReader rdr)
        {
            var names = new string[rdr.FieldCount];
            for (var i = 0; i < rdr.FieldCount; i++)
            {
                names[i] = rdr.GetName(i);
            }

            return names;
        }

        private Dictionary<string,int> GetColumnTypes(MySqlConnection conn, string query)
        {
            var types = new Dictionary<string, int>();
            var tableName = FindTableName(query);
            MySqlDataReader rdr = null;
            try
            {
                var cmd = new MySqlCommand("show columns from " + tableName, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var values = new object[rdr.FieldCount];
                    rdr.GetValues(values);
                    var typeStr = values[1].ToString().ToLowerInvariant();
                    int type = 0;
                    if (typeStr.Contains("date")) type = 1;
                    else if (typeStr.Contains("decimal")) type = 2;

                    types.Add(values[0].ToString(), type);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                rdr?.Close();
            }
            return types;
        }

        private string FindTableName(string query)
        {
            int start = query.IndexOf("from", StringComparison.InvariantCultureIgnoreCase) + 4;
            int end = query.IndexOf("where", StringComparison.InvariantCultureIgnoreCase);
            int lenght = end < start ? query.Length - start : end - start;
            return query.Substring(start, lenght).Trim();
        }

        private void ExecuteQuery(string query)
        {
            try
            {
                var cmd = new MySqlCommand(query, Connection) {CommandTimeout = 0};
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
                Logger.Error($"Query: {query}");
            }
        }

        private void ExecuteQuery(string query, MySqlConnection connection)
        {
            try
            {
                var cmd = new MySqlCommand(query, connection) {CommandTimeout = 0};
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Logger.Error("Error:", ex);
                Logger.Error($"Query: {query}");
            }
        }

        #region Events

        public event EventHandler<SelectResultEventArg> BlockReaded;
        
        protected void OnBlockReaded(byte[] rows, bool isLast = false, int orderNumber = 0)
        {
            BlockReaded?.Invoke(this, new SelectResultEventArg { Result = rows, IsLast = isLast, OrderNumber = orderNumber });
        }

        #endregion

        public void StopSelectQuery()
        {
            IsStopSelect = true;
        }

        public void Dispose()
        {
        }
    }
}