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
 using System.Net;
 using ClusterixN.Common.Data.EventArgs;

namespace ClusterixN.Network.Interfaces
{
    /// <summary>
    /// Интерфейс сетевого взаимодействия для сервера
    /// </summary>
    public interface IServerCommunicator : ICommunicator
    {
        /// <summary>
        /// Событие отключения клиента
        /// </summary>
        event EventHandler<DisconnectEventArg> ClientDisconnected;

        /// <summary>
        /// Получить адрес узла по идентификатору
        /// </summary>
        /// <param name="nodeId">идентификатор узла</param>
        /// <returns>адрес узла</returns>
        IPEndPoint GetAddressByNodeId(Guid nodeId);

        /// <summary>
        /// Получить адреса узлов по идентификаторам
        /// </summary>
        /// <param name="nodeIds">идентификаторы узлов</param>
        /// <returns>адреса узлов</returns>
        IPEndPoint[] GetAddressesByNodeIds(params Guid[] nodeIds);
    }
}