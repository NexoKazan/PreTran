﻿#region Copyright
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

using System.Xml.Serialization;
using Antlr4.Runtime.Misc;

namespace PreTran.DataBaseSchemeStructure
{
    
    public class TableStructure
    {
        private ColumnStructure[] _columns;
        private string _name;
        private string _shortName;
        private string _dotedID;
        private int _rowSize;
        private int _rowCount;
        private Interval _sourceInterval;

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

        public TableStructure(string name, Interval sourceInterval)
        {
            _name = name;
            _sourceInterval = sourceInterval;
        }

        public TableStructure(TableStructure mainTable)
        {
            _columns = mainTable.Columns;
            _name = mainTable.Name;
            _shortName = mainTable.ShortName;
            _sourceInterval = mainTable.SourceInterval;
            _dotedID = mainTable.DotedId;
            _rowCount = mainTable.RowCount;
            _rowSize = mainTable.RowSize;
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

        [XmlAttribute]
        public int RowCount
        {
            get { return _rowCount; }
            set { _rowCount = value; }

        }

        [XmlIgnore]
        public int RowCountRight
        {
            get;
            set;

        }

        [XmlIgnore]
        public int RowCountLeft
        {
            get;
            set;

        }

        [XmlIgnore]
        public Interval SourceInterval
        {
            get => _sourceInterval;
            set => _sourceInterval = value;
        }

        [XmlIgnore]
        public string DotedId
        {
            get => _dotedID;
            set => _dotedID = value;
        }

        [XmlIgnore]
        public int RowSize
        {
            get
            {
                _rowSize = 0;
                foreach (ColumnStructure column in _columns)
                {
                    _rowSize += column.Size;
                }
                return _rowSize;
            }
        }
    }
}
