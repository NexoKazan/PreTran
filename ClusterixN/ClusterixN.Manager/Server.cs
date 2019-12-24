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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ClusterixN.Common;
using ClusterixN.Common.Configuration;
using ClusterixN.Common.Data.EventArgs;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Network;
using ClusterixN.Network.Data.EventArgs;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Manager
{
    internal sealed class Server : IServerCommunicator
    {
        private readonly List<Guid> _clients;
        private readonly ILogger _logger;
        private readonly int _portNumber;
        private readonly NetworkServer _server;

        public Server()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _server = new NetworkServer();
            _clients = new List<Guid>();
            var c = new ConfigurationHelper();
            var listener = c.GetListenerConfiguration("DefaultListener");
            _portNumber = listener.Port;
            InitPacketHandlers();

            _server.ClientConnected += ServerOnClientConnected;
            _server.Disconnected += ServerOnDisconnected;
        }

        /// <summary>
        /// Отправить пакет в сеть
        /// </summary>
        /// <param name="packetBase">пакет</param>
        public void Send(PacketBase packetBase)
        {
            _server.SendPacket(packetBase, null);
        }

        /// <summary>
        /// Отправить пакет в сеть через асинхронную очередь
        /// </summary>
        /// <param name="packetBase">пакет</param>
        public void SendAsyncQueue(PacketBase packetBase)
        {
            _server.SendPacketAsyncQueue(packetBase, null);
        }

        /// <summary>
        /// Отправить пакет в сеть
        /// </summary>
        /// <param name="destination">адрес назначения пакета</param>
        /// <param name="packetBase">пакет</param>
        public void Send(IPEndPoint destination, PacketBase packetBase)
        {
            _server.SendPacket(packetBase, destination);
        }

        /// <summary>
        /// Отправить пакет в сеть через асинхронную очередь
        /// </summary>
        /// <param name="destination">адрес назначения пакета</param>
        /// <param name="packetBase">пакет</param>
        public void SendAsyncQueue(IPEndPoint destination, PacketBase packetBase)
        {
            _server.SendPacketAsyncQueue(packetBase, destination);
        }

        /// <summary>
        /// Подписаться на получение пакета из сети
        /// </summary>
        /// <typeparam name="T">Тип пакета</typeparam>
        /// <param name="action">Метод обработки пакета при получении</param>
        public void SubscribeToPacket<T>(Action<PacketBase> action) where T : PacketBase
        {
            _server.RegisterReceivePacketType<T>(action);
        }

        /// <summary>
        /// Получить адрес узла по идентификатору
        /// </summary>
        /// <param name="nodeId">идентификатор узла</param>
        /// <returns>адрес узла</returns>
        public IPEndPoint GetAddressByNodeId(Guid nodeId)
        {
            var endpoints = _server.GetClientEndPoint(nodeId);
            return endpoints.Item2 as IPEndPoint;
        }

        /// <summary>
        /// Получить адреса узлов по идентификаторам
        /// </summary>
        /// <param name="nodeIds">идентификаторы узлов</param>
        /// <returns>адреса узлов</returns>
        public IPEndPoint[] GetAddressesByNodeIds(params Guid[] nodeIds)
        {
            var ips = new List<IPEndPoint>();
            foreach (var nodeId in nodeIds)
            {
                ips.Add(GetAddressByNodeId(nodeId));
            }

            return ips.ToArray();
        }

        public void Start()
        {
            _logger.Info("Запуск сервера...");
            _server.Start(_portNumber);
            _logger.Info("Сервер запущен.");
        }

        public void Stop()
        {
            _logger.Info("Остановка сервера...");
            _server.Stop();
            _logger.Info("Сервер остановлен.");
        }

        private void InitPacketHandlers()
        {
            _logger.Info("Инициализация обработчиков пакетов....");

            _server.RegisterReceivePacketType<InfoResponcePacket>(InfoResponcePacketHandler);

            _server.RegisterSendPacketType<QueryPacket>(SendQueryPacket);
            _server.RegisterSendPacketType<RelationPreparePacket>(SendRelationPreparePacket);
            _server.RegisterSendPacketType<RelationDataPacket>(SendRelationDataPacket);
            _server.RegisterSendPacketType<JoinStartPacket>(SendJoinStartPacket);
            _server.RegisterSendPacketType<IntegratedJoinStartPacket>(SendIntegratedJoinStartPacket);
            _server.RegisterSendPacketType<SortStartPacket>(SendSortStartPacket);
            _server.RegisterSendPacketType<GetFileRequestPacket>(SendGetFileRequestPacket);
            _server.RegisterSendPacketType<CommandPacket>(SendCommandPacket);
            _server.RegisterSendPacketType<DropQueryPacket>(SendDropQueryPacket);
            _server.RegisterSendPacketType<SelectAndSendPacket>(SendSelectAndSendPacket);
            _server.RegisterSendPacketType<MultiNodeJoinStartPacket>(SendMultiNodeJoinStartPacket);
        }

        private void SendPacket(PacketBase packet, IPEndPoint endPoint)
        {
            if (endPoint == null) _server.SendPacketToClient(packet, packet.Id.ClientId);
            else _server.SendPacketToAddress(packet, endPoint);
        }

        #region Packet Handlers

        private void InfoResponcePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as InfoResponcePacket;
            if (packet == null) return;

            _logger.Info($"Узел: {packet.NodeType} Емкость: {packet.CpuCount}");
            _server.SendPacketToClient(new TimeAdjustPacket {SystemTime = DateTime.Now}, packet.Id.ClientId);
        }

        #endregion

        #region Packet Send Handlers

        private void SendQueryPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var queryPacket = packet as QueryPacket;
            if (queryPacket == null) return;

            _logger.Trace($"Отправка запроса {queryPacket.QueryNumber}: {queryPacket.Query} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                queryPacket.QueryId, queryPacket.SubQueryId, queryPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendRelationPreparePacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as RelationPreparePacket;
            if (typedPacket == null) return;

            _logger.Trace(
                $"Отправка задания на создание отношения {typedPacket.QueryNumber} {typedPacket.RelationName}: {typedPacket.RelationShema} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendRelationDataPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as RelationDataPacket;
            if (typedPacket == null) return;

            _logger.Trace(
                $"Отправка данных для отношения {typedPacket.QueryNumber} {typedPacket.RelationId} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}, IsLast = {typedPacket.IsLast}, OrderNumber = {typedPacket.OrderNumber}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendJoinStartPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as JoinStartPacket;
            if (typedPacket == null) return;

            _logger.Trace($"Старт JOIN {typedPacket.QueryNumber} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendMultiNodeJoinStartPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as MultiNodeJoinStartPacket;
            if (typedPacket == null) return;

            _logger.Trace($"Старт многоузлового JOIN {typedPacket.QueryNumber} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendIntegratedJoinStartPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as IntegratedJoinStartPacket;
            if (typedPacket == null) return;

            _logger.Trace($"Старт JOIN {typedPacket.QueryNumber} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendSortStartPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as SortStartPacket;
            if (typedPacket == null) return;

            _logger.Trace($"Старт SORT {typedPacket.QueryNumber} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendGetFileRequestPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as GetFileRequestPacket;
            if (typedPacket == null) return;

            _logger.Trace($"Запрос файла {typedPacket.FileName} с {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            SendPacket(packet, endPoint);
        }

        private void SendCommandPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as CommandPacket;
            if (typedPacket == null) return;

            _logger.Trace($"Отправка команды {typedPacket.Command} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            SendPacket(packet, endPoint);
        }

        private void SendDropQueryPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as DropQueryPacket;
            if (typedPacket == null) return;

            _logger.Trace(
                $"Отправка запроса на удаление запроса {typedPacket.QueryId} {typedPacket.SubQueryId} {typedPacket.RelationId} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");
            SendPacket(packet, endPoint);
        }

        private void SendSelectAndSendPacket(PacketBase packet, IPEndPoint endPoint)
        {
            var typedPacket = packet as SelectAndSendPacket;
            if (typedPacket == null) return;

            _logger.Trace(
                $"Отправка запроса с передачей напрямую в JOIN для {typedPacket.QueryId} {typedPacket.SubQueryId} {typedPacket.RelationId} в {endPoint?.ToString() ?? packet.Id.ClientId.ToString()}");

            var endpoints = _server.GetClientEndPoint(packet.Id.ClientId);
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                typedPacket.QueryId, typedPacket.SubQueryId, typedPacket.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());


            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        #endregion

        #region EventHandlers

        private void ServerOnClientConnected(object sender, ClientEvenArg clientEvenArg)
        {
            _logger.Trace("Запрос возможностей узла");
            _server.SendPacketToClient(new InfoRequestPacket(), clientEvenArg.ClientId);

            _clients.Add(clientEvenArg.ClientId);
        }

        private void ServerOnDisconnected(object sender, ClientEvenArg clientEvenArg)
        {
            _logger.Trace($"Клиент отключился {clientEvenArg.ClientId}");
            if (_clients.Any(c => c == clientEvenArg.ClientId)) _clients.Remove(clientEvenArg.ClientId);
            OnClientDisconnected(new DisconnectEventArg {ClientId = clientEvenArg.ClientId});
        }

        #endregion

        #region Events

        public event EventHandler<DisconnectEventArg> ClientDisconnected;

        private void OnClientDisconnected(DisconnectEventArg e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        #endregion
    }
}