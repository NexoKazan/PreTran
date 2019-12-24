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

using ClusterixN.Common.Data.Query;

namespace ClusterixN.Common.Interfaces
{
    public interface IQuerySourceManager
    {
        /// <summary>
        ///     Получает новый запрос по номеру от 1 до 14
        /// </summary>
        /// <param name="number">номер запроса</param>
        /// <returns>запрос на выполнение</returns>
        Query GetQueryByNumber(int number);

        /// <summary>
        /// Записывает результат выполнения запроса в файл csv
        /// </summary>
        /// <param name="queryNumber">порядковый номер запроса</param>
        /// <param name="result">результат запроса</param>
        void WriteResult(int queryNumber, string result);

        /// <summary>
        /// Директория сохранения результатов
        /// </summary>
        string DirName { get; }
    }
}