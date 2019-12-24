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
using System.Net.Sockets;
using System.Threading;
using ClusterixN.Network.Data;
using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Network
{
    public class NetworkClient : CommunicationBase
    {
        private TcpClient _server;
        private readonly object _syncObject = new object();
        private Thread _thread;

        public bool IsConnected => _server?.Client?.Connected ?? false;

        public bool Connect(string ip, int port)
        {
            if (_server != null && _server.Connected) return true;
            try
            {
                if (_thread != null && _thread.IsAlive) _thread.Abort();
                _server = new TcpClient();
                _server.Connect(ip, port);
                _thread = new Thread(ClientListenThread);
                _thread.Start(new NetworkClientData {Client = _server, ClientId = Guid.Empty});
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка подключения", ex);
                return false;
            }
            return true;
        }

        public void InvokePacketSend(PacketBase packet, IPEndPoint endPoint)
        {
            InvokeSendPacketAction(packet, endPoint);
        }
        
        public void InvokePacketSendAsyncQueue(PacketBase packet, IPEndPoint endPoint)
        {
            InvokeSendPacketActionAsyncQueue(packet, endPoint);
        }

        public bool SendPacket(PacketBase packet)
        {
            if (!_server.Connected) return false;
            lock (_syncObject)
            {
                var stream = _server.GetStream();
                SendPacket(stream, packet);
            }
            return true;
        }

        /// <summary>
        /// Возвращает локальную и удаленную точку
        /// </summary>
        /// <returns>item1 - локальная точка, item2 - удаленная точка</returns>
        public Tuple<EndPoint, EndPoint> GetClientEndPoint()
        {
            var socket = _server.Client;
            return new Tuple<EndPoint, EndPoint>(socket.LocalEndPoint, socket.RemoteEndPoint);
        }

        public void Disconnect()
        {
            _server.Close();
        }
    }
}