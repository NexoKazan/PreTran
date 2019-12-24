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

﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
 using System.Linq;
 using ClusterixN.Common.Data.Log.Enum;
 using LogProcessingTool.Visualizer.Data;

namespace LogProcessingTool
{
    public class QueryExecutionCounter
    {
        private string _db;
        private SQLiteConnection _logDbConnection;

        private SQLiteConnection DbConnection
        {
            get
            {
                if (_logDbConnection == null)
                {
                    _logDbConnection = new SQLiteConnection($"Data Source={_db}; Version=3;");
                    _logDbConnection.Open();
                }
                return _logDbConnection;
            }
        }

        private List<QueryLogData> GetTimeLog(SQLiteConnection connection)
        {
            var result = new List<QueryLogData>();
            var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT Timestamp, Module, Operation, Duration, IsCanceled, q.Number as Number, q.Id as QueryId FROM times t JOIN query q ON t.QueryId = q.Id; ";
            var ignoredOperations = new List<MeasuredOperation>()
            {
                MeasuredOperation.NOP,
                //MeasuredOperation.WaitStart,
                MeasuredOperation.WaitJoin,
                MeasuredOperation.WaitSort,
                MeasuredOperation.WaitStart,
                MeasuredOperation.WorkDuration
            };
            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int canceled;
                    int.TryParse(reader["IsCanceled"].ToString(), out canceled);
                    var log = new QueryLogData
                    {
                        Duration = double.Parse(reader["Duration"].ToString()),
                        Time = DateTime.Parse((string)reader["Timestamp"]),
                        Module = (string)reader["Module"],
                        IsCanceled = canceled == 1,
                        Operation =
                            (MeasuredOperation)Enum.Parse(typeof(MeasuredOperation), (string)reader["Operation"]),
                        QueryId = (string)reader["QueryId"],
                        QueryNumber = int.Parse(reader["Number"].ToString())
                    };
                    if (ignoredOperations.Contains(log.Operation)) continue;
                    result.Add(log);
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return result;
        }

        public string Count(string fileName)
        {
            _db = fileName;
            var message = "Подсчет времени обработки запросов...";
            ConsoleHelper.ReWriteLine(message);
            return Count(GetTimeLog(DbConnection));
        }

        private string Count(List<QueryLogData> logEvents)
        {
            var orderedLog = logEvents.OrderBy(l => l.Time);
            string result = "";
            string opResult = string.Empty;
            var queryTimes = new List<double>();
            var opTimes = new List<Tuple<MeasuredOperation, double>>();

            foreach (var groupedlog in orderedLog.GroupBy(l => l.QueryId))
            {
                var vizNodes = ConvertToVizNodes(groupedlog.ToList());
                var execTime = TimeSpan.Zero;
                foreach (var vizNode in vizNodes)
                {
                    foreach (var vizNodeOp in vizNode.VizData.GroupBy(v => v.Operation))
                    {
                        var time = ProjectDuration(vizNodeOp.ToList());
                        opResult += $"\t{vizNodeOp.Key}\tВремя: {time.TotalSeconds}{Environment.NewLine}";
                        opTimes.Add(new Tuple<MeasuredOperation, double>(vizNodeOp.Key, time.TotalSeconds));
                    }
                    execTime += ProjectDuration(vizNode.VizData);
                }

                queryTimes.Add(execTime.TotalSeconds);

                result +=
                //    $"{groupedlog.Key}\t{groupedlog.First().QueryNumber}\t{execTime.TotalSeconds}\n";
                $"Запрос: {groupedlog.Key}\tНомер: {groupedlog.First().QueryNumber}\tВремя обработки: {execTime.TotalSeconds}{Environment.NewLine}";
                result += opResult;
                opResult = string.Empty;
            }

            result += Environment.NewLine;
            result += Environment.NewLine;

            foreach (var g in opTimes.GroupBy(o=>o.Item1))
            {
                result += $"{g.Key}\t{g.Average(a=>a.Item2)}{Environment.NewLine}";
            }

            result += Environment.NewLine;
            var m = queryTimes.Average();
            result += $"M\t{m}{Environment.NewLine}";
            result += $"sig\t{Math.Sqrt(queryTimes.Select(q => Math.Pow(q - m, 2)).Average())}{Environment.NewLine}";

            return result;
        }
        
        private static List<VizNode> ConvertToVizNodes(List<QueryLogData> logEvents)
        {
            var orderedLog = logEvents.OrderBy(l => l.Time);
            var minDate = logEvents.Min(l => l.Time);
            var vizNodes = new List<VizNode>();

            foreach (var groupedlog in orderedLog.GroupBy(l => l.QueryId))
            {
                var vizNode = new VizNode() { Name = groupedlog.Key };
                foreach (var log in groupedlog)
                {
                    vizNode.VizData.Add(new VizData()
                    {
                        Start = log.Time - minDate,
                        Operation = log.Operation,
                        Duration = TimeSpan.FromMilliseconds(log.Duration),
                        IsCanceled = log.IsCanceled
                    });
                }
                vizNodes.Add(vizNode);
            }
            return vizNodes;
        }
        
        private TimeSpan ProjectDuration(List<VizData> data)
        {
            var lineLengths = new List<VizData>();
            var mergePerformed = false;

            foreach (var vizData in data.OrderBy(d=>d.Start))
            {
                var merged = false;
                if (lineLengths.Count>0)
                {
                    var len = lineLengths.Last();
                    if (len.Start + len.Duration >= vizData.Start &&
                        len.Start + len.Duration <= vizData.Start + vizData.Duration)
                    {
                        //продолжение вправо
                        len.Duration = vizData.Start - len.Start + vizData.Duration ;
                        merged = mergePerformed = true;
                    }
                    else if (len.Start <= vizData.Start &&
                        len.Start + len.Duration >= vizData.Start + vizData.Duration)
                    {
                        //измерение внутри промежутка
                        merged = mergePerformed = true;
                    }
                }
                if (!merged) lineLengths.Add(vizData);
            }
            if (!mergePerformed) return TimeSpan.FromSeconds(lineLengths.Sum(l => l.Duration.TotalSeconds));

            return ProjectDuration(lineLengths);
        }
    }

    public class QueryLogData : LogData
    {
        public int QueryNumber { get; set; }

        public string QueryId { get; set; }
    }
}
