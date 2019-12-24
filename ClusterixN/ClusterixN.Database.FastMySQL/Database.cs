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
using System.Runtime.InteropServices;
using System.Text;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Database.FastMySQL
{
    public class Database : MySQL.Database, IDatabase
    {
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

        /// <summary>
        ///     Выборка по блокам из БД
        /// </summary>
        /// <param name="query">Запрос к БД</param>
        /// <param name="blockSize">Размер блока в строках</param>
        public new void SelectBlocks(string query, int blockSize)
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

        public new List<byte[]> Select(string query, int blockSize)
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
    }
}