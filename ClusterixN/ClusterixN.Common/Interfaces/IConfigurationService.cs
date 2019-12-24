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

using System;

namespace ClusterixN.Common.Interfaces
{
    /// <summary>
    ///     Интерфейс сервиса конфигурации.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        ///     Получение пути до конфигурации компонента
        /// </summary>
        /// <returns>Конфигурация.</returns>
        string GetPathToConfiguration();

        /// <summary>
        ///     Получение значения из секции appSettings.
        /// </summary>
        /// <param name="key">Имя параметра.</param>
        /// <returns>Значение параметра.</returns>
        string GetAppSetting(string key);

        /// <summary>
        ///     Получение всех ConnectionStrings
        /// </summary>
        /// <returns>Перечень всех строк подключения</returns>
        string[] GetAllConnetctionStrings();

        /// <summary>
        ///     Получение всех ConnectionStrings с заданными ограничениями имени
        /// </summary>
        /// <param name="nameConstrains">Ограничения имени</param>
        /// <returns>Перечень всех строк подключения</returns>
        string[] GetConnetctionStrings(Func<string, bool> nameConstrains);
    }
}