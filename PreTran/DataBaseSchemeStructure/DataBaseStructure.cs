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

namespace PreTran.DataBaseSchemeStructure
{
    
    public class DataBaseStructure
    {
        private TableStructure[] _tables;
        private S_Type[] _types;
        private string _name;

        public DataBaseStructure()
        { }

        public DataBaseStructure(string name, TableStructure[] tables)
        {
            _tables = tables;
            _name = name;
        }

        public DataBaseStructure(string name, TableStructure[] tables, S_Type[] types)
        {
            _tables = tables;
            _name = name;
            _types = types;
        }

        [XmlArray]
        public TableStructure[] Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        [XmlArray]
        public S_Type[] Types
        {
            get { return _types; }
            set { _types = value; }
        }
        [XmlAttribute]
        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string GetText()
        {
            string output = _name + "\r\n";
            for (int i = 0; i < _tables.Length; i++)
            {
                output += _tables[i].Name + "\r\n";
                for (int j = 0; j < _tables[i].Columns.Length; j++)
                {
                    output +="\t" + _tables[i].Columns[j].Name + "\r\n";
                }
            }

            return output;
        }
    }
}
