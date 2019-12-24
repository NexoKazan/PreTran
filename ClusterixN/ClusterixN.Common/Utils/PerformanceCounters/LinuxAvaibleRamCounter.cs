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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ClusterixN.Common.Utils.PerformanceCounters
{
    class LinuxAvaibleRamCounter : ICounter
    {
        public float CurrentValue => GetFreeMemorySize();

        float GetFreeMemorySize()
        {
            var ramRegex = new Regex(@"[^\s]+\s+\d+\s+(\d+)$");
            var ramPsi = new ProcessStartInfo("free");
            ramPsi.RedirectStandardOutput = true;
            ramPsi.RedirectStandardError = true;
            ramPsi.WindowStyle = ProcessWindowStyle.Hidden;
            ramPsi.UseShellExecute = false;
            ramPsi.Arguments = "-m";
            var free = Process.Start(ramPsi);
            if (free == null) return 0;

            using (System.IO.StreamReader myOutput = free.StandardOutput)
            {
                string output = myOutput.ReadToEnd();
                string[] lines = output.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                lines[1] = lines[1].Trim();
                Match match = ramRegex.Match(lines[1]);
                if (match.Success)
                {
                    try
                    {
                        return Convert.ToSingle(match.Groups[1].Value);
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }
    }
}
