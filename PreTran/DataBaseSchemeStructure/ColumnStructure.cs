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

using System.Xml.Serialization;
using Antlr4.Runtime.Misc;

namespace PreTran.DataBaseSchemeStructure
{

    public class ColumnStructure
    {
        private string _name;
        private string _typeID;
        private string _oldName;
        private int _size;
        private int _isPrimary; // 0-не ключ 1-единичный ключ, 2-составной ключ, 3-другое
        private int _usageCounter;
        private bool _isForSelect = false;
        private bool _isRenamed = false;
        private S_Type _type;
        private TableStructure _table;
        private Interval _sourceInterval;

        public ColumnStructure()
        {
        }

        public ColumnStructure(string name, string typeID, int isPrimary)
        {
            _isPrimary = isPrimary;
            _name = name;
            _typeID = typeID;
        }

        public ColumnStructure(string name, string typeID)
        {
            _isPrimary = 0;
            _name = name;
            _typeID = typeID;
        }

        public ColumnStructure(string name)
        {
            _name = name;
        }

        public ColumnStructure(string name, Interval sourceInterval)
        {
            _name = name;
            _sourceInterval = sourceInterval;
        }

        public ColumnStructure(ColumnStructure inColumn)
        {
            _name = inColumn.Name;
            _typeID = inColumn.TypeID;
            _oldName = inColumn.OldName;
            _size = inColumn.Size;
            _isPrimary = inColumn.IsPrimary;
            _usageCounter = inColumn.UsageCounter;
            _isForSelect = inColumn.IsForSelect;
            _isRenamed = inColumn.IsRenamed;
            _type = inColumn.Type;
            _table = inColumn.Table;
            _sourceInterval = inColumn.SoureInterval;
        }

        [XmlAttribute]
        public int IsPrimary
        {
            get { return _isPrimary; }
            set { _isPrimary = value; }
        }

        [XmlAttribute]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [XmlAttribute]
        public string TypeID
        {
            get { return _typeID; }
            set { _typeID = value; }
        }

        [XmlIgnore]
        public S_Type Type
        {
            get { return _type; }
            set { _type = value; }
        }

        [XmlIgnore]
        public bool IsForSelect
        {
            get { return _isForSelect; }
            set { _isForSelect = value; }
        }

        [XmlIgnore]
        public bool IsRenamed
        {
            get { return _isRenamed; }
            set { _isRenamed = value; }
        }

        [XmlIgnore]
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        [XmlIgnore]
        public string OldName
        {
            get { return _oldName; }
            set { _oldName = value; }
        }

        [XmlIgnore]
        public int UsageCounter
        {
            get { return _usageCounter; }
            set { _usageCounter = value; }
        }

        [XmlIgnore]
        public TableStructure Table
        {
            get { return _table; }
            set { _table = value; }

        }

        [XmlIgnore]
        public Interval SoureInterval
        {
            get { return _sourceInterval; }
            set { _sourceInterval = value; }
        }
    }
}
