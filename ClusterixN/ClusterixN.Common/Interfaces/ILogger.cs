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
    ///     Интерфейс логгера.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     Логирование трассировочного сообщения
        /// </summary>
        /// <param name="message"></param>
        void Trace(string message);

        /// <summary>
        ///     Логирование отладочного сообщения.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        void Debug(string message);

        /// <summary>
        ///     Логирование информационного сообщения.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        void Info(string message);

        /// <summary>
        ///     Логирование предупреждения.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        void Warning(string message);

        /// <summary>
        ///     Логирование ошибки.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        void Error(string message);

        /// <summary>
        ///     Логирование ошибки.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="ex">Исключение.</param>
        void Error(string message, Exception ex);

        /// <summary>
        ///     Логирование ошибки.
        /// </summary>
        /// <param name="ex">Исключение.</param>
        void Error(Exception ex);

        /// <summary>
        ///     Логирование фатальной ошибки
        /// </summary>
        /// <param name="message"></param>
        void Fatal(string message);

        /// <summary>
        ///     Логирование фатальной ошибки
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Fatal(string message, Exception ex);


        /// <summary>
        ///     Логгирование фатальной ошибки
        /// </summary>
        /// <param name="ex"></param>
        void Fatal(Exception ex);

        /// <summary>
        ///     Логгирование события с кастомными полями
        /// </summary>
        /// <param name="logEvent"></param>
        void LogEvent(ILogEvent logEvent);
    }
}