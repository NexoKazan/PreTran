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
using ClusterixN.Common.Utils;

namespace ClusterixN.Common.Data
{
    public class TimeMeasure
    {
        public TimeMeasure(DateTime date, string message, TimeSpan time)
        {
            MeasureDate = date;
            MeasureMessage = message;
            Time = time;
        }

        public TimeMeasure(string message, TimeSpan time) : this(SystemTime.Now, message, time)
        { }

        protected TimeMeasure() : this(SystemTime.Now, string.Empty, TimeSpan.Zero)
        { }

        public TimeSpan Time { get; protected set; }
        public DateTime MeasureDate { get; protected set; }
        public string MeasureMessage { get; protected set; }
    }
}
