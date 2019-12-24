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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogProcessingTool.Visualizer.Data;

namespace LogProcessingTool.Visualizer.GUI
{
    public partial class VizView : Form
    {
        private readonly OperationVisualizer _visualizer;
        private TimeSpan _duration;
        private const float DefaultZoom = 0.01f;
        private int _legendWidth = 340;
        private readonly int _brushHeight;
        private TimeSpan _windowsStart;
        
        private float MinutesPerPixel { get; set; }

        private TimeSpan WindowDuration => GetDuration();

        private TimeSpan WindowsStart
        {
            get { return _windowsStart; }
            set
            {
                if (value != _windowsStart)
                {
                    _windowsStart = value;
                    paintPanel.Invalidate();
                }
            }
        }

        public VizView(OperationVisualizer visualizer)
        {
            _visualizer = visualizer;
            _duration = _visualizer.Duration;
            _brushHeight = 5;
            WindowsStart = TimeSpan.Zero;
            InitializeComponent();
        }

        private TimeSpan GetDuration()
        {
            return TimeSpan.FromMinutes(paintPanel.Width / MinutesPerPixel);
        }

        private void PaintButton_Click(object sender, EventArgs e)
        {
            paintPanel.Invalidate();
        }
        
        private void PaintTimeline(List<VizNode> vizNodes, Graphics graphics)
        {
            var xoffset = _legendWidth;
            var width = graphics.VisibleClipBounds.Width;

            var yoffset = 0;
            foreach (var vizNode in vizNodes)
            {
                yoffset = PaintNode(vizNode, null, yoffset, xoffset, false);
            }

            var height = yoffset + 25;
            var g = graphics;
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);

            PaintTime(graphics, yoffset, xoffset, true);

            yoffset = 0;
            foreach (var vizNode in vizNodes)
            {
                yoffset = PaintNode(vizNode, graphics, yoffset, xoffset, true);
            }
        }

        private int PaintNode(VizNode node, Graphics graphics, int offset, int xoffset, bool paint)
        {
            var data = node.VizData.GroupBy(v => v.Operation);
            var endOffset = offset;

            foreach (var d in data)
            {
                PaintLine(graphics, endOffset, 80, paint, d.First().OperationName, Color.LightGray);
                var newOffset = PaintOperation(d.ToList(), graphics, endOffset, xoffset, paint);
                endOffset += newOffset < 20 ? 20 : newOffset;
            }
            if (paint) PaintLine(graphics, offset, 0, true, node.Name, Color.Black);

            return endOffset - offset < 20 ? 20 + offset : endOffset;
        }

        private int PaintOperation(List<VizData> vizData, Graphics graphics, int yoffset, int xoffset, bool paint)
        {
            var g = graphics;
            var lineLengths = new List<float>();

            foreach (var data in vizData)
            {
                var i = 0;
                var lineXoffset = (float)((data.Start - WindowsStart).TotalMinutes / MinutesPerPixel) + xoffset;

                foreach (var len in lineLengths)
                {
                    if (len - 2 < lineXoffset) break; //чтобы наложенные при округлении прямоугольники располагались друг за другом ( 2 = 1(наложение) + 1(растяжение коротких промежутков)
                    i++;
                }
                if (i >= lineLengths.Count) lineLengths.Add(0);

                var x = lineXoffset;
                var y = yoffset + i * _brushHeight;
                var width = (float)(data.Duration.TotalMinutes / MinutesPerPixel);
                width = width < 1 ? 1 : width;
                var height = _brushHeight;
                lineLengths[i] = x + width;

                if (paint && data.Start + data.Duration >= WindowsStart && data.Start < WindowsStart + WindowDuration)
                {
                    if (x < _legendWidth)
                    {
                        var correct = _legendWidth - x;
                        x += correct;
                        width -= correct;
                    }
                    g.FillRectangle(data.Brush, x, y, width, height);
                }
            }

            return lineLengths.Count * _brushHeight;
        }


        private void PaintLine(Graphics graphics, int yoffset, int xoffset, bool paint, string text, Color lineColor)
        {
            if (!paint) return;

            var g = graphics;
            g.DrawLine(new Pen(lineColor, 1), xoffset, yoffset, g.VisibleClipBounds.Width, yoffset);
            g.DrawString(text, new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.Black), xoffset, yoffset);
        }

        private void PaintTime(Graphics graphics, int yoffset, int xoffset, bool paint)
        {
            if (!paint) return;

            var y = yoffset;
            var g = graphics;
            var step = 100;
            var pixelSkip = (100 - ((int)(WindowsStart.TotalMinutes / MinutesPerPixel) % step));
            if (Math.Abs(pixelSkip - 100) < 0.0001) pixelSkip = 0;
            var localxoffset = xoffset + pixelSkip;

            g.DrawLine(new Pen(Color.Black, 1), xoffset, y, g.VisibleClipBounds.Width, y);
            var textWidth = 85;

            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            for (var i = localxoffset; i < g.VisibleClipBounds.Width; i += step)
            {
                g.DrawLine(new Pen(Color.LightGray, 1), i, 0, i, y + 2);
                g.DrawLine(new Pen(Color.Black, 1), i, y - 2, i, y + 2);
                g.DrawString(
                    RoundTimeSpan(WindowsStart + TimeSpan.FromMinutes((i - localxoffset + pixelSkip) * MinutesPerPixel))
                    .ToString(@"hh\:mm\:ss"), new Font(FontFamily.GenericMonospace, 10), new SolidBrush(Color.Black),
                    new RectangleF(i - textWidth / 2f, y + 3, textWidth, 20), stringFormat);
            }
        }

        private void VizView_Load(object sender, EventArgs e)
        {
            MinutesPerPixel = DefaultZoom;
            zoomTrackBar.Value = 6;
            hScrollBar1.Maximum = (int)(_visualizer.Duration.TotalMinutes / MinutesPerPixel) / 100;
        }

        private void paintPanel_Paint(object sender, PaintEventArgs e)
        {
            PaintTimeline(_visualizer.VizNodes, paintPanel.CreateGraphics());
        }

        private void zoomTrackBar_Scroll(object sender, EventArgs e)
        {
            var trackBar = (TrackBar) sender;
            var coef = DefaultZoom * trackBar.Value/6;
            scaleLabel.Text = $@"{1.0f/((float)trackBar.Value/6):F2}X";
            MinutesPerPixel = coef;
            hScrollBar1.Maximum = (int)(_duration.TotalMinutes / MinutesPerPixel * DefaultZoom);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.Type != ScrollEventType.EndScroll) return;

            var scrollBar = (HScrollBar) sender;
            var step = _duration.TotalMinutes / scrollBar.Maximum;

            WindowsStart = RoundTimeSpan(TimeSpan.FromMinutes(step * scrollBar.Value));
        }

        private TimeSpan RoundTimeSpan(TimeSpan timeSpan, int presition = 0)
        {
            var factor = (int)Math.Pow(10, 7 - presition);
            return new TimeSpan(
                ((long)Math.Round((1.0 * timeSpan.Ticks / factor)) * factor));
        }

        private void zoomTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            paintPanel.Invalidate();
        }
    }
}
