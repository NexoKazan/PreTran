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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace ClusterixN.Database.MySQL.Test
{
    [TestClass]
    public class SelectTest
    {
        private Database _db;
        private string _connectionString;
        private TestContext _testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return _testContextInstance; }
            set { _testContextInstance = value; }
        }

        [TestInitialize]
        public void TestInit()
        {
            _db = new Database();
            _db.ConnectionString = _connectionString = "Server=127.0.0.1;Uid=root;Pwd=;Database=tpch1;DefaultCommandTimeout=86400;";
        }

        [TestMethod]
        public void FastSelect()
        {
            var db = new FastMySQL.Database();
            db.ConnectionString = _connectionString;
            var sw = new Stopwatch();
            sw.Start();
            var query = @"SELECT L_SHIPDATE, L_DISCOUNT, L_ORDERKEY, L_SUPPKEY, L_EXTENDEDPRICE FROM LINEITEM";
            var result = db.Select(query, 10000);
            sw.Stop();
            TestContext.WriteLine($"Read : {sw.Elapsed}");
            sw.Reset();
        }

        [TestMethod]
        public void FastSelectMany()
        {
            for (int i = 0; i < 1000; i++)
            {
                FastSelect();
            }
        }

        [TestMethod]
        public void ParalellSelect()
        {
            var sw = new Stopwatch();
            sw.Start();
            var query = @"SELECT L_SHIPDATE, L_DISCOUNT, L_ORDERKEY, L_SUPPKEY, L_EXTENDEDPRICE FROM LINEITEM";

            var tasks = new List<Task>();
            for (var i = 0; i < 4; i++)
            {
                tasks.Add(StartJoin(query));
            }

            Task.WaitAll(tasks.ToArray());

            sw.Stop();
            TestContext.WriteLine($"Read : {sw.Elapsed}");
            sw.Reset();
        }

        private Task<List<byte[]>> StartJoin(string joinQuery)
        {
            var task = new Task<List<byte[]>>(JoinTask,
                new Tuple<string>(joinQuery));
            task.Start();
            return task;
        }

        private List<byte[]> JoinTask(object obj)
        {
            var tup = (Tuple<string>)obj;
            var joinQuery = tup.Item1;
            var db = new FastMySQL.Database {ConnectionString = _connectionString};
            return db.Select(joinQuery,100000);
        }

        [TestMethod]
        public void SelectPs()
        {
            var sw = new Stopwatch();
            sw.Start();
            var query = @"SELECT L_SHIPDATE, L_DISCOUNT, L_ORDERKEY, L_SUPPKEY, L_EXTENDEDPRICE FROM LINEITEM WHERE L_SHIPDATE BETWEEN DATE '1995-01-01' AND DATE '1996-12-31'";
            _db.SelectBlocks(query, 10000);
            sw.Stop();
            TestContext.WriteLine($"Read : {sw.Elapsed}");
            sw.Reset();
        }

        [TestMethod]
        public void ReadTest()
        {
            var sw = new Stopwatch();
            var query = @"SELECT L_SHIPDATE, L_DISCOUNT, L_ORDERKEY, L_SUPPKEY, L_EXTENDEDPRICE FROM LINEITEM WHERE L_SHIPDATE BETWEEN DATE '1995-01-01' AND DATE '1996-12-31'";

            MySqlConnection conn = null;
            MySqlDataReader rdr = null;
                uint count = 0;

            sw.Start();
            try
            {
                conn = new MySqlConnection(_connectionString);
                conn.Open();
                var types = GetColumnTypes(conn, query);

                var cmd = new MySqlCommand(query, conn);
                rdr = cmd.ExecuteReader(CommandBehavior.SingleResult);
                var colNames = GetColumnNmaes(rdr);
                var sb = new StringBuilder();

                while (rdr.Read())
                {
                    var values = new object[rdr.FieldCount];
                    var fieldCount = rdr.GetValues(values);

                }
            }

            catch 
            {
                throw;
            }
            finally
            {
                rdr?.Close();
                conn?.Close();
            }
            sw.Stop();
            TestContext.WriteLine($"Read : {sw.Elapsed}, Count: {count}");
            sw.Reset();
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
        private string FindTableName(string query)
        {
            int start = query.IndexOf("from", StringComparison.InvariantCultureIgnoreCase) + 4;
            int end = query.IndexOf("where", StringComparison.InvariantCultureIgnoreCase);
            int lenght = end < start ? query.Length - start : end - start;
            return query.Substring(start, lenght).Trim();
        }

        private Dictionary<string, int> GetColumnTypes(MySqlConnection conn, string query)
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
            catch 
            {
                throw;
            }
            finally
            {
                rdr?.Close();
            }
            return types;
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
    }
}
