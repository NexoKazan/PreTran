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
using System.Windows.Forms.VisualStyles;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;
using PreTran.TestClasses.Rules;

namespace PreTran.Q_Structures
{
    class JoinStructure
    {
        private string _leftColumnString;
        private string _rightColumnString;
        private string _name;
        private string _output;
        private string _comparisonOperator;
        private List<string> _indexColumnNames = new List<string>();
        private List<ColumnStructure> _indexColumns = new List<ColumnStructure>();
        private string _createTableColumnNames;
        private bool _isFirst = false;
        private bool _switched = false;
        private bool _isAdditional = false;
        private bool _isFilled = true;
        private bool _isOuterJoin;
        private Interval _sourceInterval;
        private BaseRule _sortRule;
        private ColumnStructure _leftColumn;
        private ColumnStructure _rightColumn;
        private TableStructure _outTable;
        private SelectStructure _leftSelect;
        private SelectStructure _rightSelect;
        private JoinStructure _leftJoin;
        private List<ColumnStructure> _columns = new List<ColumnStructure>();
        private List<JoinStructure> _additionalJoins = new List<JoinStructure>();

        public JoinStructure(string leftColumn, string rightColumn, string comparisonOperator, Interval sourceInterval,
            BaseRule sortRule, bool isOuterJoin)
        {
            _leftColumnString = leftColumn;
            _rightColumnString = rightColumn;
            _comparisonOperator = comparisonOperator;
            _sortRule = sortRule;
            _sourceInterval = sourceInterval;
            _isOuterJoin = isOuterJoin;
        }

        #region Свойства

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string LeftColumnString
        {
            get { return _leftColumnString; }
        }

        public string RightColumnString
        {
            get { return _rightColumnString; }
        }

        public List<string> IndexColumnNames
        {
            get
            {
                if (_indexColumnNames.Count > 0)
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
        }

        public ColumnStructure LeftColumn
        {
            get { return _leftColumn; }
            set { _leftColumn = value; }
        }

        public ColumnStructure RightColumn
        {
            get { return _rightColumn; }
            set { _rightColumn = value; }
        }

        public TableStructure OutTable
        {
            get { return _outTable; }
        }

        public SelectStructure LeftSelect
        {
            get { return _leftSelect; }
            set { _leftSelect = value; }
        }

        public SelectStructure RightSelect
        {
            get { return _rightSelect; }
            set { _rightSelect = value; }
        }

        public List<ColumnStructure> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public string Output
        {
            get { return _output; }
            set { _output = value; }
        }

        public JoinStructure LeftJoin
        {
            get { return _leftJoin; }
            set { _leftJoin = value; }
        }

        public bool IsFirst
        {
            get { return _isFirst; }
            set { _isFirst = value; }
        }

        public bool Switched
        {
            get { return _switched; }
            set { _switched = value; }
        }

        public bool IsAdditional
        {
            get { return _isAdditional; }
            set { _isAdditional = value; }
        }

        public Interval SourceInterval
        {
            get => _sourceInterval;
            set => _sourceInterval = value;
        }

        public bool IsFilled {
            get
            {
                return _isFilled;
            }
            set
            {
                _isFilled = value;
            }
        }

        public List<JoinStructure> AdditionalJoins
        {
            get => _additionalJoins;
            set => _additionalJoins = value;
        }

        public List<ColumnStructure> IndexColumn
        {
            get { return _indexColumns; }
            set { _indexColumns = value; }
        }
        #endregion

        public void CreateQuerry()
        {
            if (_isFilled)
            {

                string left = "ERROR";
                string right = "ERROR";

                #region  старая тема


                //if (_leftJoin != null)
                //{
                //    if (_leftSelect != null || _rightSelect != null)
                //    {
                //        _output += _leftJoin.Name + ",\r\n\t";
                //    }
                //    else
                //    {
                //        _output += _leftJoin.Name + "\r\n";
                //    }

                //    if (_switched)
                //    {
                //        if (_leftSelect != null)
                //        {
                //            _output += _leftSelect.Name + "\r\n";
                //        }
                //    }
                //    else
                //    {
                //        if (_rightSelect != null)
                //        {
                //            _output += _rightSelect.Name + "\r\n";
                //        }
                //    }
                //}
                //else
                //{
                //    if (_leftSelect != null)
                //    {
                //        _output += _leftSelect.Name + ",\r\n\t";
                //    }
                //    else
                //    {
                //        _output += "\r\n";
                //    }

                //    if (_rightSelect != null)
                //    {
                //        _output += _rightSelect.Name + "\r\n";
                //    }
                //    else
                //    {
                //        _output += "\r\n";
                //    }
                //}


                #endregion

                if (_leftJoin != null)
                {
                    left = _leftJoin.Name;
                   
                    if (_switched)
                    {
                        if (_leftSelect != null)
                        {
                            right =  _leftSelect.Name;
                        }
                    }
                    else
                    {
                        if (_rightSelect != null)
                        {
                            right = _rightSelect.Name;
                        }
                    }
                }
                else
                {
                    if (_leftSelect != null)
                    {
                        left = _leftSelect.Name;
                    }

                    if (_rightSelect != null)
                    {
                        right= _rightSelect.Name;
                    }
                }

                this.CreateQuerry(left, right);

            }
        }

        public bool CheckIsFilled()
        {
            if (_leftColumnString == null || _rightColumnString == null || _comparisonOperator == null || _leftColumn == null || _rightColumn == null || _leftSelect == null || _rightSelect == null )
            {
                _isFilled = false;
            }

            if (_leftSelect == null && _leftJoin != null)
            {
                _isFirst = true;
            }
            return _isFilled;
        }

        public void CreateQuerry(string left, string right)
        {
            if (_isFilled)
            {
                if (_leftJoin != null)
                {
                    _columns.AddRange(_leftJoin.Columns);
                    if (_switched)
                    {
                        if (_leftSelect != null)
                            _columns.AddRange(_leftSelect.OutColumn);
                    }
                    else
                    {
                        if (_rightSelect != null)
                            _columns.AddRange(_rightSelect.OutColumn);
                    }
                }
                else
                {
                    if (_leftSelect != null)
                    {
                        _columns.AddRange(_leftSelect.OutColumn);
                    }

                    if (_rightSelect != null)
                    {
                        _columns.AddRange(_rightSelect.OutColumn);

                    }
                }

                foreach (JoinStructure additionalJoin in _additionalJoins)
                {
                    _columns.Add(additionalJoin.LeftColumn);
                    _columns.Add(additionalJoin.RightColumn);
                }

                _columns = RemoveSameNames(_columns);
                ColumnCounterDelete(_columns);
                List<ColumnStructure> tmpColumns = new List<ColumnStructure>();
                foreach (ColumnStructure column in _columns)
                {
                    if (column.IsForSelect || column.UsageCounter > 0)
                    {
                        tmpColumns.Add(column);
                    }
                }

                _columns = tmpColumns;
                _outTable = new TableStructure(_name + "_TB", _columns.ToArray());
                _output = "SELECT";
                bool commaPointer = false;
                for (int i = 0; i < _columns.Count; i++)
                {
                    if (_columns[i].UsageCounter > 0 || _columns[i].IsForSelect)
                    {

                        if (!commaPointer)
                        {
                            _output += "\r\n\t" + _columns[i].Name;
                            commaPointer = true;
                        }
                        else
                        {
                            _output += ",\r\n\t" + _columns[i].Name;
                        }
                    }
                }

                _output += "\r\nFROM\r\n\t";

                if (!_isOuterJoin)
                {
                    //ПРЕДПОЛАГАЕМ ЧТО ТОЛЬКО LEFT
                    _output += left + ", " + Environment.NewLine + "\t" + right;
                }
                else
                {
                    //ПРЕДПОЛАГАЕМ ЧТО ТОЛЬКО LEFT
                    _output += left + " LEFT OUTER JOIN " + right;
                }

                if (_leftColumn != null && _rightColumn != null)
                {
                    if (!_isOuterJoin)
                    {
                        if (_leftColumn.DotTableId == null && _rightColumn.DotTableId == null)
                        {
                            if (!_switched)
                            {
                                _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.Name + " " +
                                           _comparisonOperator +
                                           " " + _rightColumn.Name;

                            }
                            else
                            {
                                _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.Name + " " +
                                           _comparisonOperator +
                                           " " + _leftColumn.Name;
                            }
                        }
                        else
                        {
                            if (_leftColumn.DotTableId != null && _rightColumn.DotTableId != null)
                            {
                                if (!_switched)
                                {
                                    _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.DotTableId +
                                               _leftColumn.Name + " " + _comparisonOperator +
                                               " " + _rightColumn.DotTableId + _rightColumn.Name;
                                }
                                else
                                {
                                    _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.DotTableId +
                                               _rightColumn.Name + " " + _comparisonOperator +
                                               " " + _leftColumn.DotTableId + _leftColumn.Name;
                                }

                                _leftColumn.Name = _leftColumn.DotTableId + _leftColumn.Name;
                                _rightColumn.Name = _rightColumn.DotTableId + _rightColumn.Name;
                            }
                            else
                            {
                                if (_rightColumn.DotTableId != null)
                                {
                                    if (!_switched)
                                    {
                                        _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.Name + " " +
                                                   _comparisonOperator +
                                                   " " + _rightColumn.DotTableId + _rightColumn.Name;

                                    }
                                    else
                                    {
                                        _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.DotTableId +
                                                   _rightColumn.Name + " " + _comparisonOperator +
                                                   " " + _leftColumn.Name;
                                    }

                                    _rightColumn.Name = _rightColumn.DotTableId + _rightColumn.Name;
                                }

                                if (_leftColumn.DotTableId != null)
                                {
                                    if (!_switched)
                                    {
                                        _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.DotTableId +
                                                   _leftColumn.Name + " " + _comparisonOperator +
                                                   " " + _rightColumn.Name;
                                    }
                                    else
                                    {
                                        _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.Name + " " +
                                                   _comparisonOperator +
                                                   " " + _leftColumn.DotTableId + _leftColumn.Name;
                                    }

                                    _leftColumn.Name = _leftColumn.DotTableId + _leftColumn.Name;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_leftColumn.DotTableId == null && _rightColumn.DotTableId == null)
                        {
                            if (!_switched)
                            {
                                _output += Environment.NewLine + "ON\r\n\t" + _leftColumn.Name + " " +
                                           _comparisonOperator +
                                           " " + _rightColumn.Name;

                            }
                            else
                            {
                                _output += Environment.NewLine + "ON\r\n\t" + _rightColumn.Name + " " +
                                           _comparisonOperator +
                                           " " + _leftColumn.Name;
                            }
                        }
                        else
                        {
                            if (_leftColumn.DotTableId != null && _rightColumn.DotTableId != null)
                            {
                                if (!_switched)
                                {
                                    _output += Environment.NewLine + "ON\r\n\t" + _leftColumn.DotTableId +
                                               _leftColumn.Name + " " + _comparisonOperator +
                                               " " + _rightColumn.DotTableId + _rightColumn.Name;
                                }
                                else
                                {
                                    _output += Environment.NewLine + "ON\r\n\t" + _rightColumn.DotTableId +
                                               _rightColumn.Name + " " + _comparisonOperator +
                                               " " + _leftColumn.DotTableId + _leftColumn.Name;
                                }

                                _leftColumn.Name = _leftColumn.DotTableId + _leftColumn.Name;
                                _rightColumn.Name = _rightColumn.DotTableId + _rightColumn.Name;
                            }
                            else
                            {
                                if (_rightColumn.DotTableId != null)
                                {
                                    if (!_switched)
                                    {
                                        _output += Environment.NewLine + "ON\r\n\t" + _leftColumn.Name + " " +
                                                   _comparisonOperator +
                                                   " " + _rightColumn.DotTableId + _rightColumn.Name;

                                    }
                                    else
                                    {
                                        _output += Environment.NewLine + "ON\r\n\t" + _rightColumn.DotTableId +
                                                   _rightColumn.Name + " " + _comparisonOperator +
                                                   " " + _leftColumn.Name;
                                    }

                                    _rightColumn.Name = _rightColumn.DotTableId + _rightColumn.Name;
                                }

                                if (_leftColumn.DotTableId != null)
                                {
                                    if (!_switched)
                                    {
                                        _output += Environment.NewLine + "ON\r\n\t" + _leftColumn.DotTableId +
                                                   _leftColumn.Name + " " + _comparisonOperator +
                                                   " " + _rightColumn.Name;
                                    }
                                    else
                                    {
                                        _output += Environment.NewLine + "ON\r\n\t" + _rightColumn.Name + " " +
                                                   _comparisonOperator +
                                                   " " + _leftColumn.DotTableId + _leftColumn.Name;
                                    }

                                    _leftColumn.Name = _leftColumn.DotTableId + _leftColumn.Name;
                                }
                            }
                        }
                    }
                }

                foreach (JoinStructure addJoin in _additionalJoins)
                {
                   if (addJoin.LeftColumn != null && addJoin.RightColumn != null)
                   {
                       if (addJoin.LeftColumn.DotTableId == null && addJoin.RightColumn.DotTableId == null)
                       {
                           if (!_switched)
                           {
                               _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.Name + " " + _comparisonOperator +
                                          " " + addJoin.RightColumn.Name;

                           }
                           else
                           {
                               _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.Name + " " + _comparisonOperator +
                                          " " + addJoin.LeftColumn.Name;
                           }
                       }
                       else
                       {
                           if (addJoin.LeftColumn.DotTableId != null && addJoin.RightColumn.DotTableId != null)
                           {
                               if (!_switched)
                               {
                                   _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.DotTableId + addJoin.LeftColumn.Name + " " + _comparisonOperator +
                                              " " + addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;
                               }
                               else
                               {
                                   _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name + " " + _comparisonOperator +
                                              " " + addJoin.LeftColumn.DotTableId + addJoin.LeftColumn.Name;
                               }

                               addJoin.LeftColumn.Name = addJoin.LeftColumn.DotTableId + addJoin.LeftColumn.Name;
                               addJoin.RightColumn.Name = addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;
                           }
                           else
                           {
                               if (addJoin.RightColumn.DotTableId != null)
                               {
                                   if (!_switched)
                                   {
                                       _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.Name + " " + _comparisonOperator +
                                                  " " + addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;

                                   }
                                   else
                                   {
                                       _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name + " " + _comparisonOperator +
                                                  " " + addJoin.LeftColumn.Name;
                                   }
                                   addJoin.RightColumn.Name = addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;
                               }

                               if (addJoin.LeftColumn.DotTableId != null)
                               {
                                   if (!_switched)
                                   {
                                       _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.DotTableId + addJoin.LeftColumn.Name + " " + _comparisonOperator +
                                                  " " + addJoin.RightColumn.Name;
                                   }
                                   else
                                   {
                                       _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.Name + " " + _comparisonOperator +
                                                  " " + addJoin.LeftColumn.DotTableId + addJoin.LeftColumn.Name;
                                   }
                                   addJoin.LeftColumn.Name = addJoin.LeftColumn.DotTableId + addJoin.LeftColumn.Name;
                               }
                           }
                       }
                   }
                }

                //SetIndex();
                SetCreateTableColumnList();
                SetSortFrom();
                _sortRule.GetRuleBySourceInterval(_sourceInterval).Text = "";
                _sortRule.GetRuleBySourceInterval(_sourceInterval).IsRealised = true;
                _output += ";";
            }
        }

        private List<ColumnStructure> RemoveSameNames(List<ColumnStructure> columns)
        {
            List<ColumnStructure> tmpColumns = new List<ColumnStructure>();
            tmpColumns.Add(columns[0]);
            bool isForAdd ;
            foreach (ColumnStructure column in columns)
            {
                isForAdd = true;
                foreach (ColumnStructure columnStructure in tmpColumns)
                {
                    if (column.Name == columnStructure.Name)
                    {
                        isForAdd = false;
                    }
                }

                if (isForAdd)
                {
                    tmpColumns.Add(column);
                }
            }
            return tmpColumns;
        }

        public void SetIndex()
        {
            #region OLD

            /*
            if (_leftJoin != null)
            {
                foreach (ColumnStructure column in _leftJoin.Columns)
                {
                    if (column.Name == LeftColumnString)
                    {
                        _leftJoin.IndexColumnNames.Add(column.Name);
                    }

                    if (column.Name == RightColumnString)
                    {
                        _leftJoin.IndexColumnNames.Add(column.Name);
                    }

                    foreach (JoinStructure addJoin in _additionalJoins)
                    {
                        if (column.Name == addJoin.LeftColumnString)
                        {
                            _leftJoin.IndexColumnNames.Add(column.Name);
                        }

                        if (column.Name == addJoin.RightColumnString)
                        {
                            _leftJoin.IndexColumnNames.Add(column.Name);
                        }
                    }

                }

                if (_switched)
                {
                    if (_leftSelect != null)
                    {
                        foreach (ColumnStructure column in _leftSelect.OutColumn)
                        {
                            if (column.Name == LeftColumnString)
                            {
                                _leftSelect.IndexColumnNames.Add(column.Name);
                            }
                            else
                            {
                                if (column.Name == RightColumnString)
                                {
                                    //_leftSelect.IndexColumnNames.Add(column.Name);
                                }
                            }

                            foreach (JoinStructure addJoin in _additionalJoins)
                            {
                                if (column.Name == addJoin.LeftColumnString)
                                {
                                    _leftSelect.IndexColumnNames.Add(column.Name);
                                }
                                else
                                {
                                    if (column.Name == addJoin.RightColumnString)
                                    {
                                        _leftSelect.IndexColumnNames.Add(column.Name);
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (_rightSelect != null)
                    {
                        foreach (ColumnStructure column in _rightSelect.OutColumn)
                        {
                            if (column.Name == LeftColumnString )
                            {
                                //_rightSelect.IndexColumnNames.Add(column.Name);
                            }
                            else
                            {
                                if (column.Name == RightColumnString)
                                {
                                    _rightSelect.IndexColumnNames.Add(column.Name);
                                }
                            }

                            foreach (JoinStructure addJoin in _additionalJoins)
                            {
                                if (column.Name == addJoin.LeftColumnString)
                                {
                                    _rightSelect.IndexColumnNames.Add(column.Name);
                                }
                                else
                                {
                                    if (column.Name == addJoin.RightColumnString)
                                    {
                                        _rightSelect.IndexColumnNames.Add(column.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_leftSelect != null)
                {
                    foreach (ColumnStructure column in _leftSelect.OutColumn)
                    {
                        if (column.Name == LeftColumnString)
                        {
                            _leftSelect.IndexColumnNames.Add(column.Name);
                        }
                        else
                        {
                            if (column.Name == RightColumnString)
                            {
                                //_leftSelect.IndexColumnNames.Add(column.Name);
                            }
                        }

                        foreach (JoinStructure addJoin in _additionalJoins)
                        {
                            if (column.Name == addJoin.LeftColumnString)
                            {
                                _leftSelect.IndexColumnNames.Add(column.Name);
                            }
                            else
                            {
                                if (column.Name == addJoin.RightColumnString)
                                {
                                   _leftSelect.IndexColumnNames.Add(column.Name);
                                }
                            }
                        }
                    }

                }

                if (_rightSelect != null)
                {

                    foreach (ColumnStructure column in _rightSelect.OutColumn)
                    {
                        if (column.Name == LeftColumnString)
                        {
                            //_rightSelect.IndexColumnNames.Add(column.Name);
                        }
                        else
                        {
                            if (column.Name == RightColumnString)
                            {
                                _rightSelect.IndexColumnNames.Add(column.Name);
                            }
                        }

                        foreach (JoinStructure addJoin in _additionalJoins)
                        {

                            if (column.Name == addJoin.LeftColumnString)
                            {
                                _rightSelect.IndexColumnNames.Add(column.Name);
                            }
                            else
                            {
                                if (column.Name == addJoin.RightColumnString)
                                {
                                   _rightSelect.IndexColumnNames.Add(column.Name);
                                }
                            }
                        }
                    }

                }
            }

            if (_indexColumnNames.Count < 1)
            {
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.IsPrimary > 0)
                    {
                        _indexColumnNames.Add(column.Name);
                    }

                }
            }

            if (_indexColumnNames.Count < 1)
            {
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.Type.Name == "INT")
                    {
                        _indexColumnNames.Add(column.Name);
                        break;
                    }
                }
            }
            */
            #endregion

            List<string> possibleIndexNames = new List<string>();

            possibleIndexNames.Add(_leftColumn.Name);
            possibleIndexNames.Add(_rightColumn.Name);

            foreach (JoinStructure additionalJoin in _additionalJoins)
            {
                possibleIndexNames.Add(additionalJoin.LeftColumn.Name);
                possibleIndexNames.Add(additionalJoin.RightColumn.Name);
            }
            
            if (_leftJoin != null)
            {
                foreach (string name in possibleIndexNames)
                {
                    foreach (ColumnStructure column in _leftJoin.OutTable.Columns)
                    {
                        if (column.Name == name)
                        {
                            _leftJoin.IndexColumnNames.Add(column.Name);
                            _leftJoin.IndexColumn.Add(column);
                        }
                    }
                }
                if (_switched)
                {
                    if (_leftSelect.IndexColumnNames.Count > 0)
                    {
                        _leftSelect.IndexColumnNames = new List<string>();
                        _leftSelect.IndexColumns = new List<ColumnStructure>();
                    }
                    foreach (string name in possibleIndexNames)
                    {
                        foreach (ColumnStructure column in _leftSelect.OutTable.Columns)
                        {
                            if (column.Name == name)
                            {
                                _leftSelect.IndexColumnNames.Add(column.Name);
                                _leftSelect.IndexColumns.Add(column);
                            }
                        }
                    }
                }
                else
                {
                    if (_rightSelect.IndexColumnNames.Count > 0)
                    {
                        _rightSelect.IndexColumnNames = new List<string>();
                        _rightSelect.IndexColumns = new List<ColumnStructure>();
                    }
                    foreach (string name in possibleIndexNames)
                    {
                        foreach (ColumnStructure column in _rightSelect.OutTable.Columns)
                        {
                            if (column.Name == name)
                            {
                                _rightSelect.IndexColumnNames.Add(column.Name);
                                _rightSelect.IndexColumns.Add(column);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_leftSelect.IndexColumnNames.Count > 0)
                {
                    _leftSelect.IndexColumnNames = new List<string>();
                    _leftSelect.IndexColumns = new List<ColumnStructure>();
                }
                foreach (string name in possibleIndexNames)
                {
                    foreach (ColumnStructure column in _leftSelect.OutTable.Columns)
                    {
                        if (column.Name == name)
                        {
                            _leftSelect.IndexColumnNames.Add(column.Name);
                            _leftSelect.IndexColumns.Add(column);
                        }
                    }
                }
                
                if (_rightSelect.IndexColumnNames.Count > 0)
                {
                    _rightSelect.IndexColumnNames = new List<string>();
                    _rightSelect.IndexColumns = new List<ColumnStructure>();
                }
                foreach (string name in possibleIndexNames)
                {
                    foreach (ColumnStructure column in _rightSelect.OutTable.Columns)
                    {
                        if (column.Name == name)
                        {
                            _rightSelect.IndexColumnNames.Add(column.Name);
                            _rightSelect.IndexColumns.Add(column);
                        }
                    }
                }
            }


            if (_indexColumnNames.Count < 1)
            {
                foreach (ColumnStructure column in _outTable.Columns)
                {
                    if (column.IsPrimary == 1)
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

        private void SetCreateTableColumnList()
        {
            for (int i = 0; i < _outTable.Columns.Length; i++)
            {
                _createTableColumnNames += _outTable.Columns[i].Name + " " + _outTable.Columns[i].Type.Name;
                if (i < _outTable.Columns.Length - 1)
                {
                    _createTableColumnNames += ",\r\n";
                }
            }

        }

        private void ColumnCounterDelete(List<ColumnStructure> columns)
        {
            foreach (ColumnStructure column in columns)
            {
                if (_leftColumn != null && _leftColumn.Name == column.Name)
                {
                    column.UsageCounter--;
                    _leftColumn.UsageCounter--;
                }

                if (_rightColumn != null && _rightColumn.Name == column.Name)
                {

                    column.UsageCounter--;
                    _rightColumn.UsageCounter--;
                }

                //foreach (JoinStructure addJoin in AdditionalJoins)
                //{
                //    if (addJoin.LeftColumn != null && addJoin.LeftColumn.Name == column.Name)
                //    {
                //        column.UsageCounter--;
                //        addJoin.LeftColumn.UsageCounter--;
                //    }

                //    if (addJoin.RightColumn != null && addJoin.RightColumn.Name == column.Name)
                //    {
                //        column.UsageCounter--;
                //        addJoin.RightColumn.UsageCounter--;
                //    }
                //}
            }
        }

        private void SetSortFrom()
        {
            if (!_isAdditional)
            {
                if (!_isOuterJoin)
                {
                    if (_leftJoin != null)
                    {
                        if (_leftJoin.LeftSelect != null)
                        {
                            _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                false;
                            _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").Text =
                                "";
                            _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                true;
                        }

                        if (_leftJoin.RightSelect != null)
                        {
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                false;
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").Text =
                                "";
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                true;
                        }
                    }

                    if (_leftSelect != null)
                    {
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = false;
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").Text = "";
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                    }

                    if (_rightSelect != null)
                    {
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = false;
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").Text = _name;
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                    }

                    if (_leftJoin != null && _rightSelect == null && _leftSelect == null)
                    {
                        if (_leftJoin.RightSelect != null)
                        {
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                false;
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").Text =
                                _name;
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                true;
                        }
                        else
                        {
                            if (_leftJoin.LeftSelect != null)
                            {
                                _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                        .IsRealised =
                                    false;
                                _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                        .Text =
                                    _name;
                                _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                        .IsRealised =
                                    true;
                            }
                        }
                    }
                }
                else
                {
                    if (_leftJoin != null)
                    {
                        if (_leftJoin.LeftSelect != null)
                        {
                            _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                false;
                            _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").Text =
                                "";
                            _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                true;
                        }

                        if (_leftJoin.RightSelect != null)
                        {
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                false;
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").Text =
                                "";
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                true;
                        }
                    }

                    if (_leftSelect != null)
                    {
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = false;
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").Text = _name;
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                    }

                    if (_rightSelect != null)
                    {
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = false;
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").Text = "";
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                    }

                    if (_leftJoin != null && _rightSelect == null && _leftSelect == null)
                    {
                        if (_leftJoin.RightSelect != null)
                        {
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                false;
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").Text =
                                _name;
                            _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem")
                                    .IsRealised =
                                true;
                        }
                        else
                        {
                            if (_leftJoin.LeftSelect != null)
                            {
                                _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                        .IsRealised =
                                    false;
                                _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                        .Text =
                                    _name;
                                _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem")
                                        .IsRealised =
                                    true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!_switched)
                {
                    if (_leftSelect != null)
                    {
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = false;
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").Text = "";
                        _sortRule.GetRule(_leftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                    }
                }
                else
                {
                    if (_rightSelect != null)
                    {
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = false;
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").Text = "";
                        _sortRule.GetRule(_rightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                    }
                }
            }

            if (_additionalJoins.Count > 0)
            {
                foreach (JoinStructure addJoin in _additionalJoins)
                {
                    ///ЫЫЫ кривой хардкод, переделать. Слишком много делается, а нужно только сделать метод setSortFrom
                    addJoin.CreateQuerry(_name,_name);
                }
            }
        }

        public void CheckIsDistinct()
        {
            bool isDistinct = true;

            if (_leftJoin != null)
            {
                foreach (ColumnStructure column in _leftJoin.IndexColumn)
                {
                    if (column.IsPrimary == 1)
                    {
                        isDistinct = false;
                        break;
                    }
                }
                if (_switched)
                {
                    foreach (ColumnStructure column in _leftSelect.IndexColumns)
                    {
                        if (column.IsPrimary == 1)
                        {
                            isDistinct = false;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (ColumnStructure column in _rightSelect.IndexColumns)
                    {
                        if (column.IsPrimary == 1)
                        {
                            isDistinct = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (ColumnStructure column in _leftSelect.IndexColumns)
                {
                    if (column.IsPrimary == 1)
                    {
                        isDistinct = false;
                        break;
                    }
                }
                foreach (ColumnStructure column in _rightSelect.IndexColumns)
                {
                    if (column.IsPrimary == 1)
                    {
                        isDistinct = false;
                        break;
                    }
                }
            }

            if (isDistinct)
            {
                _output = _output.Insert(6, " DISTINCT");
            }

            //Console.WriteLine(Environment.NewLine + "========" + _name + "========" );
            //Console.WriteLine(Environment.NewLine + "Имена" + "========");
            //foreach (string name in _indexColumnNames)
            //{
            //    Console.WriteLine(Environment.NewLine + name);
            //}
            //Console.WriteLine(Environment.NewLine + "Столбцы" + "========");
            //foreach (ColumnStructure column in _indexColumns)
            //{
            //    Console.WriteLine(Environment.NewLine + column.Name);
        }
    }
}
