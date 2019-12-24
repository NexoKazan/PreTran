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
using System.IO;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;

namespace ClusterixN.Common.Infrastructure.Base
{
    /// <summary>
    ///     Создает запросы на выполнение из теста TPC-H
    /// </summary>
    public abstract class QuerySourceManagerBase : IQuerySourceManager
    {
        protected readonly ILogger Logger;
        protected readonly Dictionary<int, QuerySource> Queries;
        private int _queryCount = 1;
        private readonly string _dirName; 

        /// <summary>
        /// Директория сохранения результатов
        /// </summary>
        public string DirName => _dirName;

        public QuerySourceManagerBase()
        {
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            Queries = new Dictionary<int, QuerySource>();
            Queries.Add(1, Q1);
            Queries.Add(2, Q2);
            Queries.Add(3, Q3);
            Queries.Add(4, Q4);
            Queries.Add(5, Q5);
            Queries.Add(6, Q6);
            Queries.Add(7, Q7);
            Queries.Add(8, Q8);
            Queries.Add(9, Q9);
            Queries.Add(10, Q10);
            Queries.Add(11, Q11);
            Queries.Add(12, Q12);
            Queries.Add(13, Q13);
            Queries.Add(14, Q14);

            _dirName = SystemTime.Now.ToString("yyyyMMdd_HHmmss");

            if (!Directory.Exists(_dirName)) Directory.CreateDirectory(_dirName);
        }

        /// <summary>
        /// Записывает результат выполнения запроса в файл csv
        /// </summary>
        /// <param name="queryNumber">порядковый номер запроса</param>
        /// <param name="result">результат запроса</param>
        public void WriteResult(int queryNumber, string result)
        {
            File.WriteAllText($"{_dirName}{Path.DirectorySeparatorChar}{queryNumber:0000}.csv", result);
        }

        /// <summary>
        ///     Получает новый запрос по номеру от 1 до 14
        /// </summary>
        /// <param name="number">номер запроса</param>
        /// <returns>запрос на выполнение</returns>
        public Query GetQueryByNumber(int number)
        {
            return NewQuery(number);
        }

        /// <summary>
        ///     Создает новый запрос по номеру от 1 до 14
        /// </summary>
        /// <param name="number">номер запроса</param>
        /// <returns>запрос на выполнение</returns>
        protected Query NewQuery(int number)
        {
            if (Queries.ContainsKey(number))
            {
                var query = Queries[number].Invoke();
                query.SequenceNumber = _queryCount++;
                Logger.Info("Создан новый запрос: " + Environment.NewLine + ObjectDumper.Dump(query));
                query.Save($"{_dirName}{Path.DirectorySeparatorChar}{query.SequenceNumber:0000}_{query.Number:00}.xml");
                return query;
            }
            Logger.Error("нет запроса для номера " + number);
            return null;
        }

        /// <summary>
        ///     Делегат создания нового запроса
        /// </summary>
        /// <returns>созданный запрос</returns>
        protected delegate Query QuerySource();

        #region Queries

        /// <summary>
        ///     Запрос № 1 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q1();

        /// <summary>
        ///     Запрос № 2 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q2();

        /// <summary>
        ///     Запрос № 3 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q3();

        /// <summary>
        ///     Запрос № 4 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q4();

        /// <summary>
        ///     Запрос № 5 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q5();

        /// <summary>
        ///     Запрос № 6 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q6();

        /// <summary>
        ///     Запрос № 7 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q7();

        /// <summary>
        ///     Запрос № 8 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q8();

        /// <summary>
        ///     Запрос № 9 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q9();

        /// <summary>
        ///     Запрос № 10 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q10();

        /// <summary>
        ///     Запрос № 11 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q11();

        /// <summary>
        ///     Запрос № 12 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q12();

        /// <summary>
        ///     Запрос № 13 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q13();

        /// <summary>
        ///     Запрос № 14 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected abstract Query Q14();

        #endregion
    }
}