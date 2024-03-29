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
using System.Dynamic;
using System.Linq;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;
using PreTran.Q_Part_Structures;
using PreTran.TestClasses.Rules;

namespace PreTran.Q_Structures
{
    class SelectStructure
    {
        private string _name;
        private string _output = "error";
        private string _tableName;
        private List<string> _indexColumnNames = new List<string>();
        private List<ColumnStructure> _indexColumns = new List<ColumnStructure>();
        private string _createTableColumnNames;
        private BaseRule _sortRule;
        private TableStructure _inputTable;
        private List<WhereStructure> _whereList;
        private List<AsStructure> _asList;
        private List<LikeStructure> _likeList;
        private List<BetweenStructure> _betweenList;
        private List<InStructure> _inStructureList;
        private TableStructure _outTable;
        private ColumnStructure[] _outColumn;
        
        public SelectStructure(string name, TableStructure table, List<WhereStructure> whereList, List<AsStructure> asList, BaseRule sortRule, List<BetweenStructure> betweenList)
        {
            _name = name;
            _tableName = table.Name;
            _inputTable = table;
            _whereList = whereList;
            _asList = asList;
            _betweenList = betweenList;
            _sortRule = sortRule;
        }

        public SelectStructure(SelectStructure inSelect)
        {
            _name = inSelect.Name;
            _output = inSelect.Output;
            _tableName = inSelect.TableName;

            _indexColumnNames = inSelect.IndexColumnNames;
            foreach (ColumnStructure column in inSelect.IndexColumns)
            {
                _indexColumns.Add(new ColumnStructure(column));
            }
            _createTableColumnNames = inSelect.CreateTableColumnNames;
            _sortRule = inSelect.SortRule;
            _inputTable = inSelect.InputTable;
            _whereList = inSelect._whereList;
            _asList = inSelect._asList;
            _likeList = inSelect.LikeList;
            _betweenList = inSelect._betweenList;
            _inStructureList = inSelect.InStructureList;
            _outTable = inSelect.OutTable;
            List<ColumnStructure> tmpColumns = new List<ColumnStructure>();
            foreach (ColumnStructure column in inSelect.OutColumn)
            {
                tmpColumns.Add(new ColumnStructure(column));
            }
            _outColumn = tmpColumns.ToArray();
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

        public List<string> IndexColumnNames
        {
            get
            {
                if (_indexColumnNames.Count > 1)
                {
                    _indexColumnNames = _indexColumnNames.Distinct().ToList();
                }
                return _indexColumnNames;
            }
            set { _indexColumnNames = value; }
        }

        public string CreateTableColumnNames
        {
            get { return _createTableColumnNames; }
            set { _createTableColumnNames = value; }
        }

        public BaseRule SortRule
        {
            get { return _sortRule; }
            set { _sortRule = value; }
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

        public List<InStructure> InStructureList
        {
            get { return _inStructureList; }
            set { _inStructureList = value; }
        }

        public List<ColumnStructure> IndexColumns
        {
            get { return _indexColumns; }
            set { _indexColumns = value; }
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
            {
                if (!asStructure.IsSortPart)
                {
                    //Сосздать конструктор для новых столбцов
                    asStructure.AsRightColumn.UsageCounter = 100; //хардкод, сделать поределение

                    if (!asStructure.AsRightColumn.IsRenamed && asStructure.AsRightColumn.OldName != null)
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
            }

            _outColumn = new ColumnStructure[tempList.Count];
            for (int i = 0; i < _outColumn.Length; i++)
            {
                _outColumn[i] = tempList[i];
            }
            _outTable = new TableStructure(_name + "_TB", _outColumn.ToArray());
            _outTable.RowCount = _inputTable.RowCount;
            _output = "SELECT ";
           

            bool commaPointer = false;
            for (int i = 0; i < _inputTable.Columns.Length; i++)
            {
                if (_inputTable.Columns[i].UsageCounter > 0 || _inputTable.Columns[i].IsForSelect)
                {

                    if (!commaPointer)
                    {
                        if (_inputTable.Columns[i].DotTableId == null)
                        {
                            _output += "\r\n\t" + _inputTable.Columns[i].Name;
                            commaPointer = true;
                        }
                        else
                        {
                            _output += "\r\n\t" + _inputTable.Columns[i].Name + " AS " + _inputTable.Columns[i].DotTableId + _inputTable.Columns[i].Name;
                            _inputTable.Columns[i].Name =
                                _inputTable.Columns[i].DotTableId + _inputTable.Columns[i].Name;
                            commaPointer = true;
                        }
                    }
                    else
                    {
                        if (_inputTable.Columns[i].DotTableId == null)
                        {
                            _output += ",\r\n\t" + _inputTable.Columns[i].Name;
                        }
                        else
                        {
                            _output += ",\r\n\t" + _inputTable.Columns[i].Name + " AS " + _inputTable.Columns[i].DotTableId + _inputTable.Columns[i].Name;
                            _inputTable.Columns[i].Name =
                                _inputTable.Columns[i].DotTableId + _inputTable.Columns[i].Name;
                        }
                    }
                }
            }

            foreach (var asStructure in _asList)
            {
                if (!asStructure.IsSortPart)
                {
                    if (_output != "SELECT ")
                    {
                        if (_output != "SELECT DISTINCT ")
                        {
                            _output += ",";
                            _output += "\r\n\t" + asStructure.AsString + " AS " + asStructure.AsRightColumn.Name;
                        }
                        else
                        {
                            _output += "\r\n\t" + asStructure.AsString + " AS " + asStructure.AsRightColumn.Name;
                        }
                    }
                    else
                    {
                        _output += "\r\n\t" + asStructure.AsString + " AS " + asStructure.AsRightColumn.Name;
                    }
                }
            }

            _output += "\r\n" + "FROM " + "\r\n\t" + _tableName + "\r\n" ;

            

            if (_whereList.Count != 0 || _likeList != null || _inStructureList != null || _betweenList.Count > 0)
            {
                _output += "WHERE ";
                foreach (WhereStructure whereStructure in _whereList)
                {
                    if (whereStructure.Table == _tableName)
                    {
                        _output += "\r\n\t" + whereStructure.getWhereString;

                    }

                    if (whereStructure != _whereList.LastOrDefault() || (_likeList != null && _likeList.Count>0) || 
                        (_inStructureList != null && _inStructureList.Count > 0) || _betweenList.Count > 0)
                    {
                        _output += " AND ";
                    }
                }

                if (_likeList != null)
                {
                    foreach (LikeStructure like in _likeList)
                    {
                        if (!like.IsNot)
                        {

                            _output += Environment.NewLine + "\t" + like.LeftColumn.Name + " LIKE " +
                                       like.RightExpression;
                        }
                        else
                        {
                            _output += Environment.NewLine + "\t" + like.LeftColumn.Name + " NOT LIKE " +
                                       like.RightExpression;
                        }

                        if (like != _likeList.LastOrDefault() || _betweenList.Count > 0 || _inStructureList != null)
                        {
                            _output += " AND ";
                        }
                    }
                }

                if (_inStructureList != null)
                {
                    foreach (InStructure inStructure  in _inStructureList)
                    {
                        _output += Environment.NewLine + "\t" + inStructure.FullString;
                        if (inStructure != _inStructureList.LastOrDefault() || _betweenList.Count > 0)
                        {
                            _output += " AND ";
                        }
                    }
                }


                if (_betweenList.Count > 0)
                {
                    foreach (BetweenStructure betweenStructure in _betweenList)
                    {
                        _output += Environment.NewLine + "\t" + betweenStructure.Text;
                        if (betweenStructure != _betweenList.LastOrDefault() )
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

            foreach (ColumnStructure column in _outTable.Columns)
            {
                if (column.DotTableId != null)
                {
                    List<BaseRule> tmpRules = new List<BaseRule>();
                    foreach (Interval sourceInterval in column.SoureInterval)
                    {
                        tmpRules.Add(_sortRule.GetRuleBySourceInterval(sourceInterval));
                    }
                }
            }

            //SetIndexes();
        }

        private bool CheckForDistinctL()
        {
            bool isDistinct = true;
            int columsCounter = 2;
            foreach (ColumnStructure column in _outTable.Columns)
            {
                if (column.IsPrimary == 1)
                {
                    isDistinct = false;
                    break;
                }

                if (column.IsPrimary == 2)
                {
                    columsCounter--;
                }

            }

            if (columsCounter <= 0)
            {
                isDistinct = false;
            }
            return isDistinct;
        }

        public void CheckForDistinct()
        {
            if (!IsWithDistinct && CheckForDistinctL())
            {
                IsWithDistinct = true;
                _output = _output.Insert(7, "DISTINCT");
            }
        }
        
        public bool IsWithDistinct { get; set; }

        public void SetIndexes()
        {
            if (_indexColumnNames.Count < 1)
            {
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.IsPrimary == 1 )
                    {
                        _indexColumns.Add(column);
                        _indexColumnNames.Add(column.Name);
                    }
                }
            }

            if (_indexColumnNames.Count < 1)
            {
                int primaryKeyCount = 0;
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.IsPrimary > 1)
                    {
                        primaryKeyCount = column.IsPrimary;
                        break;
                    }
                }
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.IsPrimary > 1 && primaryKeyCount > 0)
                    {
                        _indexColumns.Add(column);
                        _indexColumnNames.Add(column.Name);
                        primaryKeyCount--;
                    }
                }
            }
            if (_indexColumnNames.Count < 1)
            {
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.Type.Name == "INT")
                    {
                        _indexColumns.Add(column);
                        _indexColumnNames.Add(column.Name);
                        break;
                    }
                }
            }
        }

        public void ChangeSort()
        {
            foreach (var asStructure in _asList)
            {
                if (!asStructure.IsSortPart)
                {
                    //_sortRule.GetRuleBySourceInterval(asStructure.SourceInterval).IsRealised = false;
                    if (asStructure.AggregateFunctionName != null)
                    {
                        if (asStructure.AggregateFunctionName.ToLower() != "extract")
                        {
                            _sortRule.GetRuleBySourceInterval(asStructure.SourceInterval).Text =
                                asStructure.AggregateFunctionName + "(" + asStructure.AsRightColumn.Name + ")" +
                                " AS " +
                                asStructure.AsRightColumn.OldName;
                            _sortRule.GetRuleBySourceInterval(asStructure.SourceInterval).IsRealised = true;
                        }
                        else
                        {
                            _sortRule.GetRuleBySourceInterval(asStructure.SourceInterval).Text =
                                asStructure.AsRightColumn.Name + " AS " +
                                asStructure.AsRightColumn.OldName;
                            _sortRule.GetRuleBySourceInterval(asStructure.SourceInterval).IsRealised = true;
                        }
                    }
                }
            }

            BaseRule tableRule = _sortRule.GetRule(_inputTable.SourceInterval, "atomtableitem");
            tableRule.IsRealised = false;
            tableRule.Text = _name;
            tableRule.IsRealised = true;

            if (_whereList.Count != 0 || _likeList != null || _inStructureList != null || _betweenList.Count > 0)
            {
                foreach (WhereStructure whereStructure in _whereList)
                {
                    if (whereStructure.Table == _tableName)
                    {
                        _sortRule.GetRuleBySourceInterval(_inputTable.SourceInterval).IsRealised = false;
                        _sortRule.GetRuleBySourceInterval(whereStructure.SourceInterval).Text = "";
                        _sortRule.GetRuleBySourceInterval(whereStructure.SourceInterval).IsRealised = true;

                    }
                }

                if (_likeList != null)
                {
                    foreach (LikeStructure like in _likeList)
                    {
                        _sortRule.GetRuleBySourceInterval(_inputTable.SourceInterval).IsRealised = false;
                        _sortRule.GetRuleBySourceInterval(like.SourceInterval).Text = "";
                        _sortRule.GetRuleBySourceInterval(like.SourceInterval).IsRealised = true;
                    }
                }

                if (_inStructureList != null)
                {
                    foreach (InStructure inStructure in _inStructureList)
                    {
                        _sortRule.GetRuleBySourceInterval(_inputTable.SourceInterval).IsRealised = false;
                        _sortRule.GetRuleBySourceInterval(inStructure.SourceInterval).Text = "";
                        _sortRule.GetRuleBySourceInterval(inStructure.SourceInterval).IsRealised = true;
                    }
                }


                if (_betweenList.Count > 0)
                {
                    foreach (BetweenStructure betweenStructure in _betweenList)
                    {
                        _sortRule.GetRuleBySourceInterval(_inputTable.SourceInterval).IsRealised = false;
                        _sortRule.GetRuleBySourceInterval(betweenStructure.SourceInterval).Text = "";
                        _sortRule.GetRuleBySourceInterval(betweenStructure.SourceInterval).IsRealised = true;
                    }
                }
            }

            foreach (ColumnStructure column in _outTable.Columns)
            {
                if (column.DotTableId != null)
                {
                    foreach (Interval sourceInterval in column.SoureInterval)
                    {
                        _sortRule.GetRuleBySourceInterval(sourceInterval).IsRealised = false;
                        _sortRule.GetRuleBySourceInterval(sourceInterval).Text = column.Name;
                        _sortRule.GetRuleBySourceInterval(sourceInterval).IsRealised = true;
                    }
                }
            }
        }

        private void ColumnCounterDelete()
        {
            foreach (WhereStructure ws in _whereList)
            {
                ws.Column.UsageCounter--;
                if (ws.RightColumn != null)
                {
                    ws.RightColumn.UsageCounter--;
                }
            }

            foreach (AsStructure aS in _asList)
            {
                if (!aS.IsSortPart)
                {
                    foreach (ColumnStructure column in aS.AsColumns)
                    {
                        column.UsageCounter--;
                    }
                }
            }

            if (_likeList != null)
            {
                foreach (LikeStructure like in _likeList)
                {
                    like.LeftColumn.UsageCounter--;
                }
            }

            if (_betweenList.Count > 0)
            {
                foreach (BetweenStructure betweenStructure in _betweenList)
                {
                    betweenStructure.Column.UsageCounter--;
                }
            }

            foreach (ColumnStructure column in _inputTable.Columns)
            {
                if( !column.IsForSelect)
                {
                    //column.UsageCounter--;
                }
            }
        }
    }
}
