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
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.EventArgs;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Hasher;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Common.Utils.Task;
using ClusterixN.JoinManager.Data;
using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.JoinManager
{
    public class HashJoinManager
    {
        private readonly ILogger _logger;
        private readonly ICommunicator _client;
        private readonly IServerCommunicator _server;
        private readonly RelationManager _relationManager;
        private readonly List<JoinNode> _nodes;
        private readonly HashedBufferManager _bufferManager;
        private readonly ParallelQeueHelper _joinRehashSequence;
        private readonly TaskSequenceHelper _resultSendSequence;
        private readonly MultiNodeProcessingManager _multiNodeProcessingManager;
        private readonly MultiNodeProcessingManager _joinCompleteProcessingManager;
        private readonly int _hashCount;
        private readonly List<Guid> _receivedRelations;
        private readonly Dictionary<Guid, int> _tasksInProgress;
        private readonly object _receivedRelationsSync = new object();
        private readonly object _tasksInProgressSync = new object();

        public HashJoinManager(ICommunicator client, IServerCommunicator server)
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _hashCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("HashOnJoin"));
            var hashQueueCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("HashQueueCount"));
            _client = client;
            _server = server;
            _tasksInProgress = new Dictionary<Guid, int>();
            _relationManager = new RelationManager();
            _nodes = new List<JoinNode>();
            _bufferManager = new HashedBufferManager();
            _joinRehashSequence = new ParallelQeueHelper(hashQueueCount);
            _resultSendSequence = new TaskSequenceHelper();
            _multiNodeProcessingManager = new MultiNodeProcessingManager();
            _joinCompleteProcessingManager = new MultiNodeProcessingManager();
            _receivedRelations = new List<Guid>();

            Init();

            _logger.Trace($"Инициализирован {nameof(HashJoinManager)}");
        }

        private void Init()
        {
            _client.SubscribeToPacket<RelationPreparePacket>(RelationPreparePacketReceivedHandler);
            _client.SubscribeToPacket<RelationDataPacket>(RelationDataPacketReceivedHandler);
            _client.SubscribeToPacket<QueryPacket>(QueryPacketHandler);
            _client.SubscribeToPacket<CommandPacket>(CommandPacketHandler);
            _client.SubscribeToPacket<DropQueryPacket>(DropQueryPackettHandler);
            _client.SubscribeToPacket<JoinStartPacket>(JoinStartPacketHandler);
            _client.SubscribeToPacket<GetFileRequestPacket>(GetFileRequestPacketHandler);

            _server.SubscribeToPacket<InfoResponcePacket>(InfoResponcePacketHandler);
            _server.SubscribeToPacket<StatusPacket>(StatusPacketHandler);
            _server.SubscribeToPacket<JoinCompletePacket>(JoinCompletePacketHandler);
            _server.SubscribeToPacket<SelectResult>(SelectResultHandler);
            _server.SubscribeToPacket<RelationPreparedPacket>(RelationPreparedPacketHandler);
            _server.SubscribeToPacket<GetFileResponcePacket>(GetFileResponcePacketHandler);
            _server.ClientDisconnected += ServerOnClientDisconnected;
        }

        private List<byte[]> HashData(Relation relation, byte[] data, int queueNumber)
        {
            _logger.Trace($"Хеширование данных для {relation.RelationId}");
            var hashTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.HashData, Guid.Empty,
                Guid.Empty,
                relation.RelationId);

            var hashHelper = ServiceLocator.Instance.HashService;

            var keyIndexes = new List<int>();
            var colIndex = 0;
            foreach (var field in relation.Shema.Fields)
            {
                if (relation.Shema.Indexes.Any(
                    index => index.FieldNames.Any(fieldIndex => fieldIndex == field.Name)))
                {
                    keyIndexes.Add(colIndex);
                }
                colIndex++;
            }

            List<byte[]> result;
            var gpuHashHelper = hashHelper as GpuHashHelper;
            if (gpuHashHelper != null)
            {
                result = gpuHashHelper.ProcessData(data, _hashCount * _nodes.Count,
                    keyIndexes.ToArray(), queueNumber);
            }
            else
            {
                result = hashHelper.ProcessData(data, _hashCount * _nodes.Count, keyIndexes.ToArray());
            }

            hashTime.Stop();

            return result;
        }
        
        private void HashAndSend(Relation relation, byte[] data, int orderNumber, int queryNumber, bool isLast, int queueNumber)
        {
            var hashedData = HashData(relation, data, queueNumber);

            SendHashedData(relation, orderNumber, queryNumber, isLast, hashedData);
        }

        private void SendHashedData(Relation relation, int orderNumber, int queryNumber, bool isLast, List<byte[]> hashedData)
        {
            for (var i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                node.SetRelationStatus(relation.RelationId,
                    isLast ? RelationStatus.DataTransfered : RelationStatus.DataTransfering);
                var hashNumber = 0;
                for (var j = i * _hashCount; j < (i + 1) * _hashCount; j++)
                {
                    _server.SendAsyncQueue(new HashedRelationDataPacket()
                    {
                        RelationId = relation.RelationId,
                        QueryId = relation.QueryId,
                        SubQueryId = Guid.Empty,
                        Data = hashedData[j],
                        IsLast = isLast && hashNumber == _hashCount - 1,
                        OrderNumber = orderNumber * _hashCount + hashNumber,
                        QueryNumber = queryNumber,
                        HashNumber = hashNumber++,
                        Id = new Identify {ClientId = node.Id}
                    });
                }
            }
        }

        private void SendBufferToJoin(Guid relationId)
        {
            _logger.Trace($"Отправка буфера для {relationId} в join");
            var relation = _relationManager.GetRelation(relationId);
            var data = _bufferManager.GetQueryBuffer(relationId);
            foreach (var buf in data)
            {
                SendHashedData(relation, buf.OrderNumber, -1, buf.IsLast, buf.Data);
            }
            _bufferManager.RemoveData(relationId);
            lock (_receivedRelationsSync)
            {
                if (_receivedRelations.Contains(relationId)) _receivedRelations.Remove(relationId);
            }
            _multiNodeProcessingManager.Remove(relationId);
        }

        private void StartRelationTask(Guid relationId)
        {
            lock (_tasksInProgressSync)
            {
                if (_tasksInProgress.ContainsKey(relationId)) _tasksInProgress[relationId]++;
                else
                {
                    _tasksInProgress.Add(relationId, 1);
                }
            }
        }

        private void EndRelationTask(Guid relationId)
        {
            lock (_tasksInProgressSync)
            {
                if (_tasksInProgress.ContainsKey(relationId))
                {
                    _tasksInProgress[relationId]--;
                    if (_tasksInProgress[relationId] == 0) _tasksInProgress.Remove(relationId);
                }
            }
        }
        private void WaitRelationTask(Guid relationId)
        {
            var wait = true;
            do
            {
                lock (_tasksInProgressSync)
                {
                    if (!_tasksInProgress.ContainsKey(relationId)) wait = false;
                }
                if (wait) Thread.Sleep(100);
            } while (wait);
        }

        #region Client

        private void RelationPreparePacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as RelationPreparePacket;
            if (packet == null) return;

            var relation = new Relation
            {
                RelationId = packet.RelationId,
                RelationOriginalName = packet.RelationName,
                Shema = packet.RelationShema.ToRelationSchema(),
                QueryId = packet.QueryId,
                IsEmpty = packet.IsEmptyRelation
            };

            _logger.Trace($"Получен пакет для подготовки отношения {relation.RelationName}");

            _relationManager.AddRelation(relation);

            foreach (var node in _nodes)
            {
                node.AddRelation(relation.RelationId);
                _server.SendAsyncQueue(new RelationPreparePacket()
                {
                    RelationId = packet.RelationId,
                    QueryId = packet.QueryId,
                    SubQueryId = packet.SubQueryId,
                    QueryNumber = packet.QueryNumber,
                    RelationName = packet.RelationName,
                    RelationShema = packet.RelationShema,
                    IsEmptyRelation = packet.IsEmptyRelation,
                    Id = new Identify { ClientId = node.Id }
                });
            }
        }

        private void RelationDataPacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as RelationDataPacket;
            if (packet == null) return;

            var relation = _relationManager.GetRelation(packet.RelationId);
            if (relation == null) return;

            _logger.Trace($"Получены данные для отношения {relation.RelationName}");

            _joinRehashSequence.AddToQueue(new QueueTask((i, obj) => {
                    var tup = (Tuple<Relation, byte[], int, int, bool>)obj;
                    HashAndSend(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item5, i);
                },
                new Tuple<Relation, byte[], int, int, bool>(relation, packet.Data, packet.OrderNumber,
                    packet.QueryNumber,
                    packet.IsLast)));
        }

        private void QueryPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as QueryPacket;
            if (packet == null) return;

            _logger.Trace($"Запрос результата для отношения {packet.RelationId}");

            _resultSendSequence.AddTask(new Task(obj =>
            {
                var pack = (QueryPacket) obj;

                //ожидание готовности буфера
                while (!_multiNodeProcessingManager.Check(pack.RelationId))
                {
                    Thread.Sleep(100);
                }
                _multiNodeProcessingManager.Remove(pack.RelationId);
                SendResultData(pack);
            }, packet));
        }

        private void SendResultData(QueryPacket packet)
        {
            var sended = true;
            do
            {
                if (_bufferManager.CheckReady(packet.RelationId))
                {
                    _logger.Trace($"Отправка результата для отношения {packet.RelationId}");
                    SendResult(packet);
                }
                else
                {
                    lock (_receivedRelationsSync)
                    {
                        if (_receivedRelations.Contains(packet.RelationId))
                        {
                            _logger.Error($"Отношение {packet.RelationId} для отправки результата не готово");
                            sended = false;
                        }
                    }
                }

                if (!sended) Thread.Sleep(100);

            } while (!sended);
        }

        private void SendResult(QueryPacket packet)
        {
            var data = _bufferManager.GetQueryBuffer(packet.RelationId);
            for (var index = 0; index < data.Count; index++)
            {
                var buffer = data[index];
                for (var i = 0; i < buffer.Data.Count; i++)
                {
                    _client.SendAsyncQueue(new SelectResult()
                    {
                        SubQueryId = packet.SubQueryId,
                        RelationId = packet.RelationId,
                        QueryId = packet.QueryId,
                        IsLast = buffer.IsLast && i == buffer.Data.Count - 1,
                        OrderNumber = index * _hashCount * _nodes.Count + i,
                        QueryNumber = packet.QueryNumber,
                        Result = buffer.Data[i]
                    });
                }
            }
            _bufferManager.RemoveData(packet.RelationId);
            _relationManager.RemoveRelation(packet.RelationId);
        }

        private void JoinStartPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as JoinStartPacket;
            if (packet == null) return;

            Task.Factory.StartNew(obj =>
            {
                var joinStartPacket = (JoinStartPacket) obj;
                var leftRelation = _relationManager.GetRelation(joinStartPacket.RelationLeft);
                var rightRelation = _relationManager.GetRelation(joinStartPacket.RelationRight);
                _relationManager.AddRelation(new Relation()
                {
                    RelationId = joinStartPacket.RelationId,
                    Shema = joinStartPacket.ResultSchema.ToRelationSchema(),
                    QueryId = joinStartPacket.QueryId,
                });

                _logger.Trace(
                    $"Запуск join для {leftRelation.RelationId} и {rightRelation.RelationId} в {joinStartPacket.RelationId}");

                SendJoinData(leftRelation, rightRelation);

                foreach (var node in _nodes)
                {
                    _multiNodeProcessingManager.AddTask(node.Id, joinStartPacket.RelationId);
                    _joinCompleteProcessingManager.AddTask(node.Id, joinStartPacket.RelationId);
                    node.SetRelationStatus(leftRelation.RelationId, RelationStatus.Join);
                    node.SetRelationStatus(rightRelation.RelationId, RelationStatus.Join);
                    _server.SendAsyncQueue(new JoinStartPacket()
                    {
                        RelationId = joinStartPacket.RelationId,
                        QueryId = joinStartPacket.QueryId,
                        SubQueryId = joinStartPacket.SubQueryId,
                        QueryNumber = joinStartPacket.QueryNumber,
                        Query = joinStartPacket.Query,
                        RelationLeft = joinStartPacket.RelationLeft,
                        RelationRight = joinStartPacket.RelationRight,
                        ResultSchema = joinStartPacket.ResultSchema,
                        Id = new Identify {ClientId = node.Id}
                    });
                }
            }, packet);
        }

        private void SendJoinData(Relation leftRelation, Relation rightRelation)
        {
            var sendedLeft = true;
            var sendedRight = true;
            do
            {
                if (_bufferManager.CheckReady(leftRelation.RelationId))
                {
                    SendBufferToJoin(leftRelation.RelationId);
                }
                else
                {
                    lock (_receivedRelationsSync)
                    {
                        if (_receivedRelations.Contains(leftRelation.RelationId))
                        {
                            _logger.Error($"Отношение {leftRelation.RelationId} для join не готово");
                            sendedLeft = false;
                        }
                    }
                }

                if (_bufferManager.CheckReady(rightRelation.RelationId))
                {
                    SendBufferToJoin(rightRelation.RelationId);
                }
                else
                {
                    lock (_receivedRelationsSync)
                    {
                        if (_receivedRelations.Contains(rightRelation.RelationId))
                        {
                            _logger.Error($"Отношение {rightRelation.RelationId} для join не готово");
                            sendedRight = false;
                        }
                    }
                }

                if (!sendedLeft || !sendedRight) Thread.Sleep(100);

            } while (!sendedLeft || !sendedRight);
        }

        private void CommandPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as CommandPacket;
            if (packet == null) return;

            _logger.Trace($"Получена команда {(Command)packet.Command}");

            foreach (var node in _nodes)
            {
                _server.SendAsyncQueue(new CommandPacket()
                {
                    Command = packet.Command,
                    Id = new Identify { ClientId = node.Id }
                });
            }
        }

        private void DropQueryPackettHandler(PacketBase packetBase)
        {
            var packet = packetBase as DropQueryPacket;
            if (packet == null) return;

            _logger.Warning($"Удаление отношения {packet.RelationId}");

            _relationManager.RemoveRelation(packet.RelationId);
            _bufferManager.RemoveData(packet.RelationId);

            foreach (var node in _nodes)
            {
                _server.SendAsyncQueue(new DropQueryPacket()
                {
                    RelationId = packet.RelationId,
                    QueryId = packet.QueryId,
                    SubQueryId = packet.SubQueryId,
                    QueryNumber = packet.QueryNumber,
                    Id = new Identify { ClientId = node.Id }
                });
            }
        }

        private void GetFileRequestPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as GetFileRequestPacket;
            if (packet == null) return;

            _logger.Warning($"Запрос файлов {packet.FileName}");
            
            foreach (var node in _nodes)
            {
                _server.SendAsyncQueue(new GetFileRequestPacket()
                {
                    FileName = packet.FileName,
                    Id = new Identify { ClientId = node.Id }
                });
            }
        }
        
        #endregion
        
        #region Server
        
        private void RelationPreparedPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as RelationPreparedPacket;
            if (packet == null) return;

            var node = _nodes.FirstOrDefault(n => n.Id == packet.Id.ClientId);
            if (node == null) return;
            
            _logger.Trace($"Отношение {packet.RelationId} подготовлено в {node.Id}");

            node.SetRelationStatus(packet.RelationId, RelationStatus.Prepared);

            if (_nodes.All(n => n.Status[packet.RelationId] == RelationStatus.Prepared))
            {
                _client.Send(new RelationPreparedPacket()
                {
                    RelationId = packet.RelationId,
                    QueryId = packet.QueryId,
                    SubQueryId = packet.SubQueryId,
                    QueryNumber = packet.QueryNumber
                });
            }
        }

        private void JoinCompletePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as JoinCompletePacket;
            if (packet == null) return;

            var node = _nodes.FirstOrDefault(n => n.Id == packet.Id.ClientId);
            if (node == null) return;

            _logger.Trace($"Join {packet.NewRelationId} завершен в {node.Id}");

            node.SetRelationStatus(packet.LeftRelationId, RelationStatus.JoinComplete);
            node.SetRelationStatus(packet.RightRelationId, RelationStatus.JoinComplete);

            _joinCompleteProcessingManager.SetComplete(node.Id, packet.NewRelationId);

            if (!_multiNodeProcessingManager.IsComplete(node.Id, packet.NewRelationId))
            {
                _multiNodeProcessingManager.SetComplete(node.Id, packet.NewRelationId);
                MarkLastPacket(node, packet.NewRelationId);
            }

            if (!_joinCompleteProcessingManager.Check(packet.NewRelationId)) return;

            //ожидание завершения операций
            WaitRelationTask(packet.NewRelationId);

            lock (_receivedRelationsSync)
            {
                _receivedRelations.Add(packet.NewRelationId);
            }

            _client.Send(new JoinCompletePacket()
            {
                RelationId = packet.RelationId,
                QueryId = packet.QueryId,
                SubQueryId = packet.SubQueryId,
                QueryNumber = packet.QueryNumber,
                LeftRelationId = packet.LeftRelationId,
                RightRelationId = packet.RightRelationId,
                NewRelationId = packet.NewRelationId
            });

            foreach (var joinNode in _nodes)
            {
                joinNode.RemoveRelationStatus(packet.NewRelationId);
            }

            _relationManager.RemoveRelation(packet.LeftRelationId);
            _relationManager.RemoveRelation(packet.RightRelationId);
            _joinCompleteProcessingManager.Remove(packet.NewRelationId);
        }

        private void InfoResponcePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as InfoResponcePacket;

            if (packet?.NodeType != NodeType.Join) return;

            var node = new JoinNode(packet.Id.ClientId, packet.IsHardNode, packet.MinRamAvaible) { NodeType = packet.NodeType };
            _nodes.Add(node);
        }

        private void StatusPacketHandler(PacketBase packetBase)
        {
            //ignored
        }

        private void SelectResultHandler(PacketBase packetBase)
        {
            var packet = packetBase as SelectResult;
            if (packet == null) return;

            var node = _nodes.FirstOrDefault(n => n.Id == packet.Id.ClientId);
            if (node == null) return;

            _logger.Trace($"Получен результат join {packet.RelationId} из {node.Id}");

            StartRelationTask(packet.RelationId);
            _joinRehashSequence.AddToQueue(new QueueTask((i, obj) =>
            {
                var tup = (Tuple<SelectResult, JoinNode>) obj;
                HashAndStoreData(tup.Item1, tup.Item2, i);
                EndRelationTask(tup.Item1.RelationId);
            }, new Tuple<SelectResult, JoinNode>(packet, node)));
        }
        
        private void HashAndStoreData(SelectResult packet, JoinNode node, int queueNumber)
        {
            var relationId = packet.RelationId;
            var hashedData = HashData(_relationManager.GetRelation(relationId), packet.Result, queueNumber);

            _logger.Trace($"Отхеширован результат join {packet.RelationId} из {node.Id}");
            _bufferManager.AddData(new HashedQueryBuffer
            {
                QueryId = relationId,
                IsLast = false,
                Data = hashedData,
                OrderNumber = packet.OrderNumber * _nodes.Count +
                              _nodes.IndexOf(node) % _nodes.Count,
            });

            if (packet.IsLast)
                _multiNodeProcessingManager.SetComplete(packet.Id.ClientId, relationId);

            MarkLastPacket(node, relationId);
        }

        private void MarkLastPacket(JoinNode node, Guid relationId)
        {
            if (_multiNodeProcessingManager.Check(relationId))
            {
                _logger.Trace($"Получен последний пакет для join {relationId} из {node.Id}");
                _bufferManager.MarkLastPacket(relationId);
            }
        }

        private void GetFileResponcePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as GetFileResponcePacket;
            if (packet == null) return;

            var node = _nodes.FirstOrDefault(n => n.Id == packet.Id.ClientId);
            if (node == null) return;

            _client.SendAsyncQueue(new GetFileResponcePacket()
            {
                FileName = $"{node.Id:N}_{packet.FileName}",
                Data = packet.Data,
            });
        }
        
        #endregion
        
        #region EventHandlers
        
        private void ServerOnClientDisconnected(object sender, DisconnectEventArg disconnectEventArg)
        {
            var node = _nodes.FirstOrDefault(n => n.Id == disconnectEventArg.ClientId);
            if (node != null) _nodes.Remove(node);
        }

        #endregion

    }
}
