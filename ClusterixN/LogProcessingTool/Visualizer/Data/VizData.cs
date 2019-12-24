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
using System.Drawing;
using System.Drawing.Drawing2D;
using ClusterixN.Common.Data.Log.Enum;

namespace LogProcessingTool.Visualizer.Data
{
    public class VizData
    {
        public TimeSpan Start { get; set; }
        public TimeSpan Duration { get; set; }
        public Brush Brush => MeasuredOperationToBrush(IsCanceled);
        public string OperationName => GetOperationName();
        public bool IsCanceled { get; set; }
        public MeasuredOperation Operation { get; set; }


        private Brush MeasuredOperationToBrush(bool isCanceled)
        {
            if (isCanceled)
            {
                var foreground = Color.Red;
                return new HatchBrush(HatchStyle.BackwardDiagonal, foreground, GetColor(Operation));
            }
            else
            {
                return new SolidBrush(GetColor(Operation));
            }
        }

        private Color GetColor(MeasuredOperation measuredOperation)
        {
            switch (measuredOperation)
            {
                case MeasuredOperation.NOP:
                case MeasuredOperation.WaitStart:
                case MeasuredOperation.WaitJoin:
                case MeasuredOperation.WaitSort:
                case MeasuredOperation.WorkDuration: return Color.White;

                case MeasuredOperation.ProcessingSelect: return Color.FromArgb(255, 186, 194, 196);
                case MeasuredOperation.ProcessingJoin: return Color.FromArgb(255, 182, 217, 87);
                case MeasuredOperation.ProcessingSort: return Color.FromArgb(255, 100, 150, 200);
                case MeasuredOperation.HashData: return Color.FromArgb(255, 100, 200, 125);
                case MeasuredOperation.FileSave: return Color.FromArgb(255, 217, 152, 203);
                case MeasuredOperation.LoadData: return Color.FromArgb(255, 242, 210, 73);
                case MeasuredOperation.DataTransfer: return Color.FromArgb(255, 92, 186, 230);
                case MeasuredOperation.DeleteData: return Color.FromArgb(255, 255, 150, 168);
                case MeasuredOperation.Pause: return Color.Red;
                case MeasuredOperation.Indexing: return Color.Brown;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetOperationName()
        {
            switch (Operation)
            {
                case MeasuredOperation.NOP:
                case MeasuredOperation.WaitStart:
                case MeasuredOperation.WaitJoin:
                case MeasuredOperation.WaitSort:
                case MeasuredOperation.WorkDuration: return "";

                case MeasuredOperation.ProcessingSelect: return "Выполнение «select-project»";
                case MeasuredOperation.ProcessingJoin: return "Выполнение «join»";
                case MeasuredOperation.ProcessingSort: return "Выполнение «sort»";
                case MeasuredOperation.HashData: return "Хеширование данных";
                case MeasuredOperation.FileSave: return "Подготовка к загрузке в MySQL";
                case MeasuredOperation.LoadData: return "Загрузка данных в MySQL";
                case MeasuredOperation.DataTransfer: return "Передача данных";
                case MeasuredOperation.DeleteData: return "Удаление отношений Ri’";
                case MeasuredOperation.Pause: return "Ожидание";
                case MeasuredOperation.Indexing: return "Индексация";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
