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
using ClusterixN.Common.Data.Log.Enum;
using LogProcessingTool.Visualizer.Data;

namespace LogProcessingTool.Visualizer
{
    public class OperationVisualizer : VisualizerBase
    {
        private readonly int _brushHeight;
        private readonly string _db;
        private readonly string _qid;
        private SQLiteConnection _logDbConnection;
        private List<VizNode> _vizNodes;
        private int _legendWidth = 340;

        private SQLiteConnection DbConnection
        {
            get
            {
                if (_logDbConnection == null)
                {
                    _logDbConnection = new SQLiteConnection($"Data Source={_db}; Version=3;");
                    _logDbConnection.Open();
                }
                return _logDbConnection;
            }
        }

        public List<VizNode> VizNodes => _vizNodes ?? (_vizNodes = ConvertToVizNodes(GetTimeLog(DbConnection)));

        public TimeSpan Duration
        {
            get
            {
                return VizNodes.SelectMany(v => v.VizData.Select(d => d.Start + d.Duration)).Max();
            }
        }

        public OperationVisualizer(float pixelPerMinute, int brushHeight, string db, string qid = "") : base(pixelPerMinute)
        {
            _brushHeight = brushHeight;
            _db = db;
            _qid = qid;
        }

        private List<LogData> GetTimeLog(SQLiteConnection connection)
        {
            var result = new List<LogData>();
            var cmd = connection.CreateCommand();
            if (string.IsNullOrWhiteSpace(_qid))
            {
                cmd.CommandText =
                "SELECT Timestamp, Module, Operation, Duration, IsCanceled FROM times t LEFT JOIN query q ON t.QueryId = q.Id; ";
            }
            else
            {
                cmd.CommandText =
                    $"SELECT Timestamp, Module, Operation, Duration, IsCanceled FROM times t LEFT JOIN query q ON t.QueryId = q.Id where q.Id = '{_qid}'; ";
            }
            var ignoredOperations = new List<MeasuredOperation>()
            {
                MeasuredOperation.NOP,
                MeasuredOperation.WaitJoin,
                MeasuredOperation.WaitSort,
                MeasuredOperation.WaitStart,
                MeasuredOperation.WorkDuration
            };
            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int canceled;
                    int.TryParse(reader["IsCanceled"].ToString(), out canceled);
                    var log = new LogData
                    {
                        Duration = double.Parse(reader["Duration"].ToString()),
                        Time = DateTime.Parse((string) reader["Timestamp"]),
                        Module = (string) reader["Module"],
                        IsCanceled = canceled == 1,
                        Operation =
                            (MeasuredOperation) Enum.Parse(typeof(MeasuredOperation), (string) reader["Operation"])
                    };
                    if (ignoredOperations.Contains(log.Operation)) continue;
                    result.Add(log);
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return result;
        }

        public void Visualize(string fileName)
        {
            var message = "Генерация диаграммы работы...";
            ConsoleHelper.ReWriteLine(message);
            Visualize(GetTimeLog(DbConnection), fileName);
            ConsoleHelper.ReWriteLine(message + " готово.", true);
        }

        public void Visualize(List<LogData> logEvents, string fileName)
        {
            var vizNodes = ConvertToVizNodes(logEvents);
            var image = Paint(vizNodes);
            image.Save(fileName, ImageFormat.Png);
        }

        public Bitmap Visualize(TimeSpan start, TimeSpan duration, int width)
        {
            PixelPerMinute = (float) (duration.TotalMinutes / (width - _legendWidth));
            var viz = new List<VizNode>();
            foreach (var vizNode in VizNodes)
            {
                var newNode = new VizNode()
                {
                    Name = vizNode.Name
                };

                for (int j = 0; j < vizNode.VizData.Count; j++)
                {
                    var vizData = vizNode.VizData[j];

                    if (vizData.Start >= start && vizData.Start <= (start + duration))
                    {
                        newNode.VizData.Add(new VizData()
                        {
                            Operation = vizData.Operation,
                            Start = vizData.Start,
                            Duration = vizData.Duration,
                            IsCanceled = vizData.IsCanceled
                        });
                    }
                }

                viz.Add(newNode);
            }
            return Paint(viz);
        }

        private static List<VizNode> ConvertToVizNodes(List<LogData> logEvents)
        {
            var orderedLog = logEvents.OrderBy(l => l.Module).ThenBy(l => l.Operation).ThenBy(l => l.Time);
            var minDate = logEvents.Min(l => l.Time);
            var vizNodes = new List<VizNode>();

            foreach (var groupedlog in orderedLog.GroupBy(l => l.Module))
            {
                var vizNode = new VizNode() {Name = groupedlog.Key};
                foreach (var log in groupedlog)
                {
                    vizNode.VizData.Add(new VizData()
                    {
                        Start = log.Time - minDate,
                        Operation = log.Operation,
                        Duration = TimeSpan.FromMilliseconds(log.Duration),
                        IsCanceled = log.IsCanceled
                    });
                }
                vizNodes.Add(vizNode);
            }
            return vizNodes;
        }

        private Bitmap Paint(List<VizNode> vizNodes)
        {
            var xoffset = _legendWidth;
            var duration = vizNodes.SelectMany(v => v.VizData.Select(d => d.Start + d.Duration)).Max();
            var width = (int) Math.Round(duration.TotalMinutes / PixelPerMinute, MidpointRounding.ToEven) + xoffset;

            var yoffset = 0;
            foreach (var vizNode in vizNodes)
            {
                yoffset = PaintNode(vizNode, null, yoffset, xoffset, false);
            }

            var height = yoffset+25;
            var image = new Bitmap(width, height);
            var g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);

            PaintTime(image, yoffset, xoffset, true);

            yoffset = 0;
            foreach (var vizNode in vizNodes)
            {
                yoffset = PaintNode(vizNode, image, yoffset, xoffset, true);
            }
            //PaintLegend(vizNodes, image, yoffset, xoffset, true);

            return image;
        }

        private int PaintNode(VizNode node, Bitmap image, int offset, int xoffset, bool paint)
        {
            var data = node.VizData.GroupBy(v => v.Operation);
            var endOffset = offset;

            foreach (var d in data)
            {
                PaintLine(image, endOffset, 80, paint, d.First().OperationName, Color.LightGray);
                var newOffset = PaintOperation(d.ToList(), image, endOffset, xoffset, paint);
                endOffset += newOffset < 20 ? 20 : newOffset;

            }
            if (paint) PaintLine(image, offset, 0, true, node.Name, Color.Black);

            return endOffset - offset < 20 ? 20 + offset : endOffset;
        }

        // ReSharper disable once UnusedMember.Local
        private void PaintLegend(List<VizNode> nodes, Bitmap image, int yoffset, int xoffset, bool paint)
        {
            if (!paint) return;

            var y = yoffset + 30;
            var g = Graphics.FromImage(image);
            var legend = GetLegend(nodes);
            var textHeight = 20;
            var textWidth = 120;

            foreach (var item in legend)
            {
                g.FillRectangle(item.Brush, xoffset, y, 100, _brushHeight);
                g.DrawString(item.Operation.ToString(), new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.Black), xoffset+ 120, y, StringFormat.GenericDefault);
                g.DrawString(item.Duration.TotalSeconds.ToString("F"), new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.Black),
                    xoffset + 120 + textWidth + 20, y, StringFormat.GenericDefault);
                y += textHeight;
            }
        }

        private int PaintOperation(List<VizData> vizData, Bitmap image, int yoffset, int xoffset, bool paint)
        {
            var g = paint ? Graphics.FromImage(image) : Graphics.FromImage(new Bitmap(1,1));
            var lineLengths = new List<float>();

            foreach (var data in vizData)
            {
                var i = 0;
                var lineXoffset = (float) (data.Start.TotalMinutes / PixelPerMinute) + xoffset;

                foreach (var len in lineLengths)
                {
                    if (len-2 < lineXoffset) break; //чтобы наложенные при округлении прямоугольники располагались друг за другом ( 2 = 1(наложение) + 1(растяжение коротких промежутков)
                    i++;
                }
                if (i >= lineLengths.Count) lineLengths.Add(0);

                var x = lineXoffset;
                var y = yoffset + i * _brushHeight;
                var width = (float) (data.Duration.TotalMinutes / PixelPerMinute);
                width = width < 1 ? 1 : width;
                var height = _brushHeight;
                lineLengths[i] = x + width;

                if (paint)
                {
                    g.FillRectangle(data.Brush, x, y, width, height);
                    //g.DrawRectangle(new Pen(Color.Black), x, y, width, height);
                }
            }

            return lineLengths.Count * _brushHeight;
        }

        private List<VizData> GetLegend(List<VizNode> nodes)
        {
            var legend = new List<VizData>();
            var gropByOperation = nodes.SelectMany(n => n.VizData).OrderBy(n => n.Operation).GroupBy(n => n.Operation);

            foreach (var data in gropByOperation)
            {
                legend.Add(new VizData() {Operation = data.Key, Duration = ProjectDuration(data.ToList()) });
            }

            return legend;
        }

        private TimeSpan ProjectDuration(List<VizData> data)
        {
            var lineLengths = new List<VizData>();
            var mergePerformed = false;

            foreach (var vizData in data)
            {
                var merged = false;
                foreach (var len in lineLengths)
                {
                    if (len.Start + len.Duration >= vizData.Start &&
                        len.Start + len.Duration <= vizData.Start + vizData.Duration)
                    {
                        len.Duration = vizData.Start + vizData.Duration - len.Start;
                        merged = mergePerformed = true;
                    }
                    else if (len.Start <= vizData.Start &&
                        len.Start + len.Duration >= vizData.Start + vizData.Duration)
                    {
                        merged = mergePerformed = true;
                    }
                    else if (len.Start >= vizData.Start &&
                             len.Start <= vizData.Start + vizData.Duration)
                    {
                        len.Duration = len.Start + len.Duration - vizData.Start;
                        merged = mergePerformed = true;
                    }
                }
                if (!merged) lineLengths.Add(vizData);
            }
            if (!mergePerformed) return TimeSpan.FromSeconds(lineLengths.Sum(l => l.Duration.TotalSeconds));

            return ProjectDuration(lineLengths);
        }
    }
}
