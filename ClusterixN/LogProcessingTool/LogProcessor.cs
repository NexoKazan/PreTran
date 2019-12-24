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
using System.Data.SQLite;
using System.IO;
using System.Linq;
using ClusterixN.Common.Data.Log;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Data.Query;
using LogProcessingTool.Properties;

namespace LogProcessingTool
{
    internal class LogProcessor
    {
        public void Process(string resultDb, string queryDirName, params string[] logDbs)
        {
            var queries = LoadQueries(queryDirName);
            var canceledQueries = LoadCanceledQueries(queryDirName);
            var timesList = new List<TimeLogEvent>();
            var perfCounterList = new List<PerformanceLogEvent>();
            var dbs = new List<string>();

            if (logDbs.Length == 0)
            {
                dbs.AddRange(Directory.EnumerateFiles(queryDirName, "*timeLog.db"));
            }
            else
            {
                dbs = logDbs.ToList();
            }

            for (var i = 0; i < dbs.Count; i++)
            {
                ConsoleHelper.ProgressBar(dbs.Count, i + 1, "Загрузка баз данных");
                var logDb = dbs[i];
                var logDbConnection = new SQLiteConnection($"Data Source={logDb}; Version=3;");
                logDbConnection.Open();
                timesList.AddRange(GetTimeLog(logDbConnection));
                logDbConnection.Close();
            }

            FillLogData(timesList, queries);

            dbs.Clear();
            dbs.AddRange(Directory.EnumerateFiles(queryDirName, "*performance.db"));

            for (var i = 0; i < dbs.Count; i++)
            {
                ConsoleHelper.ProgressBar(dbs.Count, i + 1, "Загрузка БД с счетчиками производительности");
                var logDb = dbs[i];
                var logDbConnection = new SQLiteConnection($"Data Source={logDb}; Version=3;");
                logDbConnection.Open();
                perfCounterList.AddRange(GetPerformanceLog(logDbConnection));
                logDbConnection.Close();
            }

            if (File.Exists(resultDb)) File.Delete(resultDb);

            var resultDbConnection = new SQLiteConnection($"Data Source={resultDb}; Version=3;");
            resultDbConnection.Open();
            CreateResultScheme(resultDbConnection);
            WriteLogData(resultDbConnection, timesList);
            WriteQueriesData(resultDbConnection, queries, canceledQueries);
            WritePerformanceLogData(resultDbConnection, perfCounterList);
            resultDbConnection.Close();
        }

        private void CreateResultScheme(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = Resources.QuerySchema;
            cmd.ExecuteNonQuery();
        }
        private List<PerformanceLogEvent> GetPerformanceLog(SQLiteConnection connection)
        {
            var result = new List<PerformanceLogEvent>();
            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT Timestamp, Module, Counter, `Value` FROM performance;";
            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    result.Add(new PerformanceLogEvent
                    {
                        Value = double.Parse(reader["Value"].ToString()),
                        Counter = (string) reader["Counter"],
                        Time = DateTime.Parse((string) reader["Timestamp"]),
                        Module = (string) reader["Module"]
                    });
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return result;
        }

        private List<TimeLogEvent> GetTimeLog(SQLiteConnection connection)
        {
            var result = new List<TimeLogEvent>();
            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT Timestamp, Module, Operation, `From`, `To`, QueryId, SubQueryId, RelationId, Duration FROM times;";
            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    result.Add(new TimeLogEvent
                    {
                        Duration = double.Parse(reader["Duration"].ToString()),
                        QueryId = Guid.Parse((string) reader["QueryId"]),
                        SubQueryId = Guid.Parse((string) reader["SubQueryId"]),
                        RelationId = Guid.Parse((string) reader["RelationId"]),
                        Time = DateTime.Parse((string) reader["Timestamp"]),
                        Module = (string) reader["Module"],
                        From = (string) reader["From"],
                        To = (string) reader["To"],
                        Operation =
                            (MeasuredOperation) Enum.Parse(typeof(MeasuredOperation), (string) reader["Operation"])
                    });
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return result;
        }

        private List<Query> LoadQueries(string dirName)
        {
            var result = new List<Query>();
            var files = Directory.EnumerateFiles(dirName, "*.xml").ToList();
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                ConsoleHelper.ProgressBar(files.Count, i + 1, "Загрузка запросов");
                result.Add(Query.Load(file)); 
            }
            return result;
        }

        private List<Guid> LoadCanceledQueries(string dirName)
        {
            var result = new List<Guid>();
            var files = Directory.EnumerateFiles(dirName, "*dropqueries.log").ToList();
            for (var i = 0; i < files.Count; i++)
            {
                ConsoleHelper.ProgressBar(files.Count, i + 1, "Загрузка отмененных запросов");
                var queries = File.ReadLines(files[i]);
                foreach (var query in queries)
                {
                    result.Add(Guid.Parse(CanceledQueryParser.GetQueryId(query)));
                }
            }
            return result;
        }

        private void FillLogData(List<TimeLogEvent> log, List<Query> queries)
        {
            for (var i = 0; i < log.Count; i++)
            {
                ConsoleHelper.ProgressBar(log.Count, i + 1, "Обработка измерений и связывание");
                var logEvent = log[i];

                if (logEvent.Operation == MeasuredOperation.WorkDuration ||
                    logEvent.Operation == MeasuredOperation.Pause) continue;

                if (logEvent.QueryId == Guid.Empty)
                {
                    if (logEvent.SubQueryId == Guid.Empty)
                    {
                        if (logEvent.Operation == MeasuredOperation.DeleteData && logEvent.Module.Contains("SORT"))
                        {
                            logEvent.SubQueryId = logEvent.RelationId;
                        }
                        else
                        {
                            logEvent.SubQueryId = FindSubQueryIdByRealtionId(queries, logEvent.RelationId);
                        }
                    }
                    logEvent.QueryId = FindQueryIdbySubQueryId(queries, logEvent.SubQueryId);
                }
            }
        }

        private Guid FindQueryIdbySubQueryId(List<Query> queries, Guid subQueryId)
        {
            var query = queries.Find(q => q.JoinQueries.Any(jq => jq.QueryId == subQueryId) ||
                                          q.SelectQueries.Any(sq => sq.QueryId == subQueryId) ||
                                          q.SortQuery.QueryId == subQueryId);
            return query.Id;
        }

        private Guid FindSubQueryIdByRealtionId(List<Query> queries, Guid relationId)
        {
            foreach (var query in queries)
            {
                foreach (var sortRelation in query.SortQuery.SortRelation)
                    if (sortRelation.RelationId == relationId)
                        return query.SortQuery.QueryId;

                foreach (var joinQuery in query.JoinQueries)
                    if (joinQuery.LeftRelation.RelationId == relationId ||
                        joinQuery.RightRelation.RelationId == relationId)
                        return joinQuery.QueryId;
            }
            return Guid.Empty;
        }

        private void WritePerformanceLogData(SQLiteConnection connection, List<PerformanceLogEvent> log)
        {
            using (var transaction = connection.BeginTransaction())
            {
                using (var cmd = connection.CreateCommand())
                {
                    for (var i = 0; i < log.Count; i++)
                    {
                        ConsoleHelper.ProgressBar(log.Count, i + 1,
                            "Запись измерений производительности в результирующую БД");
                        var logEvent = log[i];
                        cmd.CommandText =
                            "INSERT INTO performance(Timestamp, Module, Counter, `Value`) " +
                            "VALUES(@Timestamp, @Module, @Counter, @Value);";
                        cmd.Parameters.Add(new SQLiteParameter("@Timestamp",
                            logEvent.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                        cmd.Parameters.Add(new SQLiteParameter("@Module", logEvent.Module));
                        cmd.Parameters.Add(new SQLiteParameter("@Counter", logEvent.Counter));
                        cmd.Parameters.Add(new SQLiteParameter("@Value", logEvent.Value));
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        private void WriteLogData(SQLiteConnection connection, List<TimeLogEvent> log)
        {
            using (var transaction = connection.BeginTransaction())
            {
                using (var cmd = connection.CreateCommand())
                {
                    for (var i = 0; i < log.Count; i++)
                    {
                        ConsoleHelper.ProgressBar(log.Count, i + 1, "Запись измерений в результирующую БД");
                        var logEvent = log[i];
                        cmd.CommandText =
                            "INSERT INTO times(Timestamp, Module, Operation, `From`, `To`, QueryId, SubQueryId, RelationId, Duration) " +
                            "VALUES(@Timestamp, @Module, @Operation, @From, @To, @QueryId, @SubQueryId, @RelationId, @Duration);";
                        cmd.Parameters.Add(new SQLiteParameter("@Timestamp",
                            logEvent.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                        cmd.Parameters.Add(new SQLiteParameter("@Module", logEvent.Module));
                        cmd.Parameters.Add(new SQLiteParameter("@Operation", logEvent.Operation.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@From", logEvent.From));
                        cmd.Parameters.Add(new SQLiteParameter("@To", logEvent.To));
                        cmd.Parameters.Add(new SQLiteParameter("@QueryId", logEvent.QueryId.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@SubQueryId", logEvent.SubQueryId.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@RelationId", logEvent.RelationId.ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@Duration", logEvent.Duration));
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        private void WriteQueriesData(SQLiteConnection connection, List<Query> queries, List<Guid> canceledQueries)
        {
            for (var i = 0; i < queries.Count; i++)
            {
                ConsoleHelper.ProgressBar(queries.Count, i + 1, "Запись запросов в результирующую БД");
                var query = queries[i];
                WriteQueryData(connection, query, canceledQueries.Contains(query.Id));
            }
        }

        private void WriteQueryData(SQLiteConnection connection, Query query, bool isCanceled)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO query(Id, Number, IsCanceled) " +
                              "VALUES(@Id, @Number, @IsCanceled);";
            cmd.Parameters.Add(new SQLiteParameter("@Id", query.Id.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("@Number", query.Number));
            cmd.Parameters.Add(new SQLiteParameter("@IsCanceled", isCanceled ? 1 : 0));
            cmd.ExecuteNonQuery();

            foreach (var selectQuery in query.SelectQueries)
                WriteSubQueryData(connection, query.Id, selectQuery.QueryId, "SELECT", selectQuery.Order,
                    selectQuery.Query);

            foreach (var joinQuery in query.JoinQueries)
            {
                WriteSubQueryData(connection, query.Id, joinQuery.QueryId, "JOIN", joinQuery.Order, joinQuery.Query);
                WriteRelationData(connection, joinQuery.QueryId, joinQuery.LeftRelation.RelationId,
                    joinQuery.LeftRelation.Shema.ToString());
                WriteRelationData(connection, joinQuery.QueryId, joinQuery.RightRelation.RelationId,
                    joinQuery.RightRelation.Shema.ToString());
            }

            WriteSubQueryData(connection, query.Id, query.SortQuery.QueryId, "SORT", query.SortQuery.Order,
                query.SortQuery.Query);
            foreach (var relation in query.SortQuery.SortRelation)
            {
                WriteRelationData(connection, query.SortQuery.QueryId, relation.RelationId, relation.Shema.ToString());
            }
        }

        private void WriteSubQueryData(SQLiteConnection connection, Guid queryId, Guid subQueryId, string type,
            int order, string query)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO subquery(Id, QueryId, Type, `Order`, Query) " +
                              "VALUES(@Id, @QueryId, @Type, @Order, @Query);";
            cmd.Parameters.Add(new SQLiteParameter("@Id", subQueryId.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("@QueryId", queryId.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("@Type", type));
            cmd.Parameters.Add(new SQLiteParameter("@Order", order));
            cmd.Parameters.Add(new SQLiteParameter("@Query", query));
            cmd.ExecuteNonQuery();
        }

        private void WriteRelationData(SQLiteConnection connection, Guid subQueryId, Guid relationId, string schema)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO relation(Id, SubQueryId, Schema) " +
                              "VALUES(@Id, @SubQueryId, @Schema);";
            cmd.Parameters.Add(new SQLiteParameter("@Id", relationId.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("@SubQueryId", subQueryId.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("@Schema", schema));
            cmd.ExecuteNonQuery();
        }
    }
}