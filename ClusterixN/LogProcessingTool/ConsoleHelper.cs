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
using System.Text;

namespace LogProcessingTool
{
    static class ConsoleHelper
    {
        private static DateTime _lastWriteTime = DateTime.MinValue;
        
        public static void ReWriteLine(string text, bool newline = false)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(text.PadRight(Console.BufferWidth - text.Length));
                if (newline) Console.WriteLine();
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                Console.WriteLine(text);
            }
            catch
            {
                // ignored
            }
        }

        public static void ProgressBar(int max, int value, string text)
        {
            if (DateTime.Now - _lastWriteTime < TimeSpan.FromMilliseconds(20) && max != value) return;
            _lastWriteTime = DateTime.Now;

            var message = $"{text}: {value}/{max}" + (max == value ? " done." : "...");
            var tick = (Console.BufferWidth-3)/(float) max;
            var range = value * tick;
            var progressBar = new StringBuilder(Console.BufferWidth-1);
            progressBar.Append("[");
            for (int i = 1; i < Console.BufferWidth - 2; i++)
            {
                progressBar.Append(i > range ? " " : "#");
            }
            progressBar.Append("]");
            ReWriteLine(message);

            WriteProgressBar(max, value, progressBar);
        }

        private static void WriteProgressBar(int max, int value, StringBuilder progressBar)
        {
            try
            {
                Console.CursorTop++;
                if (max != value)
                {
                    ReWriteLine(progressBar.ToString());
                    Console.CursorTop--;
                }
                else
                {
                    ReWriteLine(" ");
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
