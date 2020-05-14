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
using PreTran.DataBaseSchemeStructure;
using PreTran.Q_Part_Structures;

namespace PreTran.Q_Structures
{
    class SortStructure
    {
        private string _name;
        private string _fromName;
        private string _output;
        private string _createTableColumnNames;

        private BinaryComparisionPredicateStructure _connectBinary;
        private SelectStructure _subSelect;
        private JoinStructure _subJoin;
        private JoinStructure _notFilledJoin;
        private string _selectString;

        private SelectStructure _select;
        private JoinStructure _join;
        private TableStructure _outTable;
        private List<string> _groupByColumnList;
        private List<AsStructure> _asSortList;
        private List<OrderByStructure> _orderByStructures;

        public SortStructure(string name)
        {
            _name = name;
        }

        public List<AsStructure> AsSortList
        {
            set { _asSortList = value; }
        }

        public List<string> GroupByColumnList
        {
            set { _groupByColumnList = value; }
        }
        
        public List<OrderByStructure> OrderByStructures
        {
            set { _orderByStructures = value; }
        }

        public JoinStructure Join
        {
            set { _join = value; }
        }

        public SelectStructure Select
        {
            set { _select = value; }
        }

        public TableStructure OutTable
        {
            get { return _outTable; }
        }

        public string Output
        {
            get { return _output; }
        }

        public string Name
        {
            get { return _name; }
        }
        
        public string CreateTableColumnNames
        {
            get { return _createTableColumnNames; }
        }

        public JoinStructure NotFilledJoin
        {
            get { return _notFilledJoin; }
            set { _notFilledJoin = value; }
        }

        public JoinStructure SubJoin
        {
            get { return _subJoin; }
            set { _subJoin = value; }
        }

        public SelectStructure SubSelect
        {
            get { return _subSelect; }
            set { _subSelect = value; }
        }

        public BinaryComparisionPredicateStructure ConnectBinary
        {
            get { return _connectBinary; }
            set { _connectBinary = value; }
        }

        public string SelectString
        {
            get { return _selectString; }
            set { _selectString = value; }
        }


        public void CreateQuerry()
        {
            _output = "SELECT \r\n\t";
            List<ColumnStructure> tmpSelectColumns = new List<ColumnStructure>();
            if (_join != null)
            {
                _fromName = _join.Name;
                foreach (ColumnStructure column in _join.Columns)
                {
                    if (column.IsForSelect)
                    {
                        tmpSelectColumns.Add(column);
                    }
                }
            }
            else
            {
                _fromName = _select.Name;
                foreach (ColumnStructure column in _select.OutColumn)
                {
                    if (column.IsForSelect)
                    {
                        tmpSelectColumns.Add(column);
                    }
                }
            }

            foreach (ColumnStructure column in tmpSelectColumns)
            {
                if (column != tmpSelectColumns.Last())
                {
                    _output += column.Name + ",\r\n\t";
                }
                else
                {
                    _output += column.Name + "\r\n\t";
                }
            }

            if (_asSortList.Count != 0 && tmpSelectColumns.Count!=0)
            {
                _output = _output.Insert(_output.Length - 3, ",");
            }

            foreach (AsStructure asStructure in _asSortList)
            {
                if (asStructure.AsRightColumn.IsRenamed)
                {
                    string tmpHolder = asStructure.AsRightColumn.Name;
                    asStructure.AsRightColumn.Name = asStructure.AsRightColumn.OldName;
                    asStructure.AsRightColumn.IsRenamed = false;
                    asStructure.AsRightColumn.OldName = tmpHolder;
                }
                if(asStructure.AsRightColumn.OldName!=null)
                {
                    _output += asStructure.AggregateFunctionName + "(" + asStructure.AsRightColumn.OldName + ")" + " AS " +
                           asStructure.AsRightColumn.Name;
                }
                else
                {
                    _output += asStructure.AggregateFunctionName + "(" + asStructure.AsString + ")" + " AS " +
                               asStructure.AsRightColumn.Name;
                }
                tmpSelectColumns.Add(asStructure.AsRightColumn);
                if (asStructure != _asSortList.Last())
                {
                    _output += ",\r\n\t";
                }
                else
                {
                    _output += "\r\n\t";
                }
            }

            _outTable = new TableStructure(_name+"_TB", tmpSelectColumns.ToArray());
            
            _output = _output.Remove(_output.Length - 1, 1);
            _output += "FROM\r\n\t" + _fromName + "\r\n";

            if (_connectBinary != null)
            {
                _output += "WHERE" + Environment.NewLine;
                if (_subJoin!=null)
                {
                    _output += GetSubQueryString(_subJoin.Name);
                }
                else
                {
                    _output += GetSubQueryString(_subSelect.Name);
                }
                
            }

            
            if (_groupByColumnList.Count != 0)
            {
                _output += "GROUP BY\r\n\t";
            }

            foreach (string column in _groupByColumnList)
            {
                _output += column;
                if (column != _groupByColumnList.Last())
                {
                    _output += ",\r\n\t";
                }
                else
                {
                    _output += "\r\n";
                }
            }

            if(_orderByStructures.Count!=0)
            {
                _output +=Environment.NewLine + "ORDER BY\r\n\t";
            }

            foreach (OrderByStructure orderBy in _orderByStructures)
            {
                if (!orderBy.Column.IsRenamed)
                {
                    _output += orderBy.Column.Name;
                }
                else
                {
                    _output += orderBy.Column.OldName;
                }

                if (orderBy.IsDESC)
                {
                    _output += " DESC";
                }

                if (orderBy != _orderByStructures.Last())
                {
                    _output += ",\r\n\t";
                }
                else
                {
                    _output += "\r\n";
                }

                
            }
            SetCreateTableColumnList();
            SetIndexes();
            _output += ";";
        }

        public void CreateQuerry(string tag)
        {
            _output = "SELECT \r\n\t";
            List<ColumnStructure> tmpSelectColumns = new List<ColumnStructure>();
            if (_join != null)
            {
                //_fromName = _join.Name;
                foreach (ColumnStructure column in _join.Columns)
                {
                    if (column.IsForSelect)
                    {
                        tmpSelectColumns.Add(column);
                    }
                }
            }
            else
            {
                //_fromName = _select.Name;
                foreach (ColumnStructure column in _select.OutColumn)
                {
                    if (column.IsForSelect)
                    {
                        tmpSelectColumns.Add(column);
                    }
                }
            }

            _fromName = tag;
            foreach (ColumnStructure column in tmpSelectColumns)
            {
                if (column != tmpSelectColumns.Last())
                {
                    _output += column.Name + ",\r\n\t";
                }
                else
                {
                    _output += column.Name + "\r\n\t";
                }
            }

            if (_asSortList.Count != 0 && tmpSelectColumns.Count!=0)
            {
                _output = _output.Insert(_output.Length - 3, ",");
            }

            foreach (AsStructure asStructure in _asSortList)
            {
                if (asStructure.AsRightColumn.IsRenamed)
                {
                    string tmpHolder = asStructure.AsRightColumn.Name;
                    asStructure.AsRightColumn.Name = asStructure.AsRightColumn.OldName;
                    asStructure.AsRightColumn.IsRenamed = false;
                    asStructure.AsRightColumn.OldName = tmpHolder;
                }
                if(asStructure.AsRightColumn.OldName!=null)
                {
                    _output += asStructure.AggregateFunctionName + "(" + asStructure.AsRightColumn.OldName + ")" + " AS " +
                           asStructure.AsRightColumn.Name;
                }
                else
                {
                    _output += asStructure.AggregateFunctionName + "(" + asStructure.AsString + ")" + " AS " +
                               asStructure.AsRightColumn.Name;
                }
                tmpSelectColumns.Add(asStructure.AsRightColumn);
                if (asStructure != _asSortList.Last())
                {
                    _output += ",\r\n\t";
                }
                else
                {
                    _output += "\r\n\t";
                }
            }

            _outTable = new TableStructure(_name+"_TB", tmpSelectColumns.ToArray());
            
            _output = _output.Remove(_output.Length - 1, 1);
            _output += "FROM\r\n\t" + _fromName + "\r\n";

            if (_connectBinary != null)
            {
                _output += "WHERE" + Environment.NewLine;
                _output += GetSubQueryString(tag);
            }

            if (_groupByColumnList.Count != 0)
            {
                _output += "GROUP BY\r\n\t";
            }

            foreach (string column in _groupByColumnList)
            {
                _output += column;
                if (column != _groupByColumnList.Last())
                {
                    _output += ",\r\n\t";
                }
                else
                {
                    _output += "\r\n";
                }
            }

            if(_orderByStructures.Count!=0)
            {
                _output +=Environment.NewLine + "ORDER BY\r\n\t";
            }

            foreach (OrderByStructure orderBy in _orderByStructures)
            {
                if (!orderBy.Column.IsRenamed)
                {
                    _output += orderBy.Column.Name;
                }
                else
                {
                    _output += orderBy.Column.OldName;
                }

                if (orderBy.IsDESC)
                {
                    _output += " DESC";
                }

                if (orderBy != _orderByStructures.Last())
                {
                    _output += ",\r\n\t";
                }
                else
                {
                    _output += "\r\n";
                }

                
            }
            SetCreateTableColumnList();
            SetIndexes();
            _output += ";";
        }

        private void SetCreateTableColumnList()
        {
            for (int i = 0; i < _outTable.Columns.Length; i++)
            {
                if (_outTable.Columns[i].Type != null)
                {
                    _createTableColumnNames += _outTable.Columns[i].Name + " " + _outTable.Columns[i].Type.Name;
                }
                else
                {
                    _createTableColumnNames += _outTable.Columns[i].Name + " " + "INTEGER";
                }

                if (i < _outTable.Columns.Length - 1)
                {
                    _createTableColumnNames += ",\r\n";
                }
            }
        }

        private string GetSubQueryString(string from)
        {
            string subQOutput = Environment.NewLine;
            subQOutput += "\t";
            subQOutput += _connectBinary.LeftString + " " + _connectBinary.ComparisionSymphol + " ( SELECT" +
                          Environment.NewLine;
            subQOutput += "\t" + _selectString;
            subQOutput +=Environment.NewLine + "FROM " + Environment.NewLine;
            
                subQOutput += "\t" + from + " AS SUB";
            

            if (_notFilledJoin!=null)
            {
                subQOutput += Environment.NewLine + "WHERE" + Environment.NewLine;
                subQOutput +="\t" + _notFilledJoin.LeftColumnString + " = " + "SUB." +  _notFilledJoin.RightColumnString;
            }

            subQOutput += " )";
            return subQOutput;
        }

        private void SetIndexes()
        {
            if (_join != null)
            {
                if (_notFilledJoin != null)
                {
                    foreach (ColumnStructure column in _join.Columns)
                    {
                        if (_notFilledJoin.LeftColumnString == column.Name)
                        {
                            _join.IndexColumnName = column.Name;
                        }
                        else if(_notFilledJoin.RightColumnString == column.Name)
                        {
                            _join.IndexColumnName = column.Name;
                        }
                    }
                }
                else
                {
                    foreach (ColumnStructure column in _join.Columns)
                    {
                        if (column.IsPrimary == 1)
                        {
                            _join.IndexColumnName = column.Name;
                        }
                    }
                }
            }

            if (_subJoin != null)
            {
                if (_notFilledJoin != null)
                {
                    foreach (ColumnStructure column in _subJoin.Columns)
                    {
                        if (_notFilledJoin.LeftColumnString == column.Name)
                        {
                            _subJoin.IndexColumnName = column.Name;
                        }
                        else if(_notFilledJoin.RightColumnString == column.Name)
                        {
                            _subJoin.IndexColumnName = column.Name;
                        }
                    }
                }
                else
                {
                    foreach (ColumnStructure column in _subJoin.Columns)
                    {
                        if (column.IsPrimary == 1)
                        {
                            _subJoin.IndexColumnName = column.Name;
                        }
                    }
                }
            }

        }
    }
}
