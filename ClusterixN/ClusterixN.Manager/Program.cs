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
using System.Reflection;
using ClusterixN.Common;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Manager.QueryGenerators;
using ClusterixN.Terminal;

namespace ClusterixN.Manager
{
    internal static class Program
    {
        private static Server _server;
        private static QueryProcessingHandler _queryProcessingHandler;
        private static Terminal.Terminal _terminal;
        private static ILogger _logger;

        private static void Main()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += AssemblyHelper.AssemblyResolve;
            currentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            TimeLogHelper.InitTimeLogDb();
            var version = AssemblyHelper.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            var name = ServiceLocator.Instance.ConfigurationService.GetAppSetting("ModuleName");
            var dataDir = ServiceLocator.Instance.ConfigurationService.GetAppSetting("DataDir");

            TimeLogService.Initialize(name, "timeLogger");
            PerformanceLogService.Initialize(name, "performanceLogger");

            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _logger.Info($"Модуль управления ({name}) " + version);

            InitTerminal();

            _server = new Server();
            _queryProcessingHandler = new QueryProcessingHandler(_server, dataDir);

            _server.Start();

            _terminal.Start();

            _server.Stop();
            _queryProcessingHandler.Dispose();
        }
        
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            logger.Fatal("Неожиданное исключение имело место быть");
            logger.Fatal(((Exception)unhandledExceptionEventArgs.ExceptionObject).ToString());
        }

        private static void InitTerminal()
        {
            _logger.Info("Инициализация терминала");
            _terminal = new Terminal.Terminal();
            _terminal.CommandError += (sender, arg) => { _logger.Error(arg.Exception); };
            _terminal.RegisterComand(new TerminalCommandBase("startq", "Запуск запроса")
            {
                Arguments = new List<CommandArgument>
                {
                    new CommandArgument("-n", "номер запроса", true, CommandArgumentType.Int)
                },
                ActionEvent = StartCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("startbench", "Запуск 14 запросов на выполнение")
            {
                Arguments = new List<CommandArgument>
                {
                    new CommandArgument("-c", "количество повторений", false, CommandArgumentType.Int),
                },
                ActionEvent = StartbenchCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("gettimelog", "Запрос лога времени со всех узлов")
            {
                ActionEvent = GetTimeLogCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("startstream", "Запуск потока запросов на выполнение")
            {
                Arguments = new List<CommandArgument>
                {
                    new CommandArgument("-c", "количество запросов", true, CommandArgumentType.Int),
                    new CommandArgument("-l", "длина очереди", true, CommandArgumentType.Int),
                },
                ActionEvent = StartStreamCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("tpchseq", "Запуск последовательности запросов для заданного числа перестановок")
            {
                Arguments = new List<CommandArgument>
                {
                    new CommandArgument("-c", "количество перестановок", true, CommandArgumentType.Int),
                    new CommandArgument("-l", "длина очереди", true, CommandArgumentType.Int),
                },
                ActionEvent = TpchCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("tpchstream", "Запуск последовательности запросов для заданного числа перестановок")
            {
                Arguments = new List<CommandArgument>
                {
                    new CommandArgument("-c", "количество запросов", true, CommandArgumentType.Int),
                    new CommandArgument("-l", "длина очереди", true, CommandArgumentType.Int),
                },
                ActionEvent = TpchStreamCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("pause", "остановка IO")
            {
                ActionEvent = PauseCommandActionEvent
            });
            _terminal.RegisterComand(new TerminalCommandBase("resume", "остановка IO")
            {
                ActionEvent = ResumeCommandActionEvent
            });
        }

        private static void ResumeCommandActionEvent(object sender, EventArgs e)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                _queryProcessingHandler.SendCommand(Command.Resume);
            }
        }

        private static void PauseCommandActionEvent(object sender, EventArgs e)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                _queryProcessingHandler.SendCommand(Command.Pause);
            }
        }

        private static void GetTimeLogCommandActionEvent(object sender, EventArgs e)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                _queryProcessingHandler.GetLogDb();
            }
        }

        private static void StartbenchCommandActionEvent(object sender, EventArgs e)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                var arg = command.Arguments.First(a => a.Argument.Equals("-c"));
                var count = 1;
                if (arg.IsSet)
                {
                    count = int.Parse(arg.Value);
                }
                for (var j = 0; j < count; j++)
                {
                    for (var i = 1; i <= 14; i++)
                        _queryProcessingHandler.AddQueryByNumber(i);
                }
            }
        }

        private static void StartCommandActionEvent(object sender, EventArgs eventArgs)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                var arg = command.Arguments.First(a => a.Argument.Equals("-n"));
                _queryProcessingHandler.AddQueryByNumber(int.Parse(arg.Value));
            }
        }

        private static void StartStreamCommandActionEvent(object sender, EventArgs eventArgs)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                var count = command.Arguments.First(a => a.Argument.Equals("-c"));
                var lenght = command.Arguments.First(a => a.Argument.Equals("-l"));
                _queryProcessingHandler.StartQueryStream(int.Parse(count.Value), int.Parse(lenght.Value),
                    new RandomQueryNumberGenerator());
            }
        }

        private static void TpchCommandActionEvent(object sender, EventArgs eventArgs)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                var count = command.Arguments.First(a => a.Argument.Equals("-c"));
                var lenght = command.Arguments.First(a => a.Argument.Equals("-l"));
                _queryProcessingHandler.StartQueryStream(int.Parse(count.Value) * 14, int.Parse(lenght.Value),
                    new TpchQueryNumberGenerator());
            }
        }

        private static void TpchStreamCommandActionEvent(object sender, EventArgs eventArgs)
        {
            var command = sender as TerminalCommandBase;
            if (command != null)
            {
                var count = command.Arguments.First(a => a.Argument.Equals("-c"));
                var lenght = command.Arguments.First(a => a.Argument.Equals("-l"));
                _queryProcessingHandler.StartQueryStream(int.Parse(count.Value), int.Parse(lenght.Value),
                    new TpchQueryNumberGenerator());
            }
        }
    }
}