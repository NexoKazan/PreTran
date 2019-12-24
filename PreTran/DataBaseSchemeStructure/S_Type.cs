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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MySQL_Clear_standart
{
    public class S_Type
    {
        private string _name;
        private int _size;
        private string _ID;

        public S_Type(){}

        public S_Type(string name, int size, string ID)
        {
            _name = name;
            _size = size;
            _ID = ID;
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
            get { return _size; }
            set { _size = value; }
        }

        [XmlAttribute]
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
    }
}
