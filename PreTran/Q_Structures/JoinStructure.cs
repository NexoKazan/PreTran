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
        private string _createTableColumnNames;
        private bool _isFirst = false;
        private bool _switched = false;
        private bool _isJoined = false;
        private bool _isFilled = true;       
        private Interval _sourceInterval;
        private BaseRule _sortRule;
        private ColumnStructure _leftColumn;
        private ColumnStructure _rightColumn;
        private TableStructure _outTable;
        private SelectStructure _leftSelect;
        private SelectStructure _rightSelect;
        private JoinStructure _leftJoin;
        private List<ColumnStructure> _columns = new List<ColumnStructure>();

        public JoinStructure(string leftColumn, string rightColumn, string comparisonOperator, Interval sourceInterval,
            BaseRule sortRule)
        {
            _leftColumnString = leftColumn;
            _rightColumnString = rightColumn;
            _comparisonOperator = comparisonOperator;
            _sortRule = sortRule;
            SourceInterval = sourceInterval;
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

        public bool IsJoined
        {
            get { return _isJoined; }
            set { _isJoined = value; }
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

        #endregion

        public void CreateQuerry()
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
                    //bool flag = false;
                    //if (_leftSelect != null)
                    //{
                    //    foreach (ColumnStructure column in _columns)
                    //    {
                    //        foreach (ColumnStructure selectColumn in _leftSelect.OutTable.Columns)
                    //        {
                    //            if (column.Name == selectColumn.Name)
                    //            {
                    //                flag = true;
                    //                break;
                    //            }
                    //        }
                    //    }


                    //    if (flag)
                    //    {
                    //        _columns.AddRange(_rightSelect.OutColumn);
                    //    }
                    //    else
                    //    {
                    //        _columns.AddRange(_leftSelect.OutColumn);
                    //    }
                    //}
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

                if (_leftJoin != null)
                {
                    if (_leftSelect != null || _rightSelect != null)
                    {
                        _output += _leftJoin.Name + ",\r\n\t";
                    }
                    else
                    {
                        _output += _leftJoin.Name + "\r\n";
                    }

                    if (_switched)
                    {
                        if (_leftSelect != null)
                        {
                            _output += _leftSelect.Name + "\r\n";
                        }
                    }
                    else
                    {
                        if (_rightSelect != null)
                        {
                            _output += _rightSelect.Name + "\r\n";
                        }
                    }
                }
                else
                {
                    if (_leftSelect != null)
                    {
                        _output += _leftSelect.Name + ",\r\n\t";
                    }
                    else
                    {
                        _output += "\r\n";
                    }

                    if (_rightSelect != null)
                    {
                        _output += _rightSelect.Name + "\r\n";
                    }
                    else
                    {
                        _output += "\r\n";
                    }
                }

                if (_leftColumn != null && _rightColumn != null)
                {
                    if (_leftColumn.DotTableId == null && _rightColumn.DotTableId == null)
                    {
                        if (!_switched)
                        {
                            _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.Name + " " + _comparisonOperator +
                                       " " + _rightColumn.Name;

                        }
                        else
                        {
                            _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.Name + " " + _comparisonOperator +
                                       " " + _leftColumn.Name;
                        }
                    }
                    else
                    {
                        if (_leftColumn.DotTableId != null && _rightColumn.DotTableId != null)
                        {
                            if (!_switched)
                            {
                                _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.DotTableId + _leftColumn.Name + " " + _comparisonOperator +
                                           " " + _rightColumn.DotTableId + _rightColumn.Name;
                            }
                            else
                            {
                                _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.DotTableId + _rightColumn.Name + " " + _comparisonOperator +
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
                                    _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.Name + " " + _comparisonOperator +
                                               " " + _rightColumn.DotTableId + _rightColumn.Name;

                                }
                                else
                                {
                                    _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.DotTableId + _rightColumn.Name + " " + _comparisonOperator +
                                               " " + _leftColumn.Name;
                                }
                                _rightColumn.Name = _rightColumn.DotTableId + _rightColumn.Name;
                            }

                            if (_leftColumn.DotTableId != null)
                            {
                                if (!_switched)
                                {
                                    _output += Environment.NewLine + "WHERE\r\n\t" + _leftColumn.DotTableId + _leftColumn.Name + " " + _comparisonOperator +
                                               " " + _rightColumn.Name;
                                }
                                else
                                {
                                    _output += Environment.NewLine + "WHERE\r\n\t" + _rightColumn.Name + " " + _comparisonOperator +
                                               " " + _leftColumn.DotTableId + _leftColumn.Name;
                                }
                                _leftColumn.Name = _leftColumn.DotTableId + _leftColumn.Name;
                            }
                        }
                    }
                }

                SetIndex();
                SetCreateTableColumnList();
                SetSortFrom();
                _sortRule.GetRuleBySourceInterval(_sourceInterval).Text = "";
                _sortRule.GetRuleBySourceInterval(_sourceInterval).IsRealised = true;
                _output += ";";
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

                #region old4

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

                _output += left + ", " + Environment.NewLine + right;
                if (_leftColumn != null && _rightColumn != null)
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

                SetIndex();
                SetCreateTableColumnList();
                SetSortFrom();
                _sortRule.GetRuleBySourceInterval(_sourceInterval).Text = "";
                _sortRule.GetRuleBySourceInterval(_sourceInterval).IsRealised = true;
                _output += ";";
            }
        }

        private void SetIndex()
        {
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
                                    _leftSelect.IndexColumnNames.Add(column.Name);
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
                            if (column.Name == LeftColumnString)
                            {
                                _rightSelect.IndexColumnNames.Add(column.Name);
                            }
                            else
                            {
                                if (column.Name == RightColumnString)
                                {
                                    _rightSelect.IndexColumnNames.Add(column.Name);
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
                                _leftSelect.IndexColumnNames.Add(column.Name);
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
                            _rightSelect.IndexColumnNames.Add(column.Name);
                        }
                        else
                        {
                            if (column.Name == RightColumnString)
                            {
                                _rightSelect.IndexColumnNames.Add(column.Name);
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
            }
        }

        private void SetSortFrom()
        {
            if (_leftJoin != null)
            {
                if (_leftJoin.LeftSelect != null)
                {
                    _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval,"atomtableitem").IsRealised =
                        false;
                    _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").Text = "";
                    _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised = true;
                }

                if (_leftJoin.RightSelect != null)
                {
                    _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised =
                        false;
                    _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").Text = "";
                    _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised =
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

            if (_leftJoin != null &&  _rightSelect == null && _leftSelect == null)
            {
                if (_leftJoin.RightSelect != null)
                {
                    _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised =
                        false;
                    _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").Text = _name;
                    _sortRule.GetRule(_leftJoin.RightSelect.InputTable.SourceInterval, "atomtableitem").IsRealised =
                        true;
                }
                else
                {
                    if (_leftJoin.LeftSelect != null)
                    {
                        _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised =
                            false;
                        _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").Text = _name;
                        _sortRule.GetRule(_leftJoin.LeftSelect.InputTable.SourceInterval, "atomtableitem").IsRealised =
                            true;
                    }
                }
            }
            
        }

    }
}
