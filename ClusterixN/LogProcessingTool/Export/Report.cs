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
using System.Data.SQLite;
using System.IO;
using LogProcessingTool.Properties;

namespace LogProcessingTool.Export
{
    internal class Report : ExcelBaseExport
    {
        private readonly string _dbName;

        public Report(string dbName) : base(dbName, Resources.template)
        {
            _dbName = dbName;
        }

        public void SaveReport(string fileName)
        {
            var message = "Генерация отчета...";
            ConsoleHelper.ReWriteLine(message);
            FillReport();
            File.Move(SaveAndGetFileName(), fileName);
            ConsoleHelper.ReWriteLine(message + " готово.", true);
        }

        private void FillReport()
        {
            var logDbConnection = new SQLiteConnection($"Data Source={_dbName}; Version=3;");
            logDbConnection.Open();

            FillDuration(logDbConnection);
            FillTotalQueryCount(logDbConnection);
            FillCanceledQueryCount(logDbConnection);
            FillQueryCount(logDbConnection);
            FillSumWorkByQuery(logDbConnection);
            FillAvgWorkByQuery(logDbConnection);
            FillWorkStagesByNodes(logDbConnection);

            logDbConnection.Close();
        }

        private void FillDuration(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.WorkDuration;
            try
            {
                CurrentColumn = 3;
                CurrentRow = 1;
                CurrentCell = CurrentCell[CurrentRow, CurrentColumn];

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    WriteCell(reader[0]);
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void FillTotalQueryCount(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.CanceledQueryCount;
            try
            {
                CurrentColumn = 3;
                CurrentRow = 2;
                CurrentCell = CurrentCell[CurrentRow, CurrentColumn];

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    WriteCell(reader[0]);
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void FillCanceledQueryCount(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.CanceledQueryCount;
            try
            {
                CurrentColumn = 3;
                CurrentRow = 3;
                CurrentCell = CurrentCell[CurrentRow, CurrentColumn];

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    WriteCell(reader[1]);
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void FillQueryCount(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.QueryCountByQuery;
            try
            {
                CurrentColumn = 1;
                CurrentRow = 7;

                WriteRows(cmd, 2);
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void FillSumWorkByQuery(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.SumWorkByQuery;
            try
            {
                CurrentColumn = 1;
                CurrentRow = 24;

                WriteRows(cmd, 9);
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void FillAvgWorkByQuery(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.AvgWorkByQuery;
            try
            {
                CurrentColumn = 1;
                CurrentRow = 64;

                WriteRows(cmd, 9);
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void FillWorkStagesByNodes(SQLiteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = AnalyseQueries.WorkStagesByNodes;
            try
            {
                CurrentColumn = 1;
                CurrentRow = 104;

                WriteRows(cmd, 9);
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void WriteRows(SQLiteCommand cmd, int colCount)
        {
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < colCount; i++)
                {
                    WriteCell(reader[i]);
                }
                MoveToNextRow();
            }
            reader.Close();
        }
    }
}