#region Copyright
/*
 * Copyright 2019 Igor Kazantsev
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
using System.Drawing.Drawing2D;
using System.Linq;

namespace PreTran.Visual
{
    class TreeVisitor
    {  
        private static Font NodeTextFont = new Font("Verdana", 8f);
        private static SizeF MinimumNodeSize = new SizeF(32, 28);
        private static Size NodeGapping = new Size(4, 32);
        private static Dictionary<string, Pen> Pens = new Dictionary<string, Pen>();

        private List<string> _nodePaths = new List<string>();
        private List<string> _tablesNames = new List<string>();
        private List<string> _columnNames = new List<string>();
        private List<string> _compairColumnNames = new List<string>();
        private List<ICommonNode> _terminalNodes = new List<ICommonNode>();

        private static bool _tablesPointer = false;
        private bool _columnNamePointer = false;
        private bool _compairPointer = false;

        public ICommonNode CommonNode { get; private set; }

        public TreeVisitor(ICommonNode node)
        {
            CommonNode = node;
        }

        public List<ICommonNode> TerminalNodes
        {
            get
            {
                return _terminalNodes;
            }
        }
        public List<string> CompairColumnNames
        {
            get
            {
                return _compairColumnNames;
            }
        }
        public List<string> ColumnNames
        {
            get
            {
                return _columnNames;
            }
        }
        public List<string> NodePaths
        {
            get
            {
                return _nodePaths;
            }
        }
        public List<string> TablesNames
        {
            get
            {
                return _tablesNames;
            }
        }
        public void VisitTree (ICommonNode node)
        {                             
            var enumerator = node.Children.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {                
                var currentNode = enumerator.Current;
                _nodePaths.Add(currentNode.BranchText);
                ++i;
            }

            return;
        }
        public void GetColumnNames(ICommonNode node)
        {            
            if (node.Text == "FullColumnName")
            {
                _columnNamePointer = true;
            }
            if(node.Type == "Leaf" && _columnNamePointer)
            {
                _columnNames.Add(node.Text);
                _columnNamePointer = false;
            }
            var enumerator = node.Children.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                var currentNode = enumerator.Current;
                GetColumnNames(currentNode);
                ++i;
            }
            return;
        }
        public void GetCompairColumnNames(ICommonNode node)
        {
            if (node.Text == "BinaryComparasionPredicate")
            {
                _compairPointer = true;
            }
            if (node.Text == "FullColumnName")
            {
                _columnNamePointer = true;
            }
            if (node.Type == "Leaf" && _columnNamePointer && _compairPointer)
            {
                _compairColumnNames.Add(node.Text);
                _columnNamePointer = false;
                _compairPointer = false;
            }
            var enumerator = node.Children.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                var currentNode = enumerator.Current;
                GetCompairColumnNames(currentNode);
                ++i;
            }
            return;
        }
        public List<string> GetTablesName(ICommonNode node)
        {
            if (node.Text.Contains("TableName"))
            {
                _tablesPointer = true;
            }
            if (node.Count == 0 && _tablesPointer)
            {
                _tablesNames.Add(node.Text);
                _tablesPointer = false;
            }
            var enumerator = node.Children.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                var currentNode = enumerator.Current;                
                GetTablesName(currentNode);
                ++i;
            }
            return _tablesNames;
        }
        public List<ICommonNode> GetTerminalNodes(ICommonNode node)
        {
            if (node.Type == "Leaf")
            {
                _terminalNodes.Add(node);
            }
            
            var enumerator = node.Children.GetEnumerator();
            int i = 0;
            while (enumerator.MoveNext())
            {
                var currentNode = enumerator.Current;
                GetTerminalNodes(currentNode);    
                ++i;
            }
            return _terminalNodes;
        }

        

        #region Drawing
        private Image Draw(ICommonNode node, out int center)
        {             
            var nodeText = node.Text;
            var nodeSize = TextMeasurer.MeasureString("*" + nodeText + "*", NodeTextFont);
            nodeSize.Width = Math.Max(MinimumNodeSize.Width, nodeSize.Width);
            nodeSize.Height = Math.Max(MinimumNodeSize.Height, nodeSize.Height);

            var childCentres = new int[node.Count];
            var childImages = new Image[node.Count];
            var childSizes = new Size[node.Count];

            var enumerator = node.Children.GetEnumerator(); 
            int i = 0;
            while (enumerator.MoveNext())
            {               
                var currentNode = enumerator.Current;
                var lCenter = 0;
                childImages[i] = Draw(currentNode, out lCenter);
                childCentres[i] = lCenter;
                if (childImages[i] != null)
                {
                    childSizes[i] = childImages[i] != null ? childImages[i].Size : new Size();
                }
                ++i;                
            }

            // draw current node and it's children
            var under = childImages.Any(nodeImg => nodeImg != null);// if true the current node has childs
            var maxHeight = node.Count > 0 ? childSizes.Max(c => c.Height) : 0;
            var totalFreeWidth = node.Count > 0 ? NodeGapping.Width * (node.Count - 1) : NodeGapping.Width;
            var totalChildWidth = childSizes.Sum(s => s.Width); 
                                            
            var nodeImage = CreateNodeImage(nodeSize.ToSize(), nodeText, NodeTextFont);
       
            var totalSize = new Size
            {
                Width = Math.Max(nodeImage.Size.Width, totalChildWidth) + totalFreeWidth,
                Height = nodeImage.Size.Height + (under ? maxHeight + NodeGapping.Height : 0)
            };

            var result = new Bitmap(totalSize.Width, totalSize.Height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), totalSize));

            var left = (totalSize.Width - nodeImage.Width) / 2;
            g.DrawImage(nodeImage, left, 0);
            
            center = Math.Max(totalSize.Width / 2, (nodeImage.Width + NodeGapping.Width) / 2);

            var fromLeft = 0;

            for (int j = 0; j < node.Count; ++j)
            {
                float x1 = center;
                float y1 = nodeImage.Height;
                float y2 = nodeImage.Height + NodeGapping.Height;
                float x2 = fromLeft + childCentres[j];
                var h = y2 - y1;
                var w = x1 - x2;
                var childImg = childImages[j];
                if (childImg != null)
                {
                    g.DrawImage(childImg, fromLeft, nodeImage.Size.Height + NodeGapping.Height);
                    fromLeft += childImg.Width + NodeGapping.Width; // Prepare next child left starting point 
                    var points1 = new List<PointF>
                                  {
                                      new PointF(x1, y1),
                                      new PointF(x1 - w/6, y1 + h/3.5f),
                                      new PointF(x2 + w/6, y2 - h/3.5f),
                                      new PointF(x2, y2),
                                  };
                    g.DrawCurve(ConnectionPen, points1.ToArray(), 0.5f);
                }

                childImages[j].Dispose(); // Release child image as it aleady drawn on parent node's surface 
            }

            g.Dispose();

            return result;
        }
        private static Bitmap CreateNodeImage(Size size, string text, Font font)
        {
            Bitmap img = new Bitmap(size.Width, size.Height);
            using (var g = Graphics.FromImage(img))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                var rcl = new Rectangle(1, 1, img.Width - 2, img.Height - 2);
                g.FillRectangle(Brushes.White, rcl);

                LinearGradientBrush linearBrush = new LinearGradientBrush(rcl, Color.LightBlue, Color.White, LinearGradientMode.ForwardDiagonal);
                g.DrawEllipse(NodeBorderPen, rcl);
                g.FillEllipse(linearBrush, rcl);
                linearBrush.Dispose();

                var sizeText = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.Black, Math.Max(0, (size.Width - sizeText.Width) / 2), Math.Max(0, (size.Height - sizeText.Height) / 2));
            }                 
            return img;
        }
        private static Pen ConnectionPen
        {
            get
            {
                string penName = "ConnectionPen";
                if (!Pens.ContainsKey(penName))
                {
                    Pens.Add(penName, new Pen(Brushes.Black, 1) { EndCap = LineCap.ArrowAnchor, StartCap = LineCap.Round });
                }
                return Pens[penName];
            }
        }

        private static Pen NodeBorderPen
        {
            get
            {
                string penName = "NodeBorderPen";
                if (!Pens.ContainsKey(penName))
                {
                    Pens.Add(penName, new Pen(Color.Silver, 1));
                }
                return Pens[penName];
            }
        }

        public Image Draw()
        {
            int center;
            Image image = Draw(this.CommonNode, out center);
            return image;
        }
        #endregion
    }
}
