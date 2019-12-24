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
using ClusterixN.Network;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Sort
{
    internal sealed class Client : ClientBase
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

            Client.RegisterSendPacketType<RelationPreparedPacket>(RelationPrepareCompleteHandler);
            Client.RegisterSendPacketType<SortCompletePacket>(SortCompleteEventHandler);
        }

        public override void Connect()
        {
            base.Connect();

            var c = new ConfigurationHelper();
            var listener = c.GetListenerConfiguration("DefaultListener");
            StartServer(listener.Port);
        }

        #region Packet handlers

        private void InfoRequestPacketHandler(PacketBase packetBase)
        {
            Logger.Trace("Отправка отчета о возможностях");
            int minRamAvaible = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("MinRamAvaible"));
            var cpuCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkCoresCount"));
            var c = new ConfigurationHelper();
            var listener = c.GetListenerConfiguration("DefaultListener");
            SendPacket(new InfoResponcePacket
            {
                NodeType = NodeType.Sort,
                CpuCount = cpuCount,
                MinRamAvaible = minRamAvaible,
                ServerPort = listener.Port
            });
        }

        #endregion

        #region Events

        #endregion

        #region Packets Handlers

        private void RelationPrepareCompleteHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as RelationPreparedPacket;
            if (packet == null) return;

            SendPacket(packet, endPoint);
        }

        private void SortCompleteEventHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as SortCompletePacket;
            if (packet == null) return;

            Logger.Trace($"Отправка завершения операции SORT для {packet.NewRelationId}");
            SendPacket(packet, endPoint);
        }

        #endregion
    }
}