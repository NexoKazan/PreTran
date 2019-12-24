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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MySQL_Clear_standart.DataBaseSchemeStructure
{
    
    public class TableStructure
    {
        private ColumnStructure[] _columns;
        private string _name;
        private string _shortName;

        public TableStructure() { }

        public TableStructure(string name, ColumnStructure[] columns)
        {
            _name = name;
            _columns = columns;
            foreach (ColumnStructure column in _columns)
            {
                column.Table = this;
            }
        }

        public TableStructure(TableStructure mainTable)
        {
            _columns = mainTable.Columns;
            _name = mainTable.Name;
            _shortName = mainTable.ShortName;
        }

        [XmlArray]
        public ColumnStructure[] Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        [XmlAttribute]
        public string Name
        {
            get { return _name; }
            set { _name = value; }

        }

        [XmlAttribute]
        public string ShortName
        {
            get { return _shortName; }
            set { _shortName = value; }
        }
    }
}
