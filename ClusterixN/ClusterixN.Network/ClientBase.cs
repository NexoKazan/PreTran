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
using System.Threading;
using ClusterixN.Common;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Common.Utils.PerformanceCounters;
using ClusterixN.Network.Data.EventArgs;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Network
{
    public abstract class ClientBase : ICommunicator
    {
        protected readonly ILogger Logger;
        protected string Address;
        protected NetworkClient Client;
        protected int PortNumber;
        protected NetworkServer Server;
        private DateTime _lastStatusSendedTime;

        protected ClientBase()
        {
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            Init();
        }

        /// <summary>
        ///     Отправить пакет в сеть
        /// </summary>
        /// <param name="packetBase">пакет</param>
        public void Send(PacketBase packetBase)
        {
            Client.InvokePacketSend(packetBase, null);
        }

        /// <summary>
        /// Отправить пакет в сеть через асинхронную очередь
        /// </summary>
        /// <param name="packetBase">пакет</param>
        public void SendAsyncQueue(PacketBase packetBase)
        {
            Client.InvokePacketSendAsyncQueue(packetBase, null);
        }

        /// <summary>
        /// Отправить пакет в сеть
        /// </summary>
        /// <param name="destination">адрес назначения пакета</param>
        /// <param name="packetBase">пакет</param>
        public void Send(IPEndPoint destination, PacketBase packetBase)
        {
            Client.InvokePacketSend(packetBase, destination);
        }

        /// <summary>
        /// Отправить пакет в сеть через асинхронную очередь
        /// </summary>
        /// <param name="destination">адрес назначения пакета</param>
        /// <param name="packetBase">пакет</param>
        public void SendAsyncQueue(IPEndPoint destination, PacketBase packetBase)
        {
            Client.InvokePacketSendAsyncQueue(packetBase, destination);
        }

        /// <summary>
        ///     Подписаться на получение пакета из сети
        /// </summary>
        /// <typeparam name="T">Тип пакета</typeparam>
        /// <param name="action">Метод обработки пакета при получении</param>
        public void SubscribeToPacket<T>(Action<PacketBase> action) where T : PacketBase
        {
            Client.RegisterReceivePacketType<T>(action);
            Server.RegisterReceivePacketType<T>(action);
        }

        private void Init()
        {
            Client = new NetworkClient();
            Server = new NetworkServer();

            SubscribeToPacket<GetFileRequestPacket>(GetFileRequestPacketHandler);
            SubscribeToPacket<TimeAdjustPacket>(TimeAdjustPacketHandler);

            Client.RegisterSendPacketType<SelectResult>(SendSelectResultPacket);
            Client.RegisterSendPacketType<GetFileResponcePacket>(SendGetFileResponcePacket);
            Client.RegisterSendPacketType<StatusPacket>(SendStatusPacket);
            Client.RegisterSendPacketType<QueryStatusPacket>(SendQueryStatusPacket);

            Client.Disconnected += ClientOnDisconnected;
            Server.Disconnected += ServerOnDisconnected;

            _lastStatusSendedTime = SystemTime.Now;

            PerformanceMonitor.Instance.NewValueAvaible += (sender, args) =>
            {
                var monitor = sender as PerformanceMonitor;
                if (monitor == null) return;

                PerformanceLogService.Instance.LogPerformance("CPU", monitor.CpuUsage);
                PerformanceLogService.Instance.LogPerformance("RAM", monitor.RamAvaible);

                PerformanceLogService.Instance.LogPerformance("Net_send", monitor.NetworkSendSpeed);
                PerformanceLogService.Instance.LogPerformance("Net_Receive", monitor.NetworkReceiveSpeed);

                if (SystemTime.Now - _lastStatusSendedTime > TimeSpan.FromSeconds(10))
                {
                    Send(new StatusPacket
                    {
                        AvaibleRam = monitor.RamAvaible,
                        CpuUsage = monitor.CpuUsage
                    });
                    _lastStatusSendedTime = SystemTime.Now;
                }
            };
        }

        public virtual void Connect()
        {
            Logger.Info($"Подключение к {Address}:{PortNumber}");
            while (!Client.Connect(Address, PortNumber))
            {
                Logger.Error($"Ошибка подключения к {Address}:{PortNumber}");
                Thread.Sleep(5000);
                Logger.Info($"Переподключение к {Address}:{PortNumber}");
            }
        }
        
        protected void StartServer(int portNumber)
        {
            Logger.Info("Запуск сервера...");
            Server.Start(portNumber);
            Logger.Info("Сервер запущен");
        }

        private void CheckConnection()
        {
            if (!Client.IsConnected)
            {
                Logger.Error($"Нет соединения с {Address}:{PortNumber}");
                Connect();
            }
        }

        protected bool SendPacket(PacketBase packet)
        {
            try
            {
                CheckConnection();
                return Client.SendPacket(packet);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка отправки пакета {packet.PacketType} на {Address}:{PortNumber}", ex);
            }
            return false;
        }

        protected bool SendPacket(PacketBase packet, IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                return SendPacket(packet);
            }

            try
            {
                return Client.SendPacketToAddress(packet, endPoint);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка отправки пакета {packet.PacketType} на {endPoint.Address}:{endPoint.Port}", ex);
            }
            return false;
        }

        public string GetLocalEndPoint()
        {
            return Client.GetClientEndPoint().Item1.ToString();
        }

        #region EventHandlers

        private void ClientOnDisconnected(object sender, ClientEvenArg clientEvenArg)
        {
            CheckConnection();
        }

        private void ServerOnDisconnected(object o, ClientEvenArg clientEvenArg)
        {
            Logger.Info($"Отключен клиент {clientEvenArg.ClientId}");
        }


        #endregion

        #region Packet Send Handlers

        private void SendSelectResultPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var queryPacket = packet as SelectResult;
            if (queryPacket == null) return;

            Logger.Trace(
                $"Отправка блока для {queryPacket.QueryId}, IsLast = {queryPacket.IsLast}, OrderNumber = {queryPacket.OrderNumber}");

            var endpoints = Client.GetClientEndPoint();
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer, queryPacket.QueryId,
                queryPacket.SubQueryId, queryPacket.RelationId, endpoints.Item1.ToString(), endpoints.Item2.ToString());

            SendPacket(queryPacket, endPoint);

            timeLog.Stop();
        }

        private void SendGetFileResponcePacket(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as GetFileResponcePacket;
            if (packet == null) return;

            Logger.Trace($"Отправка файла {packet.FileName}");

            SendPacket(packet, endPoint);
        }

        private void SendStatusPacket(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as StatusPacket;
            if (packet == null) return;

            Logger.Trace($"Отправка статуса. CPU: {packet.CpuUsage} RAM: {packet.AvaibleRam}");

            SendPacket(packet, endPoint);
        }

        private void SendQueryStatusPacket(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as QueryStatusPacket;
            if (packet == null) return;

            Logger.Trace($"Отправка нового статуса заппроса. QueryId: {packet.QueryId} SubQueryId: {packet.SubQueryId} RelationId: {packet.RelationId} Status: {packet.Status}");

            SendPacket(packet, endPoint);
        }

        #endregion

        #region Packet Handlers

        private void GetFileRequestPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as GetFileRequestPacket;
            if (packet == null) return;
            try
            {
                Send(new GetFileResponcePacket
                {
                    Data = FileHelper.ReadLockedFile(packet.FileName),
                    FileName = packet.FileName
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка чтения файла {packet.FileName}", ex);
            }
        }

        private void TimeAdjustPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as TimeAdjustPacket;
            if (packet == null) return;

            Logger.Info($"Получено системное время {packet.SystemTime}");
            SystemTime.SetOffsetBySystemTime(packet.SystemTime);
        }

        #endregion

        #region Events

        #endregion
    }
}