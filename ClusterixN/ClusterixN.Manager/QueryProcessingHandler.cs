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
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.EventArgs;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Common.Utils.PerformanceCounters;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Manager.Data.Config;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline;
using ClusterixN.Manager.ProcessingPipeline.Handlers;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.Network.Packets.Data;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager
{
    internal sealed class QueryProcessingHandler : IDisposable
    {
        private readonly NodesManager _nodesManager;
        private readonly QueryBufferManager _queryBufferManager;
        private IQueryManager _queryManager;
        private readonly Thread _queryProcessingThread;
        private readonly CancellationTokenSource _queryProcessingThreadCancellationTokenSource;
        private readonly IQuerySourceManager _querySourceManager;
        private readonly TaskSequenceHelper _taskSequenceHelper;
        private readonly ILogger _logger;
        private int _minRamAvaible;
        private TimeLogHelper _executionStopwatch;
        private QueryStreamGenerator _queryStreamGenerator;
        private Dictionary<Guid, Command> _lastCommands;
        private readonly PipelineManagment _pipeline;
        private readonly QueueManager _queueManager;
        private readonly ICommunicator _server;
        private JoinDataStoreMode _joinDataStoreMode;

        public QueryProcessingHandler(Server server, string dataDir)
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _pipeline = new PipelineManagment();
            _querySourceManager = ServiceLocator.Instance.QuerySourceManager;
            _nodesManager = new NodesManager();
            _taskSequenceHelper = new TaskSequenceHelper();
            _queryBufferManager = new QueryBufferManager(GetDataDirectoryPath(dataDir));
            _queueManager = new QueueManager();
            _server = server;
            _server.SubscribeToPacket<InfoResponcePacket>(InfoResponcePacketRecieved);
            _server.SubscribeToPacket<StatusPacket>(StatusResponcePacketHandler);
            _server.SubscribeToPacket<StatusPacket>(StatusResponcePacketHandler);
            _server.SubscribeToPacket<GetFileResponcePacket>(GetFileResponcePacketHandler);
            server.ClientDisconnected += ClientDisconnectedEventHandler;

            _pipeline.RegisterObject(server);

            Init();

            _queryProcessingThreadCancellationTokenSource = new CancellationTokenSource();
            _queryProcessingThread = new Thread(QueryProcessingLoop);
            _queryProcessingThread.Start(_queryProcessingThreadCancellationTokenSource.Token);
            _logger.Trace("Инициализирован обработчик запросов");
        }

		private void Init()
		{

		    if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkMode"), "Decentralized", StringComparison.OrdinalIgnoreCase) == 0)
		    {
		        InitSequentialParallelPipeline();
            }
		    else if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkMode"), "DecentralizedSelectJoin", StringComparison.OrdinalIgnoreCase) == 0)
		    {
		        InitSequentialPipeline();
		    }
            else
            {
                InitSequentialTransferPipeline();
		    }

            _minRamAvaible = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("MinRamAvaible"));
		    _lastCommands = new Dictionary<Guid, Command>();
		    PerformanceMonitor.Instance.NewValueAvaible += OnPerformanceMonitorNewValue;

		    _queryManager.SendResult += QueryManagerOnSendResultAsync;
        }

        private void InitPipeline()
        {
            var conf = ConfigurationManager.GetSection("ClusterixN.Pipeline") as NameValueCollection;
            if (conf == null) return;

            foreach (string className in conf)
            {
                var type = Type.GetType(className);
                if (type != null)
                {
                    _pipeline.RegisterPipelineNode(type);
                }
            }
        }

        private void InitSequentialParallelPipeline()
        {
            QueryBuilder.DefaultJoinRelationStatus = QueryRelationStatus.Wait;
            QueryBuilder.DefaultRelationStatus = QueryRelationStatus.Wait;

            _queryManager = new ParallelTranfserQueryManager();
            _pipeline.RegisterObject(_queryManager);
            _pipeline.RegisterObject(_nodesManager);
            _pipeline.RegisterObject(_queryBufferManager);
            _pipeline.RegisterObject(_queueManager);
            _pipeline.RegisterType<PauseLogManager>();

            _pipeline.RegisterPipelineNode<AddQuery>();
            _pipeline.RegisterPipelineNode<CleanUp>();

            _pipeline.RegisterPipelineNode<SelectAndSendToJoin>();
            _pipeline.RegisterPipelineNode<JoinMultiNodePrepare>();
            _pipeline.RegisterPipelineNode<JoinStartSequencedMultiNode>();
            _pipeline.RegisterPipelineNode<SortPrepareMultiNode>();
            _pipeline.RegisterPipelineNode<SortStart>();
            _pipeline.RegisterPipelineNode<SortTransferResult>();

            _pipeline.RegisterPipelineNode<TimeoutProcessing>();
        }

        private void InitSequentialPipeline()
        {
            QueryBuilder.DefaultJoinRelationStatus = QueryRelationStatus.Wait;
            QueryBuilder.DefaultRelationStatus = QueryRelationStatus.Wait;

            _queryManager = new ParallelTranfserQueryManager();
            _pipeline.RegisterObject(_queryManager);
            _pipeline.RegisterObject(_nodesManager);
            _pipeline.RegisterObject(_queryBufferManager);
            _pipeline.RegisterObject(_queueManager);
            _pipeline.RegisterType<PauseLogManager>();

            _pipeline.RegisterPipelineNode<AddQuery>();
            _pipeline.RegisterPipelineNode<CleanUp>();

            _pipeline.RegisterPipelineNode<SelectAndSendToJoinAndWait>();
            _pipeline.RegisterPipelineNode<JoinMultiNodePrepare>();
            _pipeline.RegisterPipelineNode<JoinStartSequencedMultiNodeWait>();
            _pipeline.RegisterPipelineNode<SortPrepareMultiNode>();
            _pipeline.RegisterPipelineNode<SortStart>();
            _pipeline.RegisterPipelineNode<SortTransferResult>();

            _pipeline.RegisterPipelineNode<TimeoutProcessing>();
        }

        private void InitSequentialTransferPipeline()
        {
            QueryBuilder.DefaultJoinRelationStatus = QueryRelationStatus.WaitAnotherRelation;
            QueryBuilder.DefaultRelationStatus = QueryRelationStatus.Wait;

            var joinMode = JoinMode.Sequenced;
            _joinDataStoreMode = JoinDataStoreMode.Memory;
            JoinDataTransferMode joinDataTransferMode;
            var selectMode = SelectMode.CoreForRelation;

            if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("RoutingMode"), "Packets", StringComparison.OrdinalIgnoreCase) == 0)
            {
                joinDataTransferMode = JoinDataTransferMode.Packets;
                _queryManager = new QueryManager();
            }
            else if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("RoutingMode"), "Relations", StringComparison.OrdinalIgnoreCase) == 0)
            {
                joinDataTransferMode = JoinDataTransferMode.Relations;
                _queryManager = new SequenceQueryManager();
            }
            else
            {
                joinDataTransferMode = JoinDataTransferMode.AllRelations;
                _queryManager = new SequenceQueryManager();
            }

            if (ServiceLocator.Instance.ConfigurationService.GetAppSetting("UseIntegratedJoin") == "1")
            {
                joinMode = JoinMode.Integrated;
            }

            if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("SelectMode"), "AllNodesForOneQuery", StringComparison.OrdinalIgnoreCase) == 0)
            {
                selectMode = SelectMode.AllNodesForOneQuery;
            }

            if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("JoinDataStoreMode"), "Disc", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _joinDataStoreMode = JoinDataStoreMode.Disc;
            }

            _pipeline.RegisterObject(_queryManager);
            _pipeline.RegisterObject(_nodesManager);
            _pipeline.RegisterObject(_queryBufferManager);
            _pipeline.RegisterObject(_queueManager);
            _pipeline.RegisterType<PauseLogManager>();

            _pipeline.RegisterPipelineNode<AddQuery>();
            _pipeline.RegisterPipelineNode<CleanUp>();
            switch (selectMode)
            {
                case SelectMode.CoreForRelation:
                    _pipeline.RegisterPipelineNode<Select>();
                    break;
                case SelectMode.AllNodesForOneQuery:
                    _pipeline.RegisterPipelineNode<SelectOneByOne>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _pipeline.RegisterPipelineNode<JoinPrepare>();
            switch (joinDataTransferMode)
            {
                case JoinDataTransferMode.Packets:
                    _pipeline.RegisterPipelineNode<JoinTransferDataByPackets>();
                    break;
                case JoinDataTransferMode.Relations:
                    _pipeline.RegisterPipelineNode<JoinTransferDataByRelations>();
                    break;
                case JoinDataTransferMode.AllRelations:
                    _pipeline.RegisterPipelineNode<JoinTransferDataByAllRelations>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (joinMode)
            {
                case JoinMode.Sequenced:
                    _pipeline.RegisterPipelineNode<JoinStartSequenced>();
                    break;
                case JoinMode.Integrated:
                    _pipeline.RegisterPipelineNode<JoinStartIntegrated>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _pipeline.RegisterPipelineNode<JoinTransferResult>();
            _pipeline.RegisterPipelineNode<SortPrepare>();
            _pipeline.RegisterPipelineNode<SortTransferData>();
            _pipeline.RegisterPipelineNode<SortStart>();
            _pipeline.RegisterPipelineNode<SortTransferResult>();
            _pipeline.RegisterPipelineNode<TimeoutProcessing>();
        }
        
        public void Dispose()
        {
            _queryProcessingThreadCancellationTokenSource.Cancel();
            if (!_queryProcessingThread.Join(10000))
            {
                _queryProcessingThread.Abort();
            }
            _queryProcessingThreadCancellationTokenSource.Dispose();
            _taskSequenceHelper.Dispose();
        }

        private string GetDataDirectoryPath(string dataDir)
        {
            var dir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), dataDir));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private void QueryProcessingLoop(object obj)
        {
            if (!(obj is CancellationToken)) return;
#if DEBUG
            var points = 100;
            var index = 0;
            var elapsed = new int[points];
#endif
            var sw = new Stopwatch();

            var tocken = (CancellationToken) obj;
            while (!tocken.IsCancellationRequested)
            {
                sw.Start();

                _pipeline.IteratePipeline();

                while (!_queueManager.IsEmpty)
                {
                    _queueManager.Get().Invoke();
                }

                sw.Stop();
#if DEBUG
                elapsed[index++] = (int)sw.ElapsedMilliseconds;

                if (index == elapsed.Length)
                {
                    index = 0;
                    Debug.WriteLine($"{elapsed.Average():F2} {elapsed.Max()} {elapsed.Min()}");
                }
#endif

                if (10-sw.ElapsedMilliseconds > 0)
                    Thread.Sleep(10 - (int)sw.ElapsedMilliseconds);

                sw.Reset();
            }
        }
        
        public void GetLogDb()
        {
            foreach (var node in _nodesManager.GetNodes())
            {
                GetFileRequest("timeLog.db", node.Id);
                GetFileRequest("performance.db", node.Id);
            }
        }

        public void SendCommand(Command command)
        {
            SendCommand(command, _nodesManager.GetNodes(NodeType.Io));
        }

        public void AddQueryByNumber(int number)
        {
            if (_queryManager.GetQueryCount() == 0) InitBatchProcess();
            _queryManager.AddQuery(_querySourceManager.GetQueryByNumber(number));
        }

        public void StartQueryStream(int count, int queueLength, IQueryNumberGenerator generator)
        {
            if (_queryManager.GetQueryCount() == 0) InitBatchProcess();
            _queryStreamGenerator = new QueryStreamGenerator(count, _querySourceManager, generator);
            AddStreamQuery(queueLength);
        }

        private void InitBatchProcess()
        {
            _executionStopwatch = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.WorkDuration, Guid.Empty,
                Guid.Empty, Guid.Empty);
        }

        private void AddStreamQuery(int count = 1)
        {
            if (_queryStreamGenerator!=null)
            {
                foreach (var query in _queryStreamGenerator.Generate(count))
                {
                    _queryManager.AddQuery(query);
                }
            }
        }

        private void SaveLogFile(NodeType nodeType, Guid id, string file, byte[] data)
        {
            try
            {
                File.WriteAllBytes(
                    $"{_querySourceManager.DirName}{Path.DirectorySeparatorChar}{nodeType}_{id}_{file}",
                    data);
            }
            catch (Exception exception)
            {
                _logger.Error("ошибка сохранения файла", exception);
            }
        }

        private void GetFileRequest(string filename, Guid clientId)
        {
            _server.Send(new GetFileRequestPacket()
            {
                Id = new Identify() {ClientId = clientId},
                FileName = filename
            });
        }

        private void SendCommandEvent(Command command, Guid clientId)
        {
            _server.Send(new CommandPacket()
            {
                Id = new Identify() {ClientId = clientId},
                Command = (int) command
            });
        }

        #region Packet Handlers

        private void InfoResponcePacketRecieved(PacketBase packetBase)
        {
            var packet = packetBase as InfoResponcePacket;
            if (packet != null)
            {
                _nodesManager.AddNode(new Node(packet.Id.ClientId, _joinDataStoreMode == JoinDataStoreMode.Disc, packet.MinRamAvaible)
                {
                    CpuCount = packet.CpuCount,
                    NodeType = packet.NodeType,
                    IsForHardQueries = packet.IsHardNode,
                    ServerPort = packet.ServerPort
                });
            }
        }

        private void StatusResponcePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as StatusPacket;
            if (packet != null)
            {
                _logger.Trace($"Узел: {packet.Id.ClientId} CPU: {packet.CpuUsage} RAM: {packet.AvaibleRam}");

                var node = _nodesManager.GetNode(packet.Id.ClientId);
                if (node == null) return;

                node.RamAvaible = packet.AvaibleRam;
                node.CpuUsage = packet.CpuUsage;
            }
        }

        private void GetFileResponcePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as GetFileResponcePacket;
            if (packet != null)
            {
                var node = _nodesManager.GetNode(packet.Id.ClientId);
                SaveLogFile(node.NodeType, node.Id, packet.FileName, packet.Data);
            }
        }

        #endregion

        #region EventHandlers

        private void ClientDisconnectedEventHandler(object sender, DisconnectEventArg disconnectEventArg)
        {
            _nodesManager.RemoveNode(disconnectEventArg.ClientId);
        }

        private void QueryManagerOnSendResult(Query query)
        {
            AddStreamQuery();
            var result = _queryBufferManager.GetQueryBuffer(query.SortQuery.QueryId);
            var sb = new StringBuilder();
            foreach (var buf in result)
            {
                var str = Encoding.UTF8.GetString(buf.Data);
                if (str.Length == 0)
                {
                    _logger.Error($"Пустой результат для запроса {query.Number} {query.Id}");
                    continue;
                }
                str = str.Replace("|", ";");
                sb.Append(str);
            }
            _querySourceManager.WriteResult(query.SequenceNumber, sb.ToString());
            _queryBufferManager.RemoveData(query.SortQuery.QueryId);

            if (_queryManager.GetQueryCount() == 0)
            {
                _executionStopwatch?.Stop();
                _logger.Info($"Работа завершена. Общее время выполнения: {_executionStopwatch?.Duration/1000 ?? 0} сек");
                GetLogDb();
                var fileName = "timeLog.db";
                var performanceFileName = "performance.db";
                var queryTimesFileName = "times_manager.log";
                var dropQueriesFileName = "dropqueries.log";
                SaveLogFile(NodeType.Mgm,Guid.Empty, fileName, FileHelper.ReadLockedFile(fileName));
                SaveLogFile(NodeType.Mgm, Guid.Empty, performanceFileName, FileHelper.ReadLockedFile(performanceFileName));
                SaveLogFile(NodeType.Mgm, Guid.Empty, queryTimesFileName, FileHelper.ReadLockedFile(queryTimesFileName));
                SaveLogFile(NodeType.Mgm, Guid.Empty, dropQueriesFileName, FileHelper.ReadLockedFile(dropQueriesFileName));
            }
        }

        private void QueryManagerOnSendResultAsync(Query query)
        {
            Task.Factory.StartNew(obj =>
            {
                var queryObj = (Query) obj;
                QueryManagerOnSendResult(queryObj);
            }, query);
        }

        private bool _performanceMonitorNewValueProcessingInProgress;
        private void OnPerformanceMonitorNewValue(object sender, EventArgs e)
        {
            var monitor = sender as PerformanceMonitor;
            if (monitor == null) return;

            if (_performanceMonitorNewValueProcessingInProgress) return;
            _performanceMonitorNewValueProcessingInProgress = true;

            PerformanceLogService.Instance.LogPerformance("CPU", monitor.CpuUsage);
            PerformanceLogService.Instance.LogPerformance("RAM", monitor.RamAvaible);

            PerformanceLogService.Instance.LogPerformance("Net_send", monitor.NetworkSendSpeed);
            PerformanceLogService.Instance.LogPerformance("Net_Receive", monitor.NetworkReceiveSpeed);

            if (monitor.RamAvaible < _minRamAvaible)
            {
                SendCommand(Command.Pause, _nodesManager.GetNodes(NodeType.Io));
                _queryBufferManager.FlushBlockToDisk();
            }
            else if (monitor.RamAvaible > _minRamAvaible)
            {
                SendCommand(Command.Resume, _nodesManager.GetNodes(NodeType.Io));
            }

            _performanceMonitorNewValueProcessingInProgress = false;
        }

        private void SendCommand(Command command, List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (_lastCommands.ContainsKey(node.Id))
                {
                    if (_lastCommands[node.Id] != command)
                    {
                        SendCommandEvent(command, node.Id);
                        _lastCommands[node.Id] = command;
                    }
                }
                else
                {
                    SendCommandEvent(command, node.Id);
                    _lastCommands.Add(node.Id, command);
                }
            }
        }

        #endregion

        #region Events
        

        
        #endregion
    }
}