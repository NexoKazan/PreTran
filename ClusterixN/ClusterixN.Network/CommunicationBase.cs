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
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Network.Data;
using ClusterixN.Network.Data.EventArgs;
using ClusterixN.Network.Packets.Base;
using ProtoBuf;

namespace ClusterixN.Network
{
    public abstract class CommunicationBase
    {
        protected readonly Dictionary<Type, List<Action<PacketBase>>> KnownReceivePacketTypes;
        protected readonly Dictionary<Type, Action<PacketBase, IPEndPoint>> KnownSendPacketTypes;
        protected ILogger Logger;
        private readonly Dictionary<string, TcpClient> _connectionCache;
        private readonly TaskSequenceHelper _taskSequenceHelper;
        private readonly TaskSequenceHelper _receiveSequenceHelper;
        private object _syncObject = new object();

        public CommunicationBase()
        {
            _taskSequenceHelper = new TaskSequenceHelper();
            _receiveSequenceHelper = new TaskSequenceHelper();
            Logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            KnownReceivePacketTypes = new Dictionary<Type, List<Action<PacketBase>>>();
            KnownSendPacketTypes = new Dictionary<Type, Action<PacketBase, IPEndPoint>>();
            _connectionCache = new Dictionary<string, TcpClient>();
        }

        public void RegisterSendPacketType<T>(Action<PacketBase, IPEndPoint> action) where T : PacketBase
        {
            if (KnownSendPacketTypes.ContainsKey(typeof(T)))
            {
                Logger.Trace($"Изменен обработчик для пакета: {typeof(T)}");
                KnownSendPacketTypes[typeof(T)] = action;
            }
            else
            {
                Logger.Trace($"Зарегистрирован новый тип пакета: {typeof(T)}");
                KnownSendPacketTypes.Add(typeof(T), action);
            }
        }

        public void RegisterReceivePacketType<T>(Action<PacketBase> action) where T : PacketBase
        {
            if (KnownReceivePacketTypes.ContainsKey(typeof(T)))
            {
                Logger.Trace($"добавлен обработчик для пакета: {typeof(T)}");
                KnownReceivePacketTypes[typeof(T)].Add(action);
            }
            else
            {
                Logger.Trace($"Зарегистрирован новый тип пакета: {typeof(T)}");
                KnownReceivePacketTypes.Add(typeof(T), new List<Action<PacketBase>>() {action});
            }
        }

        protected void InvokeReceivePacketActionAsync(PacketBase packet)
        {
            if (packet == null) return;

            if (KnownReceivePacketTypes.ContainsKey(packet.PacketType))
            {
                _receiveSequenceHelper.AddTask(new Task((obj) =>
                {
                    foreach (var action in KnownReceivePacketTypes[packet.PacketType])
                        action.Invoke((PacketBase)obj);
                }, packet));
            }
            else
                Logger.Error($"Незарегистрированный тип пакета: {packet.PacketType}");
        }

        protected void InvokeSendPacketAction(PacketBase packet, IPEndPoint endPoint)
        {
            if (packet == null) return;

            if (KnownSendPacketTypes.ContainsKey(packet.PacketType))
            {
                KnownSendPacketTypes[packet.PacketType].Invoke(packet, endPoint);
            }
            else
                Logger.Error($"Незарегистрированный тип пакета: {packet.PacketType}");
        }

        protected void InvokeSendPacketActionAsyncQueue(PacketBase packet, IPEndPoint endPoint)
        {
            _taskSequenceHelper.AddTask(new Task(obj =>
            {
                var tup = (Tuple<PacketBase, IPEndPoint>)obj;
                InvokeSendPacketAction(tup.Item1, tup.Item2);
            }, new Tuple<PacketBase, IPEndPoint>(packet, endPoint)));
        }

        protected PacketBase ReceivePacket(NetworkStream networkStream)
        {
            return Serializer.DeserializeWithLengthPrefix<PacketBase>(networkStream, PrefixStyle.Base128);
        }
        
        protected void SendPacket(NetworkStream networkStream, PacketBase packet)
        {
            Serializer.SerializeWithLengthPrefix(networkStream, packet, PrefixStyle.Base128);
        }

        protected void ClientListenThread(object obj)
        {
            var client = obj as NetworkClientData;
            if (client == null) return;

            var tcpClient = client.Client;
            var endpoint = tcpClient.Client.RemoteEndPoint.ToString();
            while (tcpClient.Connected)
            {
                try
                {
                    if (tcpClient.Available > 0)
                    {
                        var packet = ReceivePacket(tcpClient.GetStream());
                        if (packet != null)
                        {
                            Logger.Trace($"Получен пакет {packet.PacketType} от: {endpoint}");
                            packet.Id.ClientId = client.ClientId;
                            InvokeReceivePacketActionAsync(packet);
                        }
                        else
                        {
                            Logger.Error("Получен пустой пакет");
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                        if (tcpClient.Client.Poll(1000, SelectMode.SelectRead) && tcpClient.Available == 0)
                        {
                            tcpClient.Close();
                            break;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка связи с клиентом: {endpoint}", ex);
                    break;
                }
            }

            OnDisconnect(new ClientEvenArg(client.ClientId));
        }

        public bool SendPacketToAddress(PacketBase packet, IPEndPoint endPoint)
        {
            var result = true;

            lock (_syncObject)
            {
                TcpClient client;

                if (_connectionCache.ContainsKey(endPoint.ToString()))
                {
                    client = _connectionCache[endPoint.ToString()];
                }
                else
                {
                    client = new TcpClient();
                    result = ConnectToAddress(client, endPoint);

                    if (result)
                    {
                        _connectionCache.Add(endPoint.ToString(), client);
                    }
                }

                if (!client.Connected)
                {
                    result = ConnectToAddress(client, endPoint);
                }

                if (result)
                {
                    result = SendToAddress(client, packet);
                }

            }

            return result;
        }

        private bool ConnectToAddress(TcpClient client, IPEndPoint endPoint)
        {
            const int numberOfRetries = 10;
            var result = false;
            var exp = new Exception();
            Logger.Info($"Установка соединения с {endPoint}...");

            for (var i = 1; i <= numberOfRetries; ++i)
            {
                try
                {
                    client.Connect(endPoint);
                    result = true;
                    break;
                }
                catch (Exception ex) when (i <= numberOfRetries)
                {
                    Logger.Warning($"Соединение с {endPoint} не удалась. Повтор...");
                    exp = ex;
                    Thread.Sleep(10);
                }
            }

            if (!result) Logger.Error($"Ошибка соединения с {endPoint}", exp);

            return result;
        }

        private bool SendToAddress(TcpClient client, PacketBase packet)
        {
            const int numberOfRetries = 10;
            var result = false;
            var exp = new Exception();

            for (var i = 1; i <= numberOfRetries; ++i)
            {
                try
                {
                    SendPacket(client.GetStream(), packet);
                    result = true;
                    break;
                }
                catch (Exception ex) when (i <= numberOfRetries)
                {
                    Logger.Warning($"Передача в {client.Client?.RemoteEndPoint} не удалась. Повтор...");
                    exp = ex;
                    Thread.Sleep(10);
                }
            }

            if (!result) Logger.Error($"Ошибка передачи", exp);

            return result;
        }

        #region Events

        public event EventHandler<ClientEvenArg> Disconnected;

        protected virtual void OnDisconnect(ClientEvenArg e)
        {
            Disconnected?.Invoke(this, e);
        }

        #endregion
    }
}