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

namespace ClusterixN.JoinManager
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
            Client.RegisterSendPacketType<SelectResult>(SelectResultHandler);
        }

        public override void Connect()
        {
            base.Connect();

            var c = new ConfigurationHelper();
            var listener = c.GetListenerConfiguration("DefaultListener");
            StartServer(listener.Port);
        }

        #region Packet Send Handlers

        private void SendRelationPreparedPacket(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as RelationPreparedPacket;
            if (packet == null) return;

            SendPacket(packet);
        }

        private void JoinCompleteHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as JoinCompletePacket;
            if (packet == null) return;

            SendPacket(packet);
        }

        private void SelectResultHandler(PacketBase packetBase, IPEndPoint endPoint)
        {
            var packet = packetBase as SelectResult;
            if (packet == null) return;

            Logger.Trace($"Отправка результата IsLast={packet.IsLast} OrderNumber={packet.OrderNumber}");

            SendPacket(packet);
        }

        #endregion

        #region Packet handlers

        private void InfoRequestPacketHandler(PacketBase packetBase)
        {
            Logger.Trace("Отправка отчета о возможностях");
            var isHardNode = ServiceLocator.Instance.ConfigurationService.GetAppSetting("IsHardNode") == "1";
            var minRamAvaible = float.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("MinRamAvaible"));
            SendPacket(new InfoResponcePacket
            {
                NodeType = NodeType.Join,
                CpuCount = Environment.ProcessorCount,
                IsHardNode = isHardNode,
                MinRamAvaible = minRamAvaible
            });
        }

        #endregion
    }
}