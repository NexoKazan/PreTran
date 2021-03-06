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

using System;
using System.Collections.Generic;
using System.Linq;
using PreTran.DataBaseSchemeStructure;
using PreTran.Q_Part_Structures;

namespace PreTran.Q_Structures
{
    class SelectStructure
    {
        private string _name;
        private string _output = "error";
        private string _tableName;
        private static int _id = 0;
        private string _indexColumnName = null;
        private string _createTableColumnNames;
        private TableStructure _inputTable;
        private List<WhereStructure> _whereList;
        private List<AsStructure> _asList;
        private List<LikeStructure> _likeList;
        private TableStructure _outTable;
        private ColumnStructure[] _outColumn;
        
        public SelectStructure(string name, TableStructure table, List<WhereStructure> whereList, List<AsStructure> asList)
        {
            _name = name;
            _tableName = table.Name;
            _inputTable = table;
            _whereList = whereList;
            _asList = asList;
        }

        public string Output
        {
            get { return _output; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public string IndexColumnName
        {
            get { return _indexColumnName; }
            set { _indexColumnName = value; }
        }

        public string CreateTableColumnNames
        {
            get { return _createTableColumnNames; }
            set { _createTableColumnNames = value; }
        }

        public List<LikeStructure> LikeList
        {
            get { return _likeList; }
            set { _likeList = value; }
        }
        
        public ColumnStructure[] OutColumn
        {
            get { return _outColumn; }
        }

        public TableStructure OutTable
        {
            get { return _outTable; }
        }

        public TableStructure InputTable
        {
            get { return _inputTable; }
        }
        
        public void CreateQuerry()
        {
            //выдернуто из свойства OutTable
            ColumnCounterDelete();
            List<ColumnStructure> tempList = new List<ColumnStructure>();
            foreach (ColumnStructure column in _inputTable.Columns)
            {
                if(column.IsForSelect || column.UsageCounter>0)
                    tempList.Add(column);
            }

            foreach (AsStructure asStructure in _asList)
            {//Сосздать конструктор для новых столбцов
                asStructure.AsRightColumn.UsageCounter = 100;//хардкод, сделать поределение
                                                             
                if (!asStructure.AsRightColumn.IsRenamed && asStructure.AsRightColumn.OldName!=null)
                {
                    string tmpNameHolder = asStructure.AsRightColumn.OldName;
                    asStructure.AsRightColumn.OldName = asStructure.AsRightColumn.Name;
                    asStructure.AsRightColumn.Name = tmpNameHolder;
                    asStructure.AsRightColumn.IsRenamed = true;
                    tempList.Add(asStructure.AsRightColumn);
                }
                else
                {
                    tempList.Add(asStructure.AsRightColumn);
                }
            }
            
            _outColumn = new ColumnStructure[tempList.Count];
            for (int i = 0; i < _outColumn.Length; i++)
            {
                _outColumn[i] = tempList[i];
            }
            _outTable = new TableStructure(_name + "_TB", _outColumn.ToArray());

            _output = "SELECT ";
            bool commaPointer = false;
            for (int i = 0; i < _inputTable.Columns.Length; i++)
            {
                if (_inputTable.Columns[i].UsageCounter > 0 || _inputTable.Columns[i].IsForSelect)
                {
                    
                   if (!commaPointer)
                    {
                        _output += "\r\n\t" + _inputTable.Columns[i].Name;
                        commaPointer = true;
                    }
                   else
                   {
                       _output += ",\r\n\t" + _inputTable.Columns[i].Name;
                   }
                }
            }

            foreach (var asStructure in _asList)
            {
                
                if (_output != "SELECT ")
                {
                    _output += ",";
                    _output += "\r\n\t" + asStructure.AsString + " AS " + asStructure.AsRightColumn.Name;
                }
                else
                {
                    _output += "\r\n\t" + asStructure.AsString + " AS " + asStructure.AsRightColumn.Name;
                }
            }

            _output += "\r\n" + "FROM " + "\r\n\t" + _tableName + "\r\n" ;
            if (_whereList.Count != 0 || _likeList != null)
            {
                _output += "WHERE ";
                foreach (WhereStructure whereStructure in _whereList)
                {
                    if (whereStructure.Table == _tableName)
                    {
                        _output += "\r\n\t" + whereStructure.getWhereString;
                    }

                    if (whereStructure != _whereList.LastOrDefault() || (_likeList!=null && _likeList.Count>0))
                    {
                        _output += " AND ";
                    }

                }

                if (_likeList != null)
                {
                    foreach (LikeStructure like in _likeList)
                    {
                        _output += Environment.NewLine + "\t" + like.LeftColumn.Name + " LIKE " + like.RightExpression;
                        if (like != _likeList.LastOrDefault())
                        {
                            _output += " AND ";
                        }
                    }
                }
            }

            for (int i = 0; i < _outColumn.Length; i++)
            {
                _createTableColumnNames += _outColumn[i].Name + " " + _outColumn[i].Type.Name;
                if (i < _outColumn.Length - 1)
                {
                    _createTableColumnNames += ",\r\n";
                }
            }
            _output += ";";
        }

        private void ColumnCounterDelete()
        {
            foreach (WhereStructure ws in _whereList)
            {
                ws.Column.UsageCounter--;
            }

            foreach (AsStructure aS in _asList)
            {
                foreach (ColumnStructure column in aS.AsColumns)
                {
                    column.UsageCounter--;
                }
            }

            if (_likeList != null)
            {
                foreach (LikeStructure like in _likeList)
                {
                    like.LeftColumn.UsageCounter--;
                }
            }

            foreach (ColumnStructure column in _inputTable.Columns)
            {
                //column.UsageCounter--;
            }
        }
    }
}
