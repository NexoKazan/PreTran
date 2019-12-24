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
using System.IO;
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Managers;
using ClusterixN.QueryProcessing.Services;

namespace ClusterixN.QueryProcessing
{
    public class QueryProcessor
    {
        private readonly ILogger _logger;
        private readonly QueryProcessConfig _dbConfig;

        public QueryProcessor(ICommunicator client, QueryProcessConfig dbConfig)
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _dbConfig = dbConfig;
            _dbConfig.DataDir = GetDataDirectoryPath(dbConfig.DataDir);
            var loadStatusManager = new LoadStatusManager();
            var relationManager = new RelationManager();
            var taskSequenceLoadManager = new TaskSequenceLoadManager();

            QueryProcessorServiceLocator.Instance.RegisterObject(client);
            QueryProcessorServiceLocator.Instance.RegisterObject(_dbConfig);
            QueryProcessorServiceLocator.Instance.RegisterObject(loadStatusManager);
            QueryProcessorServiceLocator.Instance.RegisterObject(taskSequenceLoadManager);
            QueryProcessorServiceLocator.Instance.RegisterObject(relationManager);

            QueryProcessorServiceLocator.Instance.RegisterService<CommandService>();
            if (string.Compare(ServiceLocator.Instance.ConfigurationService.GetAppSetting("WorkMode"), "Hash", StringComparison.OrdinalIgnoreCase) == 0)
            {
                QueryProcessorServiceLocator.Instance.RegisterService<HashRelationService>();
            }
            else
            {
                QueryProcessorServiceLocator.Instance.RegisterService<RelationService>();
            }

            InitServices();
            
            _logger.Trace($"Инициализирован обработчик запросов с БД: {dbConfig.ConnectionString}");
            DropTmpRealtionsFromDb();
        }

        private void InitServices()
        {
            //QueryProcessorServiceLocator.Instance.RegisterService<SelectService>();
            QueryProcessorServiceLocator.Instance.RegisterService<SelectAndSendService>();
            QueryProcessorServiceLocator.Instance.RegisterService<JoinService>();
            QueryProcessorServiceLocator.Instance.RegisterService<SortService>();
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

        private void DropTmpRealtionsFromDb()
        {
            _logger.Trace("Очистка БД");
            using (var database =
                ServiceLocator.Instance.DatabaseService.GetDatabase(_dbConfig.ConnectionString, string.Empty,
                    newInstance: true))
            {
                database.DropTmpRealtions();
            }
        }
    }
}
