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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClusterixN.Common;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Network.Converters;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Data.EventArgs;
using ClusterixN.QueryProcessing.Managers;
using ClusterixN.QueryProcessing.Services.Base;
using Relation = ClusterixN.QueryProcessing.Data.Relation;

namespace ClusterixN.QueryProcessing.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class HashRelationService : RelationServiceBase
    {
        private readonly int _hashCount;
        private readonly object _fileSaveSync = new object();
        private readonly bool _mergeBlocksIntoOneFile;
        private readonly Action<RelationDataPacket, Relation> _processPacketAction;
        private readonly Action<HashedRelationDataPacket, Relation> _processHashedPacketAction;
        private readonly LoadStatusManager _fileSaveStatusManager;
        private readonly TaskSequenceHelper _fileSaveSequenceHelper;

        public HashRelationService(ICommunicator client, RelationManager relationManager, QueryProcessConfig dbConfig,
            LoadStatusManager loadStatusManager, TaskSequenceLoadManager taskSequenceLoadManager) : base(client,
            relationManager, dbConfig, loadStatusManager, taskSequenceLoadManager)
        {
            _hashCount = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkCoresCount"));
            _mergeBlocksIntoOneFile = ServiceLocator.Instance.ConfigurationService.GetAppSetting("MergeBlocksIntoOneFile") == "1";
            _processPacketAction = _mergeBlocksIntoOneFile ? (Action<RelationDataPacket, Relation>)ProcessPacketIntoOneFile : ProcessPacket;
            _processHashedPacketAction = _mergeBlocksIntoOneFile ? (Action<HashedRelationDataPacket, Relation>)ProcessHashedPacketIntoOneFile : ProcessHashedPacket;
            _fileSaveStatusManager = new LoadStatusManager();
            _fileSaveStatusManager.LoadCompleteEvent += FileSaveStatusManagerOnLoadCompleteEvent;
            _fileSaveSequenceHelper = new TaskSequenceHelper();
            Client.SubscribeToPacket<RelationPreparePacket>(RelationPreparePacketReceivedHandler);
            Client.SubscribeToPacket<RelationDataPacket>(RelationDataPacketReceivedHandler);
            Client.SubscribeToPacket<HashedRelationDataPacket>(HashedRelationDataPacketHandler);
        }

        private void FileSaveStatusManagerOnLoadCompleteEvent(object sender, LoadCompleteEventArg loadCompleteEventArg)
        {
            var relation = GetRelation(loadCompleteEventArg.RelationId);
            TaskSequenceLoadManager.Add(relation.RelationId, new Task(obj =>
                {
                    var tup = (Tuple<Relation, string, bool, int, int>) obj;
                    LoadData(tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item5);
                },
                new Tuple<Relation, string, bool, int, int>(relation, GetDirectoryName(), true, 0, 1)));
        }

        public int HashCount => _hashCount;

        public bool MergeBlocksIntoOneFile => _mergeBlocksIntoOneFile;

        private string GetHashedRelationName(Relation relation, int hash)
        {
            return $"{relation.RelationName}_{hash}";
        }
        private string GetHashedFileName(Relation relation, int orderNumber, int hash)
        {
            return $"{relation.RelationId}_{orderNumber}_{hash}";
        }

        protected override void LoadComplete(Relation relation, LoadCompleteEventArg loadCompleteEventArg)
        {
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.Indexing, Guid.Empty, Guid.Empty,
                relation.RelationId);
            
            Parallel.For(0, loadCompleteEventArg.HashCount, i =>
            {
                var database =
                    ServiceLocator.Instance.DatabaseService.GetDatabase(Config.ConnectionString, relation.QueryId.ToString());

                IndexRelationAfterLoad(GetHashedRelationName(relation, i), relation.Shema.Indexes.ToList(), database);
            });
            
            timeLog.Stop();
            Logger.Trace($"Данные для {relation.RelationName} проиндексированы за {timeLog.Duration} мс");

            base.LoadComplete(relation, loadCompleteEventArg);
        }

        private void CreateRelation(Relation relation)
        {
            Logger.Trace($"Создание отношения {relation.RelationName}");
            var timeMeter = new TimeMeasureHelper();
            timeMeter.Start();

            RelationManager.SetRelationStatus(relation.RelationId, RelationStatus.Preparing);
            var database = ServiceLocator.Instance.DatabaseService.GetDatabase(Config.ConnectionString, relation.QueryId.ToString());
            for (int i = 0; i < _hashCount; i++)
            {
                var tableName = GetHashedRelationName(relation, i);
                database.CreateRelation(tableName,
                    relation.Shema.Fields.Select(f => f.Name).ToArray(),
                    relation.Shema.Fields.Select(f => f.Params).ToArray());
                IndexRelation(tableName, relation.Shema.Indexes, database);
            }
            RelationManager.SetRelationStatus(relation.RelationId, RelationStatus.Prepared);

            timeMeter.Stop();
            Logger.Trace($"Отношение {relation.RelationName} создано за {timeMeter.Elapsed.TotalMilliseconds} мс");
        }

        private void LoadHashedData(Relation relation, string dirPath, HashedRelationDataPacket packet)
        {
            WaitPause(); //ждем разрешения загрузки очередного блока

            Logger.Trace(
                $"Загрузка данных для {relation.RelationName}, IsLast = {packet.IsLast}, orderNumber = {packet.OrderNumber}, hashedNumber = {packet.HashNumber}");

            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.LoadData, Guid.Empty, Guid.Empty,
                relation.RelationId);

            var taskId = Guid.NewGuid();
            LoadStatusManager.StartLoad(relation.RelationId, taskId, packet.OrderNumber, packet.SourceNodeCount);

            RelationManager.SetRelationStatus(relation.RelationId, RelationStatus.DataTransfering);

            var database =
                ServiceLocator.Instance.DatabaseService.GetDatabase(Config.ConnectionString,
                    relation.QueryId.ToString());

            database.DissableKeys(GetHashedRelationName(relation, packet.HashNumber));
            database.LoadFile(dirPath + GetHashedFileName(relation, packet.OrderNumber, packet.HashNumber),
                GetHashedRelationName(relation, packet.HashNumber));
            database.EnableKeys(GetHashedRelationName(relation, packet.HashNumber));

            timeLog.Stop();
            Logger.Trace($"Данные для {relation.RelationName} загружены за {timeLog.Duration} мс");

            File.Delete(dirPath + GetHashedFileName(relation, packet.OrderNumber, packet.HashNumber));
            
            LoadStatusManager.EndLoad(relation.RelationId, taskId, packet.OrderNumber, packet.IsLast, _hashCount);
        }

        private void LoadData(Relation relation, string dirPath, bool isLast, int orderNumber, int sendNodeCount)
        {
            WaitPause(); //ждем разрешения загрузки очередного блока
            
            Logger.Trace($"Загрузка данных для {relation.RelationName}, IsLast = {isLast}, orderNumber = {orderNumber}");

            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.LoadData, Guid.Empty, Guid.Empty,
                relation.RelationId);

            var taskId = Guid.NewGuid();
            LoadStatusManager.StartLoad(relation.RelationId, taskId, orderNumber, sendNodeCount);

            RelationManager.SetRelationStatus(relation.RelationId, RelationStatus.DataTransfering);

            Parallel.For(0, _hashCount, i =>
            {
                var database =
                    ServiceLocator.Instance.DatabaseService.GetDatabase(Config.ConnectionString, relation.QueryId.ToString());
                database.DissableKeys(GetHashedRelationName(relation, i));
                database.LoadFile(dirPath + GetHashedFileName(relation, orderNumber, i), GetHashedRelationName(relation, i));
                database.EnableKeys(GetHashedRelationName(relation, i));
            });

            timeLog.Stop();
            Logger.Trace($"Данные для {relation.RelationName} загружены за {timeLog.Duration} мс");

            for (var i = 0; i < _hashCount; i++)
            {
                File.Delete(dirPath + GetHashedFileName(relation, orderNumber, i));
            }
            
            LoadStatusManager.EndLoad(relation.RelationId, taskId, orderNumber, isLast, _hashCount);
        }
        
        private void PrepareRelation(Relation relation)
        {
            RelationManager.AddRelation(relation);

            if (!relation.IsEmpty)
            {
                CreateRelation(relation);
            }
            else
            {
                SetRelationStatus(relation.RelationId, RelationStatus.DataTransfered);
            }

            Client.Send(new RelationPreparedPacket() { RelationId = relation.RelationId, QueryId = relation.QueryId });
        }

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
            Logger.Trace("Подготовка отношения " + relation.RelationId);
            StartTask(obj => { PrepareRelation((Relation) obj); }, relation);
        }

        private readonly object _sync = new object();
        private void RelationDataPacketReceivedHandler(PacketBase packetBase)
        {
            var packet = packetBase as RelationDataPacket;
            if (packet == null) return;

            Logger.Trace($"Получен пакет данных {packet.RelationId} OrderNumber={packet.OrderNumber} IsLast={packet.IsLast}");
            var relation = RelationManager.GetRelation(packet.RelationId);
            if (relation != null)
            {
                lock (_sync)
                {
                    _processPacketAction.Invoke(packet, relation);
                }
            }
            else
            {
                Logger.Error($"Отношение не найдено id = {packet.RelationId}");
            }
        }
        private void HashedRelationDataPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as HashedRelationDataPacket;
            if (packet == null) return;

            Logger.Trace($"Получен пакет хешированных данных {packet.RelationId} OrderNumber={packet.OrderNumber} HashNumber={packet.HashNumber} IsLast={packet.IsLast}");
            var relation = RelationManager.GetRelation(packet.RelationId);
            if (relation != null)
            {
                _processHashedPacketAction.Invoke(packet, relation);
            }
            else
            {
                Logger.Error($"Отношение не найдено id = {packet.RelationId}");
            }
        }

        private void ProcessPacket(RelationDataPacket packet, Relation relation)
        {
            Logger.Trace($"Загрузка данных для отношения по очереди {packet.OrderNumber} " + relation.RelationId);

            var dir = SaveFile(packet.Data, packet.OrderNumber, relation);
            
            TaskSequenceLoadManager.Add(relation.RelationId, new Task(obj =>
            {
                var tup = (Tuple<Relation, string, RelationDataPacket>)obj;
                LoadData(tup.Item1, tup.Item2, tup.Item3.IsLast, tup.Item3.OrderNumber, tup.Item3.SourceNodeCount);
            }, new Tuple<Relation, string, RelationDataPacket>(relation, dir, packet)));
        }

        private void ProcessPacketIntoOneFile(RelationDataPacket packet, Relation relation)
        {
            Logger.Trace($"Загрузка данных для отношения по очереди {packet.OrderNumber} " + relation.RelationId);

            _fileSaveSequenceHelper.AddTask(new Task(obj =>
            {
                var tup = (Tuple<RelationDataPacket,Relation>)obj;
                var taskId = Guid.NewGuid();
                _fileSaveStatusManager.StartLoad(tup.Item2.RelationId, taskId, tup.Item1.OrderNumber, tup.Item1.SourceNodeCount);
                SaveInOneFile(tup.Item1.Data, tup.Item2);
                _fileSaveStatusManager.EndLoad(tup.Item2.RelationId, taskId, tup.Item1.OrderNumber, tup.Item1.IsLast);
            }, new Tuple<RelationDataPacket, Relation>(packet, relation)));

        }

        private void ProcessHashedPacket(HashedRelationDataPacket packet, Relation relation)
        {
            Logger.Trace($"Загрузка данных для отношения по очереди {packet.OrderNumber} " + relation.RelationId);

            var dir = SaveFile(packet.Data, packet.OrderNumber, relation, packet.HashNumber);

            TaskSequenceLoadManager.Add(relation.RelationId, new Task(obj =>
            {
                var tup = (Tuple<Relation, string, HashedRelationDataPacket>)obj;
                LoadHashedData(tup.Item1, tup.Item2, tup.Item3);
            }, new Tuple<Relation, string, HashedRelationDataPacket>(relation, dir, packet)));
        }

        private void ProcessHashedPacketIntoOneFile(HashedRelationDataPacket packet, Relation relation)
        {
            Logger.Trace($"Загрузка данных для отношения по очереди {packet.OrderNumber} " + relation.RelationId);

            _fileSaveSequenceHelper.AddTask(new Task(obj =>
            {
                var tup = (Tuple<HashedRelationDataPacket, Relation>)obj;
                var taskId = Guid.NewGuid();
                _fileSaveStatusManager.StartLoad(tup.Item2.RelationId, taskId, tup.Item1.OrderNumber, tup.Item1.SourceNodeCount);
                SaveInOneFile(tup.Item1.Data, tup.Item2, tup.Item1.HashNumber);
                _fileSaveStatusManager.EndLoad(tup.Item2.RelationId, taskId, tup.Item1.OrderNumber, tup.Item1.IsLast);
            }, new Tuple<HashedRelationDataPacket, Relation>(packet, relation)));

        }

        public void LoadRelation(Relation relation, string dirPath, bool isLast, int orderNumber)
        {
            TaskSequenceLoadManager.Add(relation.RelationId, new Task(obj =>
            {
                var tup = (Tuple<Relation, string, bool, int>)obj;
                LoadData(tup.Item1, tup.Item2, tup.Item3, tup.Item4, 1);
            }, new Tuple<Relation, string, bool, int>(relation, dirPath, isLast, orderNumber)));
        }

        public string SaveFile(byte[] data, int orderNumber, Relation relation)
        {
            Logger.Trace($"Сохранение файлов для {relation.RelationName} " + relation.RelationId);
            lock (_fileSaveSync)
            {
                var hasedData = HashData(data, relation);
                var dir = GetDirectoryName();

                var saveTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.FileSave, Guid.Empty,
                    Guid.Empty,
                    relation.RelationId);
                for (var i = 0; i < _hashCount; i++)
                {
                    var fileName = GetFullFileName(relation, orderNumber, i);
                    File.WriteAllBytes(fileName, hasedData[i]);
                }

                saveTime.Stop();
                return dir;
            }
        }

        private string SaveFile(byte[] data, int orderNumber, Relation relation, int hashNumber)
        {
            Logger.Trace($"Сохранение файлов для {relation.RelationName} " + relation.RelationId);
            lock (_fileSaveSync)
            {
                var dir = GetDirectoryName();

                var saveTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.FileSave, Guid.Empty,
                    Guid.Empty,
                    relation.RelationId);
                var fileName = GetFullFileName(relation, orderNumber, hashNumber);
                File.WriteAllBytes(fileName, data);

                saveTime.Stop();
                return dir;
            }
        }

        private List<byte[]> HashData(byte[] data, Relation relation)
        {
            var hashTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.HashData, Guid.Empty,
                Guid.Empty,
                relation.RelationId);
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

            var hashHelper = ServiceLocator.Instance.HashService;
            var hasedData = hashHelper.ProcessData(data, _hashCount, keyIndexes.ToArray());
            hashTime.Stop();
            return hasedData;
        }

        public string SaveInOneFile(byte[] data, Relation relation)
        {
            Logger.Trace($"Сохранение файлов для {relation.RelationName} " + relation.RelationId);
            lock (_fileSaveSync)
            {
                var hasedData = HashData(data, relation);
                var dir = GetDirectoryName();
                var saveTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.FileSave, Guid.Empty,
                    Guid.Empty,
                    relation.RelationId);

                for (var i = 0; i < _hashCount; i++)
                {
                    var fileName = GetFullFileName(relation, 0, i);
                    using (var fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                    {
                        fs.Write(hasedData[i], 0, hasedData[i].Length);
                    }
                }

                saveTime.Stop();
                return dir;
            }
        }

        private void SaveInOneFile(byte[] data, Relation relation, int hashNumber)
        {
            Logger.Trace($"Сохранение файлов для {relation.RelationName} " + relation.RelationId);
            lock (_fileSaveSync)
            {
                var saveTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.FileSave, Guid.Empty,
                    Guid.Empty,
                    relation.RelationId);

                var fileName = GetFullFileName(relation, 0, hashNumber);
                const int numberOfRetries = 10;
                for (var i = 1; i <= numberOfRetries; ++i)
                {
                    try
                    {
                        using (var fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                            fs.Flush();
                        }
                        break;
                    }
                    catch (IOException) when (i <= numberOfRetries)
                    {
                        Logger.Warning($"Файл {fileName} занят..");
                        Thread.Sleep(100);
                    }
                }
                
                saveTime.Stop();
            }
        }
        
        private string GetFullFileName(Relation relation, int orderNumber, int hashNumber)
        {
            return GetDirectoryName() + GetHashedFileName(relation, orderNumber, hashNumber);
        }

        protected override void DropRealtion(object o)
        {
            var taskRealtion = (Relation)o;
            Logger.Trace($"Удаление отношения {taskRealtion.RelationName} id={taskRealtion.RelationId}");

            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.DeleteData, Guid.Empty, Guid.Empty,
                taskRealtion.RelationId);

            if (!taskRealtion.IsEmpty)
            {
                var database =
                    ServiceLocator.Instance.DatabaseService.GetDatabase(Config.ConnectionString,
                        taskRealtion.QueryId.ToString());
                for (var i = 0; i < _hashCount; i++)
                {
                    database.DropTable(GetHashedRelationName(taskRealtion, i));
                }
            }

            timeLog.Stop();

            RelationManager.RemoveRelation(taskRealtion.RelationId);
            TaskSequenceLoadManager.Remove(taskRealtion.RelationId);
        }

        protected override void Dispose(bool disposing)
        {
            DeleteSequenceHelper.Dispose();
            base.Dispose(disposing);
        }
    }
}

