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
using System.Globalization;
using System.IO;
using System.Linq;
using ClusterixN.Common;
using LogProcessingTool.Export;
using LogProcessingTool.Visualizer;
using LogProcessingTool.Visualizer.GUI;

namespace LogProcessingTool
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(@"Программа обработки логов времени со всех модулей системы");
                Console.WriteLine($@"Использование: {AppDomain.CurrentDomain.FriendlyName} [папка с запросами и логами времени]");
                Console.WriteLine($@"Например: {AppDomain.CurrentDomain.FriendlyName} 20170711_182942");
                Console.WriteLine(@"Конфигурационные параметры:");
                Console.WriteLine(@" --consolidate - сбор всех данных из указанной папки в одну БД");
                Console.WriteLine(@" --report - создания отчета по консолидированной БД");
                Console.WriteLine(@" --visualize - визуализация работы системы по консолидированной БД");
                Console.WriteLine(@"    -vp [float] - масштаб визуализации (пикселей на минуту)");
                Console.WriteLine(@"    -vh [int] - высота бара");
                Console.WriteLine(@"    -vram [int] - максимальный объем RAM");
                Console.WriteLine(@"    -vnet [int] - максимальная скорсоть передачи по сети");
                Console.WriteLine(@"    -vaper [int] - период усреднения в минутах");
                Console.WriteLine(@"    -qid [guid] - идентификатор запроса для визуализации");
                Console.WriteLine(@" --check [dir] - проверка результата с эталоном");
                Console.WriteLine(@" --timecount [outfile] - подсчет времени обработки запросов и запись в outfile");
                return;
            }

            Console.CursorVisible = false;

            var resultDb = Path.GetFullPath(args[0]) + ".db";
            var dir = args[0];
            var logProcess = new LogProcessor();
            var report = new Report(resultDb);
            var pixelPerMinute = 0.1f;
            var height = 5;
            var ram = 131072;
            var net = 1024 * 1024 * 1024 / 8;
            var cpuAvgPeriod = 0;

            if (!args.Contains("--report") && 
                !args.Contains("--consolidate") && 
                !args.Contains("--visualize") && 
                !args.Contains("--check") &&
                !args.Contains("--timecount"))
            {
                var oldArgs = args;
                args = new string[oldArgs.Length+5];
                int argIndex;
                for (argIndex = 0; argIndex < oldArgs.Length; argIndex++)
                {
                    args[argIndex] = oldArgs[argIndex];
                }
                args[argIndex++] = "--consolidate";
                args[argIndex++] = "--report";
                args[argIndex++] = "--visualize";
                args[argIndex++] = "--timecount";
                args[argIndex] = args[0] + ".txt";
            }

            if (args.Contains("--consolidate"))
                logProcess.Process(resultDb, dir);
            if (args.Contains("--report"))
                report.SaveReport(Path.GetFileNameWithoutExtension(resultDb) + ".xlsx");
            if (args.Contains("--visualize"))
            {
                var qid = string.Empty;
                if (args.Contains("--gui"))
                {
                    var visualizer = new OperationVisualizer(pixelPerMinute, height, resultDb);
                    var view = new VizView(visualizer);
                    view.ShowDialog();
                    return;
                }
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].Contains("-vp"))
                        pixelPerMinute = float.Parse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (args[i].Contains("-vh"))
                        height = int.Parse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (args[i].Contains("-vram"))
                        ram = int.Parse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (args[i].Contains("-vnet"))
                        net = int.Parse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (args[i].Contains("-vaper"))
                        cpuAvgPeriod = int.Parse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (args[i].Contains("-qid"))
                        qid = args[i + 1];
                }

                var ovz = new OperationVisualizer(pixelPerMinute, height, resultDb, qid) {TimeFormat = GetTimeFormat()};
                ovz.Visualize(Path.GetFileNameWithoutExtension(resultDb) + ".png");

                var rvz = new RamVisualizer(pixelPerMinute, ram, 100) {TimeFormat = GetTimeFormat()};
                rvz.Visualize(resultDb, Path.GetFileNameWithoutExtension(resultDb) + "_ram.png", qid);

                var cvz = new CpuVisualizer(pixelPerMinute, 100, TimeSpan.FromMinutes(cpuAvgPeriod)) {TimeFormat = GetTimeFormat()};
                cvz.Visualize(resultDb, Path.GetFileNameWithoutExtension(resultDb) + "_cpu.png", qid);

                var nvz = new NetVisualizer(pixelPerMinute, net, 100) {TimeFormat = GetTimeFormat()};
                nvz.Visualize(resultDb, Path.GetFileNameWithoutExtension(resultDb) + "_net.png", qid);

                var concat = new GraphConCat(Path.GetFileNameWithoutExtension(resultDb) + ".png",
                    Path.GetFileNameWithoutExtension(resultDb) + "_cpu.png",
                    Path.GetFileNameWithoutExtension(resultDb) + "_ram.png",
                    Path.GetFileNameWithoutExtension(resultDb) + "_net.png");
                concat.Concat(Path.GetFileNameWithoutExtension(resultDb) + "_concat.png");
            }

            if (args.Contains("--check"))
            {
                var answerDir = string.Empty;
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].Contains("--check"))
                        answerDir = args[i + 1];
                }
                var checker = new AnswerChecker();
                checker.Check(answerDir, dir);
            }

            if (args.Contains("--timecount"))
            {
                var outfile = "timecount.txt";
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].Contains("--timecount"))
                        outfile = args[i + 1];
                }
                var counter = new QueryExecutionCounter();
                var countResult = counter.Count(resultDb);
                File.WriteAllText(outfile, countResult);
            }

            Console.CursorVisible = true;
        }

        private static string GetTimeFormat()
        {
            try
            {
                var timeFormat = ServiceLocator.Instance.ConfigurationService.GetAppSetting("TimeFormat");
                return timeFormat;
            }
            catch (Exception)
            {
                Console.WriteLine(@"Настройка формата времени не определена. Используется значение по умолчанию.");
            }

            return @"hh\:mm";
        }
    }
}