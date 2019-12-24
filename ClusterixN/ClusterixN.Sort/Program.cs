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
using System.Configuration;
using ClusterixN.Common;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.QueryProcessing;
using ClusterixN.QueryProcessing.Data;

namespace ClusterixN.Sort
{
    internal static class Program
    {
        private static Client _client;

        private static void Main()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += AssemblyHelper.AssemblyResolve;
            currentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            TimeLogHelper.InitTimeLogDb();
            string version = AssemblyHelper.GetAssemblyVersion(System.Reflection.Assembly.GetExecutingAssembly());
            string name = ServiceLocator.Instance.ConfigurationService.GetAppSetting("ModuleName");

            PerformanceLogService.Initialize(name, "performanceLogger");
            var logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            logger.Info($"Модуль SORT ({name}) " + version);

            var connectionString =
                ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var blocksize = int.Parse(ServiceLocator.Instance.ConfigurationService.GetAppSetting("BlockSize"));
            var dataDir = ServiceLocator.Instance.ConfigurationService.GetAppSetting("DataDir");
            var syncDelete = ServiceLocator.Instance.ConfigurationService.GetAppSetting("SyncDelete") == "1";
            _client = new Client();
            // ReSharper disable once UnusedVariable
            var qs = new QueryProcessor(_client,
                new QueryProcessConfig() {ConnectionString = connectionString, DataDir = dataDir, BlockLength = blocksize, SyncQueryDrop = syncDelete });
            
            _client.Connect();
            TimeLogService.Initialize(name, "timeLogger", _client.GetLocalEndPoint());

            Console.ReadKey();
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            logger.Fatal("Неожиданное исключение имело место быть");
            logger.Fatal(((Exception)unhandledExceptionEventArgs.ExceptionObject).ToString());
        }
    }
}