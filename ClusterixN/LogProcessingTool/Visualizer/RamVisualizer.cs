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
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using LogProcessingTool.Visualizer.Data;

namespace LogProcessingTool.Visualizer
{
    public class RamVisualizer : GraphVisualizerBase
    {
        private readonly int _ramAvaible;

        public RamVisualizer(float pixelPerMinute, int ramAvaible, int areaHeight) : base (pixelPerMinute, areaHeight)
        {
            _ramAvaible = ramAvaible;
        }
        
        public void Visualize(string db, string fileName, string qid = "")
        {
            var message = "Генерация графика потребления памяти...";
            ConsoleHelper.ReWriteLine(message);
            var logDbConnection = new SQLiteConnection($"Data Source={db}; Version=3;");
            logDbConnection.Open();
            Visualize(GetCounterLog(logDbConnection, "RAM", qid), fileName, GetStartDate(logDbConnection, qid),
                GetEndDate(logDbConnection, qid));
            ConsoleHelper.ReWriteLine(message + " готово.", true);
        }

        private void Visualize(List<NodeCounter> logEvents, string fileName, DateTime startDate, DateTime endDate)
        {
            var orderedLog = logEvents.OrderBy(l=>l.NodeName).ToList();
            var image = Paint(orderedLog, startDate, endDate);
            image.Save(fileName, ImageFormat.Png);
        }

        private Bitmap Paint(List<NodeCounter> log, DateTime startDate, DateTime endDate)
        {
            var xoffset = 150;
            var duration = endDate - startDate;
            var width = (int)Math.Round(duration.TotalMinutes / PixelPerMinute, MidpointRounding.ToEven) + xoffset;

            var yoffset = 0;
            foreach (var node in log)
            {
                yoffset = PaintNode(node, null, yoffset, xoffset, false, startDate, endDate);
            }

            var height = yoffset + 25;
            var image = new Bitmap(width, height);
            var g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);

            PaintTime(image, yoffset, xoffset, true);

            yoffset = 0;
            foreach (var node in log)
            {
                yoffset = PaintNode(node, image, yoffset, xoffset, true, startDate, endDate);
            }

            return image;
        }
        
        private int PaintNode(NodeCounter node, Bitmap image, int offset, int xoffset, bool paint, DateTime startDate, DateTime endDate)
        {
            var endOffset = offset + AreaHeight;
            if (!paint) return endOffset;

            PaintScale(image, offset, xoffset, 0, _ramAvaible);
            var data = node.CounterLogs.Where(c=>c.Time > startDate && c.Time < endDate).ToList();
            var g = Graphics.FromImage(image);
            g.DrawLines(new Pen(Color.Crimson), GetPointsToDraw(data, 0, _ramAvaible, offset, xoffset));
            PaintLine(image, offset, 0, true, node.NodeName, Color.Black);

            return offset + AreaHeight;
        }

        private PointF[] GetPointsToDraw(List<CounterLogData> counters, float min, float max, int offset, int xoffset)
        {
            var points = new List<PointF>();
            var startTime = counters.First().Time;
            foreach (var counter in counters)
            {
                points.Add(new PointF((float) ((counter.Time - startTime).TotalMinutes / PixelPerMinute) + xoffset,
                    offset + NormalizeValue(counter.Value, min, max, AreaHeight)));
            }
            return points.ToArray();
        }
    }
}
