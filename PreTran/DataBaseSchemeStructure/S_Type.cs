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
using System.Windows.Forms;
using System.Xml.Serialization;

//про Decimal
//https://dev.mysql.com/doc/refman/8.0/en/precision-math-decimal-characteristics.html

namespace PreTran.DataBaseSchemeStructure
{
    public class S_Type
    {
        private string _name;
        private int _size;
        private string _ID;
        private int _param1 = -1;
        private int _param2 = -1;

        public S_Type(){}

        public S_Type(string name, int size, string ID)
        {
            _name = name;
            _size = size;
            _ID = ID;
            SetParams();
        }

        public S_Type(string name, int size, string ID, int param1, int param2)
        {
            _name = name;
            _size = size;
            _ID = ID;
            _param1 = param1;
            _param2 = param2;
            SetDecimalSize();
        }

        [XmlAttribute]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [XmlAttribute]
        public int Size
        {
            get
            {
                SetDecimalSize();
                return _size;
            }
            set { _size = value; }
        }

        [XmlAttribute]
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        /// <summary>
        /// знаков всего в DECIMAL
        /// </summary>
        [XmlIgnore]
        public int Param1
        {
           
            get
            {
                SetParams();
                return _param1;
            }
            set { _param1 = value; }
        }
        /// <summary>
        /// знаков после запятой в DECIMAL
        /// </summary>
        [XmlIgnore]
        public int Param2
        {
            
            get
            {
                SetParams();
                return _param2;
            }
            set { _param2 = value; }
        }

        private void SetParams()
        {
            if (_name != null && _name.ToLower().Contains("decimal"))
            {
                int left;
                int right;
                bool open = false;
                bool close = false;
                bool divide = false;
                string leftS = "";
                string rightS = "";
                foreach (char c in _name)
                {
                    if (c == ')')
                    {
                        open = false;
                        divide = false;
                    }

                    if (c == ',')
                    {
                        divide = true;
                        continue;
                    }

                    if (open && !divide)
                    {
                        leftS += c.ToString();
                    }

                    if (open && divide)
                    {
                        rightS += c.ToString();
                    }

                    if (c == '(')
                    {
                        open = true;
                    }
                }

                left = Convert.ToInt32(leftS);
                _param1 = left;
                right = Convert.ToInt32(rightS);
                _param2 = right;
            }
            else
            {
                _param1 = -1;
                _param2 = -1;
            }
        }

        private  void SetDecimalSize()
        {
            if (_name != null &&  _name.ToLower().Contains("decimal"))
            {
                int left = Param1 - Param2;
                int right = Param2;
                int bits = 4;
                int size = 0;
                for (int i = 9; i > 0; i--)
                {
                    if (i > 4 && i < 7)
                    {
                        bits = 3;
                    }
                    if (i > 2 && i < 5)
                    {
                        bits = 2;
                    }

                    if (i > 0 && i < 3)
                    {
                        bits = 1;
                    }

                    if (left / i >= 0)
                    {
                        size = size + (left / i) * bits;
                        left = left % i;
                    }
                    if (right / i >= 0)
                    {
                        size = size + (right / i) * bits;
                        right = right % i;
                    }

                }

                _size = size;
            }
        }
    }
}
