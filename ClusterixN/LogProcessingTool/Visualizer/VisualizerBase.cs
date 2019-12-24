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

namespace LogProcessingTool.Visualizer
{
    public abstract class VisualizerBase
    {
        protected float PixelPerMinute { get; set; }

        public string TimeFormat { get; set; } = @"hh\:mm";

        public VisualizerBase(float pixelPerMinute)
        {
            PixelPerMinute = pixelPerMinute;
        }

        protected void PaintLine(Bitmap image, int yoffset, int xoffset, bool paint, string text, Color lineColor)
        {
            if (!paint) return;

            var g = Graphics.FromImage(image);

            g.DrawLine(new Pen(lineColor, 1), xoffset, yoffset, image.Width, yoffset);
            g.DrawString(text, new Font(FontFamily.GenericMonospace, 10),new SolidBrush(Color.Black), xoffset, yoffset);

        }

        protected void PaintTime(Bitmap image, int yoffset, int xoffset, bool paint)
        {
            if (!paint) return;

            var y = yoffset;
            var g = Graphics.FromImage(image);

            g.DrawLine(new Pen(Color.Black, 1), xoffset, y, image.Width, y);
            var step = 100;
            var textWidth = 80;

            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            for (int i = xoffset; i < image.Width; i+= step)
            {
                g.DrawLine(new Pen(Color.LightGray, 1), i, 0, i, y + 2);
                g.DrawLine(new Pen(Color.Black, 1), i, y-2, i, y+2);
                g.DrawString(TimeSpan.FromMinutes((i-xoffset)*PixelPerMinute).ToString(TimeFormat), new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.Black),
                    new RectangleF(i - textWidth / 2, y + 3, textWidth, 20), stringFormat);
            }

        }
    }
}
