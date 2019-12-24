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

using System;
using System.Collections.Generic;
using System.Linq;

namespace LogProcessingTool.Visualizer.Data
{
    public class NodeCounter
    {
        public string NodeName { get; set; }
        public List<CounterLogData> CounterLogs { get; set; }

        public List<CounterLogData> GetAverage2(TimeSpan period)
        {
            var startDate = CounterLogs.Min(c => c.Time);
            var maxDate = CounterLogs.Max(c => c.Time);
            var points = new List<CounterLogData>((int) ((maxDate - startDate).Ticks / period.Ticks) + 1);
            int pointCount;
            double pointVlaueSum;

            while (startDate < maxDate)
            {
                pointCount = 0;
                pointVlaueSum = 0;
                foreach (var periodPoint in CounterLogs.Where(c => c.Time > startDate && c.Time < startDate + period))
                {
                    pointCount++;
                    pointVlaueSum += periodPoint.Value;
                }

                points.Add(new CounterLogData()
                {
                    Time = startDate + TimeSpan.FromTicks(period.Ticks / 2),
                    Value = (float) (pointVlaueSum / pointCount)
                });
                startDate += period;
            }

            return points;
        }

        public List<CounterLogData> GetAverage(TimeSpan period)
        {
            var points = new List<CounterLogData>(CounterLogs.Count);

            foreach (var counterLog in CounterLogs)
            {
                var pointCount = 0;
                double pointVlaueSum = 0;
                foreach (var periodPoint in CounterLogs.Where(c => c.Time >= counterLog.Time && c.Time < counterLog.Time + period))
                {
                    pointCount++;
                    pointVlaueSum += periodPoint.Value;
                }

                points.Add(new CounterLogData()
                {
                    Time = counterLog.Time,
                    Value = (float)(pointVlaueSum / pointCount)
                });
            }
            
            return points;
        }
    }
}
