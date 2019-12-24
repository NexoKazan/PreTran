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

﻿using System.Collections.Generic;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Base;
using ClusterixN.QueryProcessing.Services.Interfaces;

namespace ClusterixN.QueryProcessing.Services
{
    class CommandService : ServiceBase, ICommandService
    {
        private List<IService> _subscribers;
        public CommandService(ICommunicator client, QueryProcessConfig dbConfig) : base(client, dbConfig)
        {
            _subscribers = new List<IService>();
            Client.SubscribeToPacket<CommandPacket>(CommandReceivedHandler);
            Client.SubscribeToPacket<DropQueryPacket>(DropQueryEventHandler);
        }

        private void CommandReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as CommandPacket;
            if (packet == null) return;

            var command = (Command) packet.Command;
            Logger.Info($"Получена команда {command}");

            foreach (var subscriber in _subscribers)
            {
                var canDrop = subscriber as ICanPauseWork;
                canDrop?.Pause(command == Command.Pause);
            }
        }

        private void DropQueryEventHandler(PacketBase packetBase)
        {
            var packet = packetBase as DropQueryPacket;
            if (packet == null) return;

            Logger.Info($"Получена команда удаления запроса {packet.QueryId} {packet.SubQueryId} {packet.RelationId}");

            foreach (var subscriber in _subscribers)
            {
                var canDrop = subscriber as ICanCancelQuery;
                canDrop?.CancelQuery(packet.QueryId, packet.SubQueryId, packet.RelationId);
            }
        }

        public void Subscribe(IService service)
        {
            _subscribers.Add(service);
        }
    }
}
