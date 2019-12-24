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

﻿namespace ClusterixN.Common.Data.Query.Enum
{
    /// <summary>
    /// Статусы всего (не разделенного) запроса
    /// </summary>
    public enum QueryStatus
    {
        /// <summary>
        /// Ожидание
        /// </summary>
        Wait,

        /// <summary>
        /// Обработка подзапроса select
        /// </summary>
        SelectProcessing,

        /// <summary>
        /// Передача результата подзапроса select в узел управления
        /// </summary>
        TransferSelectResult,

        /// <summary>
        /// Обработка подзапроса select завершена
        /// </summary>
        SelectProcessed,

        /// <summary>
        /// Передача результата подзапроса select в узел join
        /// </summary>
        TransferToJoin,

        /// <summary>
        /// Обработка подзапроса join
        /// </summary>
        JoinProcessing,

        /// <summary>
        /// Обработка подзапроса join завершена
        /// </summary>
        JoinProcessed,

        /// <summary>
        /// Передача результата подзапросов join в узел управления
        /// </summary>
        TransferJoinResult,

        /// <summary>
        /// Передача результата подзапросов join в узел управления завершена
        /// </summary>
        JoinResultTransfered,

        /// <summary>
        /// Передача результата подзапросов join в узел sort
        /// </summary>
        TransferToSort,

        /// <summary>
        /// Передача результата подзапросов join в узел завершена
        /// </summary>
        TransferedToSort,

        /// <summary>
        /// Обработка подзапроса sort
        /// </summary>
        ProcessingSort,

        /// <summary>
        /// Обработка подзапроса sort завершена
        /// </summary>
        SortProcessed,

        /// <summary>
        /// Передача результата подзапросов sort в узел управления
        /// </summary>
        TransferSortResult,

        /// <summary>
        /// Передача результата подзапросов sort в узел управления завершена
        /// </summary>
        SortResultTransfered,

        /// <summary>
        /// Запрос выполнен
        /// </summary>
        Ready
    }
}