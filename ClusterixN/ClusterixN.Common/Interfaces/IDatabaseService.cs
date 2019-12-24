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

﻿namespace ClusterixN.Common.Interfaces
{
    /// <summary>
    ///     Интерфейс сервиса БД.
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        ///     Получение БД для указанной строки подключения.
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="connectionId">Идентификатор соединения</param>
        /// <param name="newInstance">создать новый экземпляр</param>
        /// <returns>Интерфейс БД.</returns>
        IDatabase GetDatabase(string connectionString, string connectionId, bool newInstance = false);
    }
}