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
using ClusterixN.Common.Data.Query;
using static System.String;

namespace LogProcessingTool
{
    internal class AnswerChecker 
    {
        public void Check(string answerDir, string resultDir)
        {
            var message = "Сравнение результатов с эталоном...";
            ConsoleHelper.ReWriteLine(message, true);
            ConsoleHelper.WriteLine("");
            var queries = LoadQueries(resultDir);
            foreach (var query in queries)
            {
                var resultFileName = $"{query.SequenceNumber:0000}.csv";
                var checkMessage = $"Файл {resultFileName} Запрос: {query.Number}";
                ConsoleHelper.ReWriteLine(checkMessage);
                var unmatchLines = CompareFiles($"{answerDir}\\{GetAnswerNameByQueryNumber(query.Number)}",
                    $"{resultDir}\\{resultFileName}");
                ConsoleHelper.ReWriteLine(checkMessage + " " + (unmatchLines == 0 ? "совпадает." : $"{unmatchLines} строк не совпало"), true);
            }

            ConsoleHelper.ReWriteLine(message + " завершено.", true);
        }

        private List<Query> LoadQueries(string dirName)
        {
            var result = new List<Query>();
            var files = Directory.EnumerateFiles(dirName, "*.xml").ToList();
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                ConsoleHelper.ProgressBar(files.Count, i + 1, "Загрузка запросов");
                result.Add(Query.Load(file));
            }
            return result;
        }

        private string GetAnswerNameByQueryNumber(int queryNumber)
        {
            return $"{queryNumber}.csv";
        }

        private int CompareFiles(string answer, string result)
        {
            var answerFileStream = new FileStream(answer,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            var answerFile = new StreamReader(answerFileStream, System.Text.Encoding.UTF8);

            var resultFileStream = new FileStream(result,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            var resultFile = new StreamReader(resultFileStream, System.Text.Encoding.UTF8);

            var lineNumber = 0;
            var unmatch = 0;
            bool leftReaded;
            bool rightReaded;

            do
            {
                var left = answerFile.ReadLine();
                var right = resultFile.ReadLine();

                leftReaded = left != null;
                rightReaded = right != null;

                lineNumber++;
                if (left == null)
                {
                    left = string.Empty;
                }
                if (right == null)
                {
                    right = string.Empty;
                }

                if (Compare(left, right, StringComparison.Ordinal) != 0)
                {
                    // ReSharper disable once LocalizableElement
                    File.AppendAllText(result + ".diff", $"{lineNumber}: {left} -> {right}\n");
                    unmatch++;
                }
            } while (leftReaded || rightReaded);

            return unmatch;
        }
    }
}