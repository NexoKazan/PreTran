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

namespace ClusterixN.Common.Interfaces
{
    /// <summary>
    ///     Интерфейс сервиса логгирования.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        ///     Получение логгера для указанного источника.
        /// </summary>
        /// <param name="source">Имя источника.</param>
        /// <returns>Интерфейс логгера.</returns>
        ILogger GetLogger(string source);

        /// <summary>
        ///     Получение логгера для указанного источника.
        /// </summary>
        /// <param name="sourceType">Тип объекта источника.</param>
        /// <returns>Интерфейс логгера.</returns>
        ILogger GetLogger(Type sourceType);

        /// <summary>
        ///     Получение логгера для указанного источника.
        /// </summary>
        /// <param name="sourceType">Тип объекта источника.</param>
        /// <param name="instanceName">Имя экземпляра объекта.</param>
        /// <returns>Интерфейс логгера.</returns>
        ILogger GetLogger(Type sourceType, string instanceName);
    }
}