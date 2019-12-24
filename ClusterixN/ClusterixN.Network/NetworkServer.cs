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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClusterixN.Network.Data;
using ClusterixN.Network.Data.EventArgs;
using ClusterixN.Network.Packets.Base;

namespace ClusterixN.Network
{
    public class NetworkServer : CommunicationBase
    {
        private readonly Dictionary<Guid, TcpClient> _clients;
        private readonly List<Thread> _clientThreads;
        private readonly Thread _thread;
        private TcpListener _server;
        private readonly object _syncObject = new object();
        private bool _threadLive;

        public NetworkServer()
        {
            _clients = new Dictionary<Guid, TcpClient>();
            _clientThreads = new List<Thread>();
            _thread = new Thread(Listen);
        }

        private void Listen()
        {
            while (_threadLive)
            {
                try
                {
                    var tcpClient = _server.AcceptTcpClient();
                    var clientId = Guid.NewGuid();
                    var serverThread = new Thread(ClientListenThread);
                    _clients.Add(clientId, tcpClient);
                    _clientThreads.Add(serverThread);
                    serverThread.Start(new NetworkClientData {Client = tcpClient, ClientId = clientId});

                    Logger.Info($"Подключен новый клиент: {tcpClient.Client.RemoteEndPoint} id: {clientId}");
                    OnClientConnected(new ClientEvenArg(clientId));
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка сервера", ex);
                }
            }
        }

        /// <summary>
        /// Возвращает локальную и удаленную точку для указанного клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns>item1 - локальная точка, item2 - удаленная точка</returns>
        public Tuple<EndPoint,EndPoint> GetClientEndPoint(Guid clientId)
        {
            if (_clients.ContainsKey(clientId))
            {
                var socket = _clients[clientId].Client;
                return new Tuple<EndPoint, EndPoint>(socket.LocalEndPoint, socket.RemoteEndPoint);
            }
            return null;
        }

        public void Start(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            _threadLive = true;
            _thread.Start();
        }

        public void Stop()
        {
            _threadLive = false;

            if (!_thread.Join(1000))
            {
                _thread.Abort();
            }

            foreach (var client in _clients)
                client.Value.Close();
            foreach (var clientThread in _clientThreads)
                if (!clientThread.Join(5000)) clientThread.Abort();

            _server.Stop();
        }

        public void SendPacket(PacketBase packet, IPEndPoint endPoint)
        {
            InvokeSendPacketAction(packet, endPoint);
        }

        public void SendPacketAsyncQueue(PacketBase packetBase, IPEndPoint endPoint)
        {
            InvokeSendPacketActionAsyncQueue(packetBase, endPoint);
        }

        public bool SendPacketToClient(PacketBase packet, Guid clientId)
        {
            var result = true;
            if (_clients.ContainsKey(clientId))
            {
                try
                {
                    lock (_syncObject)
                    {
                        var stream = _clients[clientId].GetStream();
                        SendPacket(stream, packet);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error("Ошибка отправки пакета", exception);
                    result = false;
                }
            }
            else
            {
                Logger.Error($"Не найден клиент {clientId}");
                result = false;
            }

            return result;
        }
        
        #region Events

        public event EventHandler<ClientEvenArg> ClientConnected;

        protected virtual void OnClientConnected(ClientEvenArg e)
        {
            ClientConnected?.Invoke(this, e);
        }

        #endregion

    }
}