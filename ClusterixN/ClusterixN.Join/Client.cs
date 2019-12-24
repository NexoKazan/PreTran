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
 using ClusterixN.Common;
using ClusterixN.Common.Configuration;
using ClusterixN.Common.Data.Enums;
 using ClusterixN.Common.Data.Log.Enum;
 using ClusterixN.Common.Utils.LogServices;
 using ClusterixN.Network;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Join
{
    internal class Client : ClientBase
    {
        public Client()
        {
            var c = new ConfigurationHelper();
            var connection = c.GetConnectionConfiguration("DefaultServer");
            PortNumber = connection.Port;
            Address = connection.Address;
            InitPacketHandlers();
        }

        private void InitPacketHandlers()
        {
            SubscribeToPacket<InfoRequestPacket>(InfoRequestPacketHandler);

            Client.RegisterSendPacketType<RelationPreparedPacket>(SendRelationPreparedPacket);
            Client.RegisterSendPacketType<JoinCompletePacket>(JoinCompleteHandler);
            Client.RegisterSendPacketType<IntegratedJoinCompletePacket>(IntegratedJoinCompleteHandler);
            Client.RegisterSendPacketType<RelationDataPacket>(RelationDataPacketHandler);
            Client.RegisterSendPacketType<HashedRelationDataPacket>(SendHashedRelationDataPacket);
        }

        public override void Connect()
        {
            base.Connect();

            var c = new ConfigurationHelper();
            var listener = c.GetListenerConfiguration("DefaultListener");
            StartServer(listener.Port);
        }

        #region Packet Send Handlers

        private void SendHashedRelationDataPacket(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as HashedRelationDataPacket;
            if (packet == null) return;

            Logger.Trace(
                $"Отправка хешированных данных для отношения {packet.RelationId} в {packet.Id.ClientId}, IsLast = {packet.IsLast}, OrderNumber = {packet.OrderNumber}, HashNumber = {packet.HashNumber}");

            var endpoints = Client.GetClientEndPoint();
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                packet.QueryId, packet.SubQueryId, packet.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }
        
        private void RelationDataPacketHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as RelationDataPacket;
            if (packet == null) return;

            Logger.Trace(
                $"Отправка данных для отношения {packet.RelationId} в {packet.Id.ClientId}, IsLast = {packet.IsLast}, OrderNumber = {packet.OrderNumber}");

            var endpoints = Client.GetClientEndPoint();
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DataTransfer,
                packet.QueryId, packet.SubQueryId, packet.RelationId, endpoints.Item1.ToString(),
                endPoint?.ToString() ?? endpoints.Item2.ToString());

            SendPacket(packet, endPoint);

            timeLog.Stop();
        }

        private void SendRelationPreparedPacket(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as RelationPreparedPacket;
            if (packet == null) return;

            SendPacket(packet, endPoint);
        }

        private void JoinCompleteHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as JoinCompletePacket;
            if (packet == null) return;

            SendPacket(packet, endPoint);
        }

        private void IntegratedJoinCompleteHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as IntegratedJoinCompletePacket;
            if (packet == null) return;

            SendPacket(packet, endPoint);
        }

        #endregion

        #region Packet handlers

        private void InfoRequestPacketHandler(PacketBase packetBase)
        {
            Logger.Trace("Отправка отчета о возможностях");
            var isHardNode = ServiceLocator.Instance.ConfigurationService.GetAppSetting("IsHardNode") == "1";
            var minRamAvaible = float.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("MinRamAvaible"));
            var cpuCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkCoresCount"));
            var c = new ConfigurationHelper();
            var listener = c.GetListenerConfiguration("DefaultListener");
            SendPacket(new InfoResponcePacket
            {
                NodeType = NodeType.Join,
                CpuCount = cpuCount,
                IsHardNode = isHardNode,
                MinRamAvaible = minRamAvaible,
                ServerPort = listener.Port
            });
        }

        #endregion

        #region Events

        #endregion

        #region EventHandlers

        #endregion
    }
}