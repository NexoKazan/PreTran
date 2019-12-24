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
    /// Статусы обработки отношений
    /// </summary>
    public enum QueryRelationStatus
    {
        /// <summary>
        /// Ожидание
        /// </summary>
        Wait,

        /// <summary>
        /// Подготовка
        /// </summary>
        Preparing,
        
        /// <summary>
        /// Подготовка завершена
        /// </summary>
        Prepared,

        /// <summary>
        /// Передача данных
        /// </summary>
        TransferData,

        /// <summary>
        /// <para>Ожидание другого отношения</para>
        /// <para>Если отношение является результатом работы join, то оно устанавливается в этот статус</para>
        /// </summary>
        WaitAnotherRelation,

        /// <summary>
        /// Передача завершена
        /// </summary>
        Transfered,

        /// <summary>
        /// Идет обработка отношения
        /// </summary>
        Processing,

        /// <summary>
        /// Обработка отношения заврешена
        /// </summary>
        Processed
    }
}