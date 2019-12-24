#region Copyright
/*
 * Copyright 2018 Roman Klassen
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace LogProcessingTool.Visualizer
{
    public class GraphConCat
    {
        private readonly string _operationImagePath;
        private readonly string _cpuImagePath;
        private readonly string _ramImagePath;
        private readonly string _netImagePath;

        private readonly List<Tuple<int, Image[]>> _images;

        public GraphConCat(string operationImagePath, string cpuImagePath = "", string ramImagePath = "", string netImagePath = "")
        {
            _operationImagePath = operationImagePath;
            _cpuImagePath = cpuImagePath;
            _ramImagePath = ramImagePath;
            _netImagePath = netImagePath;

            _images = new List<Tuple<int, Image[]>>();
        }

        private Image[] SplitImage(Image image)
        {
            var heights = new List<int>() {0};
            using (var bmp = new Bitmap(image))
            {
                for (int i = 3; i < bmp.Height; i++)
                {
                    if (bmp.GetPixel(0,i).R < 100) heights.Add(i);
                }
            }
            heights.Add(image.Height);

            Image[] images = new Image[heights.Count-1];
            for (int i = 0; i < heights.Count-1; i++)
            {
                images[i] = new Bitmap(image.Width, heights[i + 1] - heights[i]);
                using (var g = Graphics.FromImage(images[i]))
                {
                    var sourceRect = new Rectangle(0, heights[i], images[i].Width, images[i].Height);
                    var destRect = new Rectangle(0, 0, images[i].Width, images[i].Height);
                    g.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
                }
            }

            return images;
        }

        private void Load()
        {
            var maxPadding = 340;
            var operationImage = Image.FromFile(_operationImagePath);
            _images.Add(new Tuple<int, Image[]>(maxPadding - 340, SplitImage(operationImage)));

            if (!string.IsNullOrWhiteSpace(_cpuImagePath))
            {
                var cpuImage = Image.FromFile(_cpuImagePath);
                _images.Add(new Tuple<int, Image[]>(maxPadding - 150, SplitImage(cpuImage)));
            }

            if (!string.IsNullOrWhiteSpace(_ramImagePath))
            {
                var ramImage = Image.FromFile(_ramImagePath);
                _images.Add(new Tuple<int, Image[]>(maxPadding - 150, SplitImage(ramImage)));
            }

            if (!string.IsNullOrWhiteSpace(_netImagePath))
            {
                var netImage = Image.FromFile(_netImagePath);
                _images.Add(new Tuple<int, Image[]>(maxPadding - 150, SplitImage(netImage)));
            }
        }
        
        private Image Concat()
        {
            var image = new Bitmap(_images.Max(i=>i.Item2.Max(img=> img.Width)), _images.Sum(i => i.Item2.Sum(img=>img.Height)));
            var g = Graphics.FromImage(image);
            var yoffset = 0;

            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, image.Width, image.Height));

            for (int i = 0; i < _images[0].Item2.Length; i++)
            {
                foreach (var tuple in _images)
                {
                    g.DrawImage(tuple.Item2[i], new Point(tuple.Item1, yoffset));
                    yoffset += tuple.Item2[i].Height;
                }
            }


            return image;
        }

        public void Concat(string fileName)
        {
            ConsoleHelper.ReWriteLine("Загрузка изображений...");
            Load();
            ConsoleHelper.ReWriteLine("Загрузка изображений... готово.", true);

            ConsoleHelper.ReWriteLine("Объединение изображений...");
            var image = Concat();
            image.Save(fileName, ImageFormat.Png);
            ConsoleHelper.ReWriteLine("Объединение изображений... готово.", true);
        }
    }
}
