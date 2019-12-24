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
using ClusterixN.Common.Utils;

namespace LogProcessingTool.Export
{
    public class ExportUtils
    {
        /// <summary> Генератор имени файла на основе базового названия </summary>
        /// <param name="baseName">назовоен название файла</param>
        /// <param name="uniq">флаг уникальности файла</param>
        /// <param name="extension">Расширение</param>
        /// <returns>сгенерирование имя</returns>
        public string ExportFileName(string baseName, bool uniq, string extension)
        {
            return string.Format("{0}({1}){2}.{3}",
                baseName,
                SystemTime.Now.ToString("dd.MM.yyyy_hh-mm-ss"),
                uniq ? Guid.NewGuid().ToString() : string.Empty,
                extension);
        }

        public string ExportFileName(string baseName, DateTime date)
        {
            return string.Format("{0}({1:dd.MM.yyyy_hh-mm-ss})",
                baseName, date);
        }

        public string ExportFileName(string baseName, DateTime startdate, DateTime enddate)
        {
            return string.Format("{0}_{1:dd.MM.yyyy_hh-mm-ss}_{2:dd.MM.yyyy_hh-mm-ss}",
                baseName, startdate, enddate);
        }

        /// <summary> Получение темпового имени файла xls в темповой папке </summary>
        /// <param name="baseName">базовое имя</param>
        /// <param name="extension">Расширение</param>
        /// <returns>сгенерирование имя</returns>
        public string ExportTempName(string baseName, string extension = "xls")
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ExportFileName(baseName, true, extension));
        }

        /// <summary> Вспомогательный класс для работы ячейками </summary>
        public class ExcelExtCell
        {
            public ExcelExtCell(string text, int width, string styleName, int mergeAcross = 0, int mergeDown = 0)
            {
                Text = text;
                Width = width;
                StyleName = styleName;
                MergeAcross = mergeAcross;
                MergeDown = mergeDown;
            }

            public string Text { get; set; }
            public int Width { get; set; }
            public string StyleName { get; set; }
            public int MergeAcross { get; set; }
            public int MergeDown { get; set; }
        }
    }
}
