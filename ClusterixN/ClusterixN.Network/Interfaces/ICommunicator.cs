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
 using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Network.Interfaces
{
     /// <summary>
     /// Интерфейс сетевого взаимодействия
     /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Отправить пакет в сеть
        /// </summary>
        /// <param name="packetBase">пакет</param>
        void Send(PacketBase packetBase);

        /// <summary>
        /// Отправить пакет в сеть через асинхронную очередь
        /// </summary>
        /// <param name="packetBase">пакет</param>
        void SendAsyncQueue(PacketBase packetBase);

        /// <summary>
        /// Отправить пакет в сеть
        /// </summary>
        /// <param name="destination">адрес назначения пакета</param>
        /// <param name="packetBase">пакет</param>
        void Send(IPEndPoint destination, PacketBase packetBase);

        /// <summary>
        /// Отправить пакет в сеть через асинхронную очередь
        /// </summary>
        /// <param name="destination">адрес назначения пакета</param>
        /// <param name="packetBase">пакет</param>
        void SendAsyncQueue(IPEndPoint destination, PacketBase packetBase);

        /// <summary>
        /// Подписаться на получение пакета из сети
        /// </summary>
        /// <typeparam name="T">Тип пакета</typeparam>
        /// <param name="action">Метод обработки пакета при получении</param>
        void SubscribeToPacket<T>(Action<PacketBase> action) where T : PacketBase;
    }
}