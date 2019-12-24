#region Copyright
/*
 * Copyright 2018 Roman Klassen
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
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data.EventArgs.Base;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Services.Interfaces;
using ClusterixN.QueryProcessing.Services.Processors;

namespace ClusterixN.QueryProcessing.Services
{
    internal class SelectAndSendService : SelectService
    {
        private readonly Action<SelectAndSendPacket> _processSelectAction;
        private readonly Dictionary<Guid, SelectAndSendPacket> _recievedTasks;
        private readonly TaskSequenceHelper _taskSequenceHelper;
        private readonly object _syncObject = new object();
        private readonly List<string> _connectionStrings;
        private readonly object _syncOnSelectBlockReadedObject = new object();
        private readonly Dictionary<Guid,int> _parallelSelectPacketNumber;
        private readonly Dictionary<Guid, int> _parallelSelectCount;
        private TaskSequenceHelper _taskSequenceHelperForPackets;
        private TaskSequenceHelper _selectResultSequenceHelper;

        public SelectAndSendService(ICommunicator client, IRelationService relationService, ICommandService commandService,
            QueryProcessConfig dbConfig) : base(client, relationService, commandService,  dbConfig)
        {
            _connectionStrings = new List<string>();
            _parallelSelectPacketNumber = new Dictionary<Guid, int>();
            _parallelSelectCount = new Dictionary<Guid, int>();
            _taskSequenceHelper = new TaskSequenceHelper();
            _taskSequenceHelperForPackets = new TaskSequenceHelper();
            _selectResultSequenceHelper = new TaskSequenceHelper();
            _recievedTasks = new Dictionary<Guid, SelectAndSendPacket>();
            commandService.Subscribe(this);
            RelationService = relationService;
            Client.SubscribeToPacket<SelectAndSendPacket>(SelectAndSendPacketReceived);
            
            if (relationService is HashRelationService)
            {
                _processSelectAction = HashSelectProcess;
            }
            else
            {
                _processSelectAction = SelectProcess;
            }

            var multiDb = ServiceLocator.Instance.ConfigurationService.GetAppSetting("MultiDB");

            if (!string.IsNullOrWhiteSpace(multiDb) && multiDb == "1")
            {
                _connectionStrings = ServiceLocator.Instance.ConfigurationService
                    .GetConnetctionStrings(s => s.Contains("MultiDB")).ToList();
                _processSelectAction = SelectProcessParallel;
            }
        }

        private void SelectAndSendPacketReceived(PacketBase packetBase)
        {
            var packet = packetBase as SelectAndSendPacket;
            if (packet != null)
            {
                lock (_syncObject)
                {
                    _recievedTasks.Add(packet.RelationId, packet);
                }
                _taskSequenceHelperForPackets.AddTask(new Task(obj =>
                {
                    var pkt = (SelectAndSendPacket)obj;
                    _processSelectAction.Invoke(pkt);
                }, packet));
            }
        }

        private void HashSelectProcess(SelectAndSendPacket packet)
        {
            using (var qh = new HashSelectQueryProcessor(Config, packet.QueryId, RelationService))
            {
                PauseSelect += qh.Pause;
                StopQuery += qh.StopQuery;
                qh.OnBlockReaded += OnSelectBlockReaded;
                qh.Pause(IsPaused);
                qh.StartParallelQueryProcess(packet);
                PauseSelect -= qh.Pause;
                StopQuery -= qh.StopQuery;
            }
        }

        private void SelectProcess(SelectAndSendPacket packet)
        {
            Select(Config, packet);
        }

        private void SelectProcessParallel(SelectAndSendPacket packet)
        {
            var tasks = new List<Task>();
            foreach (var connectionString in _connectionStrings)
            {
                var task = SelectProcessParallelTask(new QueryProcessConfig()
                {
                    BlockLength = Config.BlockLength,
                    ConnectionString = connectionString,
                    DataDir = Config.DataDir,
                    SyncQueryDrop = Config.SyncQueryDrop
                }, packet);

                tasks.Add(task);
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void Select(QueryProcessConfig config, SelectAndSendPacket packet)
        {
            using (var qh = new SelectQueryProcessor(config, packet.QueryId, RelationService))
            {
                PauseSelect += qh.Pause;
                StopQuery += qh.StopQuery;
                qh.OnBlockReaded += OnSelectBlockReaded;
                qh.Pause(IsPaused);
                qh.StartQueryProcess(packet);
                PauseSelect -= qh.Pause;
                StopQuery -= qh.StopQuery;
            }
        }

        private Task SelectProcessParallelTask(QueryProcessConfig config, SelectAndSendPacket packet)
        {
            return new Task(obj =>
            {
                lock (_syncOnSelectBlockReadedObject)
                {
                    if (_parallelSelectCount.ContainsKey(packet.RelationId))
                    {
                        _parallelSelectCount[packet.RelationId]++;
                    }
                    else
                    {
                        _parallelSelectCount.Add(packet.RelationId, 1);
                    }
                }

                var tup = (Tuple<QueryProcessConfig, SelectAndSendPacket>) obj;
                Select(tup.Item1, tup.Item2);
            }, new Tuple<QueryProcessConfig, SelectAndSendPacket>(config, packet));
        }

        private void OnSelectBlockReaded(object sender, SimpleEventArgs<SelectResult> simpleEventArgs)
        {
            lock (_syncOnSelectBlockReadedObject)
            {
                var selectResult = simpleEventArgs.Value;
                _selectResultSequenceHelper.AddTask(new Task(obj =>
                {
                    var result = (SelectResult) obj;
                    ProcessSelectedBlock(selectResult);
                }, selectResult));
            }
        }

        private void ProcessSelectedBlock(SelectResult selectResult)
        {
            if (_parallelSelectCount.ContainsKey(selectResult.RelationId))
            {
                if (_parallelSelectCount[selectResult.RelationId] > 0 &&
                    selectResult.IsLast) _parallelSelectCount[selectResult.RelationId]--;

                selectResult.IsLast &= _parallelSelectCount[selectResult.RelationId] == 0;
                selectResult.OrderNumber = GetNextPacketNumber(selectResult);
            }

            SelectAndSendPacket taskPacket;
            lock (_syncObject)
            {
                taskPacket = _recievedTasks[selectResult.RelationId];
            }

            var hashCount = taskPacket.HashCount.Sum();
            var relation = new Relation()
            {
                RelationId = selectResult.SubQueryId,
                QueryId = selectResult.QueryId,
                Shema = taskPacket.RelationShema.ToRelationSchema()
            };

            if (hashCount == 1)
            {
                _taskSequenceHelper.AddTask(new Task(obj =>
                {
                    var tup = (Tuple<SelectAndSendPacket, SelectResult>) obj;
                    SendData(tup.Item1, tup.Item2);
                }, new Tuple<SelectAndSendPacket, SelectResult>(taskPacket, selectResult)));
            }
            else
            {
                HashHelper.Instance.HashDataAsync(relation, selectResult.Result, hashCount, SendHashedDataAsync,
                    new Tuple<SelectAndSendPacket, SelectResult>(taskPacket, selectResult));
            }
        }

        private int GetNextPacketNumber(SelectResult selectResult)
        {
            if (_parallelSelectPacketNumber.ContainsKey(selectResult.RelationId))
            {
                if (selectResult.IsLast)
                {
                    var number = _parallelSelectPacketNumber[selectResult.RelationId] + 1;
                    _parallelSelectPacketNumber.Remove(selectResult.RelationId);
                    return number;
                }
                else
                {
                    return ++_parallelSelectPacketNumber[selectResult.RelationId];
                }
            }
            else
            {
                _parallelSelectPacketNumber.Add(selectResult.RelationId, 0);
                return 0;
            }
        }

        private void SendData(SelectAndSendPacket taskPacket, SelectResult selectResult)
        {
            for (var i = 0; i < taskPacket.JoinCount; i++)
            {
                Client.Send(IPEndPointParser.Parse(taskPacket.JoinAddresses[i]),
                    new RelationDataPacket()
                    {
                        RelationId = taskPacket.RelationId,
                        QueryId = taskPacket.QueryId,
                        SubQueryId = taskPacket.SubQueryId,
                        QueryNumber = taskPacket.QueryNumber,
                        Data = selectResult.Result,
                        OrderNumber = selectResult.OrderNumber,
                        IsLast = selectResult.IsLast,
                        SourceNodeCount = taskPacket.IoCount
                    });
            }
            DataSendedHandler(selectResult, taskPacket);
        }

        private void SendHashedDataAsync(List<byte[]> hashedData, object obj)
        {
            _taskSequenceHelper.AddTask(new Task(o =>
            {
                var tup = (Tuple<List<byte[]>, object>)o;
                SendHashedData(tup.Item1, tup.Item2);
            }, new Tuple<List<byte[]>, object>(hashedData, obj)));
        }

        private void SendHashedData(List<byte[]> hashedData, object obj)
        {
            var tup = (Tuple<SelectAndSendPacket, SelectResult>) obj;
            var taskPacket = tup.Item1;
            var selectResult = tup.Item2;
            var coreIndex = 0;
            //var startJoinIndex = taskPacket.IoNumber % taskPacket.IoCount;

            for (var i = 0; i < taskPacket.JoinCount; i++)
            {
                for (var j = 0; j < taskPacket.HashCount[i]; j++)
                {
                    Client.Send(IPEndPointParser.Parse(taskPacket.JoinAddresses[i]),
                        new HashedRelationDataPacket()
                        {
                            RelationId = taskPacket.RelationId,
                            QueryId = taskPacket.QueryId,
                            SubQueryId = taskPacket.SubQueryId,
                            QueryNumber = taskPacket.QueryNumber,
                            Data = hashedData[coreIndex++],
                            HashNumber = j,
                            OrderNumber = selectResult.OrderNumber * taskPacket.HashCount[i] + j,
                            IsLast = selectResult.IsLast && taskPacket.HashCount[i] == j + 1,
                            SourceNodeCount = taskPacket.IoCount,
                        });
                }
            }

            DataSendedHandler(selectResult, taskPacket);
        }

        private void DataSendedHandler(SelectResult selectResult, SelectAndSendPacket taskPacket)
        {
            if (selectResult.IsLast)
            {
                Client.Send(new QueryStatusPacket()
                {
                    RelationId = taskPacket.RelationId,
                    QueryId = taskPacket.QueryId,
                    SubQueryId = taskPacket.SubQueryId,
                    QueryNumber = taskPacket.QueryNumber,
                    Status = QueryStatus.SelectProcessed
                });

                lock (_syncObject)
                {
                    _recievedTasks.Remove(taskPacket.RelationId);
                }
            }
            else
            {
                Client.Send(new QueryStatusPacket()
                {
                    RelationId = taskPacket.RelationId,
                    QueryId = taskPacket.QueryId,
                    SubQueryId = taskPacket.SubQueryId,
                    QueryNumber = taskPacket.QueryNumber,
                    Status = QueryStatus.SelectProcessing
                });
            }

            GC.Collect();
        }
    }
}