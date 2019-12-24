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
using System.Globalization;
using LogProcessingTool.Visualizer.Data;

namespace LogProcessingTool.Visualizer
{
    public abstract class GraphVisualizerBase : VisualizerBase
    {
        protected readonly int AreaHeight;

        protected GraphVisualizerBase(float pixelPerMinute, int areaHeight) : base(pixelPerMinute)
        {
            AreaHeight = areaHeight;
        }
       
        protected void PaintScale(Bitmap image, int yoffset, int xoffset, float min, float max)
        {
            var g = Graphics.FromImage(image);
            g.DrawLine(new Pen(Color.Black, 1), xoffset, yoffset, xoffset, yoffset + AreaHeight);
            var textWidth = 100;

            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Far
            };

            for (var y = yoffset + AreaHeight; y > yoffset; y -= 25)
            {
                var value = (AreaHeight + (yoffset - y)) * ((max - min) / AreaHeight);
                g.DrawLine(new Pen(y == yoffset + AreaHeight ? Color.Black : Color.LightGray, 1), xoffset, y, image.Width, y);
                g.DrawLine(new Pen(Color.Black, 1), xoffset - 2, y, xoffset + 2, y);
                g.DrawString(value.ToString("F0"), new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.Black),
                    new RectangleF(xoffset - textWidth, y - 20, textWidth, 20), stringFormat);
            }
        }

        protected float NormalizeValue(float value, float min, float max, float height)
        {
            return value / (max - min) * height;
        }


        protected DateTime GetStartDate(SQLiteConnection connection, string qid)
        {
            if (string.IsNullOrWhiteSpace(qid))
            {
                return GetDate(connection, "SELECT Timestamp FROM times t ORDER BY Timestamp LIMIT 1;", "Timestamp");
            }
            else
            {
                return GetDate(connection,
                    $"SELECT Timestamp FROM times t WHERE QueryId = '{qid}' AND Operation NOT IN ('NOP','WaitJoin','WaitSort','WaitStart') ORDER BY Timestamp LIMIT 1;",
                    "Timestamp");
            }
        }

        protected DateTime GetEndDate(SQLiteConnection connection, string qid)
        {
            if (string.IsNullOrWhiteSpace(qid))
            {
                return GetDate(connection, "SELECT Timestamp FROM times t ORDER BY Timestamp DESC LIMIT 1;", "Timestamp");
            }
            else
            {
                return GetDate(connection, $"SELECT Timestamp FROM times t WHERE QueryId = '{qid}' ORDER BY Timestamp DESC LIMIT 1;", "Timestamp");
            }
        }

        private DateTime GetDate(SQLiteConnection connection, string query, string key)
        {
            var date = DateTime.Now;
            var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    date = DateTime.Parse((string)reader[key]);
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return date;
        }

        protected List<NodeCounter> GetCounterLog(SQLiteConnection connection, string counterName, string qid)
        {
            var result = new List<NodeCounter>();
            var cmd = connection.CreateCommand();
            cmd.CommandText =
                $"SELECT Timestamp, Module, Value FROM performance p WHERE p.Counter = '{counterName}' ORDER BY p.Module, p.Timestamp; ";
            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var module = reader["Module"].ToString();
                    bool found = false;
                    foreach (var nodeCounter in result)
                    {
                        if (nodeCounter.NodeName != module) continue;

                        nodeCounter.CounterLogs.Add(new CounterLogData()
                        {
                            Time = DateTime.Parse((string)reader["Timestamp"]),
                            Value = float.Parse(reader["Value"].ToString().Replace(",", "."), CultureInfo.InvariantCulture)
                        });
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        result.Add(new NodeCounter()
                        {
                            NodeName = module,
                            CounterLogs = new List<CounterLogData>()
                            {
                                new CounterLogData()
                                {
                                    Time = DateTime.Parse((string)reader["Timestamp"]),
                                    Value = float.Parse(reader["Value"].ToString().Replace(",","."), CultureInfo.InvariantCulture)
                                }
                            }
                        });
                    }

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
    }
}
