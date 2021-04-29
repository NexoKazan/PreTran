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
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;
using PreTran.TestClasses.Rules;

namespace PreTran.Q_Structures
{
    class JoinStructure
    {
        Guid _jGuid;
        private string _leftColumnString;
        private string _rightColumnString;
        private string _name;
        private string _output;
        private string _comparisonOperator;
        private int _inputRowsSize;
        private List<string> _indexColumnNames = new List<string>();
        private List<ColumnStructure> _indexColumns = new List<ColumnStructure>();
        private string _createTableColumnNames;
        private string _comparitionString;
        private bool _isFirst = false;
        private bool _switched = false;
        private bool _isAdditional = false;
        private bool _isFilled = true;
        private bool _isOuterJoin;
        private bool _isJoined = false; //Всегда должна быть FALSE используется только при создании последовательности join 
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
            _jGuid = Guid.NewGuid();
            _leftColumnString = leftColumn;
            _rightColumnString = rightColumn;
            _comparisonOperator = comparisonOperator;
            _sortRule = sortRule;
            _sourceInterval = sourceInterval;
            _isOuterJoin = isOuterJoin;
        }

        public JoinStructure(JoinStructure inJoin)
        {
            _jGuid = Guid.NewGuid();
            _leftColumnString = inJoin.LeftColumnString;
            _rightColumnString = inJoin.RightColumnString;
            _name = inJoin.Name;
            _output = inJoin.Output;
            _comparisonOperator = inJoin.ComparisonOperator;
            _inputRowsSize = inJoin.InputRowSize;
            _indexColumnNames = inJoin.IndexColumnNames;
            _indexColumns = inJoin.IndexColumns;
            _createTableColumnNames = inJoin.CreateTableColumnNames;
            _comparitionString = inJoin.ComparitionString;
            _isFirst = inJoin.IsFirst;
            _switched = inJoin.Switched;
            _isAdditional = inJoin.IsAdditional;
            _isFilled = inJoin.IsFilled;
            _isOuterJoin = inJoin.IsOuterJoin;
            _sourceInterval = inJoin.SourceInterval;
            _sortRule = inJoin.SortRule;
            _leftColumn = inJoin.LeftColumn;
            _rightColumn = inJoin.RightColumn;
            _outTable = inJoin.OutTable;
            _leftSelect = inJoin.LeftSelect;
            _rightSelect = inJoin.RightSelect;
            _leftJoin = inJoin.LeftJoin;

            foreach (ColumnStructure column in inJoin.Columns)
            {
                _columns.Add(new ColumnStructure(column));
            }

            foreach (JoinStructure addjoin in inJoin.AdditionalJoins)
            {
                _additionalJoins.Add(addjoin);
            }

        }

        #region Свойства

        public Guid JGuid
        {
            get
            {
                return _jGuid;
            }
        }

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

        public string ComparitionString
        {
            get
            {
                _comparitionString = "";

                _comparitionString += LeftColumnString + " " + ComparisonOperator + " " + RightColumnString;
                if (AdditionalJoins.Count > 0)
                {
                    foreach (var additionalJoin in AdditionalJoins)
                    {
                        _comparitionString += Environment.NewLine + additionalJoin.ComparitionString;
                    }
                }

                return _comparitionString;
            }
        }

        public int InputRowSize
        {
            get
            {
                _inputRowsSize = 0;

                if (_leftJoin != null)
                {
                    _inputRowsSize += _leftJoin.InputRowSize;
                    //if (_switched)
                    //{
                    //    if(_leftSelect != null)
                    //        _inputRowsSize += _leftSelect.OutTable.RowSize;
                    //}
                    //else
                    //{
                    //    if(_rightSelect != null)
                    //        _inputRowsSize += _rightSelect.OutTable.RowSize;
                    //}
                }
                else
                {
                    //_inputRowsSize += _leftSelect.OutTable.RowSize + _rightSelect.OutTable.RowSize;
                }
                

                return _inputRowsSize;
            }
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

        public bool IsFilled
        {
            get { return _isFilled; }
            set { _isFilled = value; }
        }

        public List<JoinStructure> AdditionalJoins
        {
            get => _additionalJoins;
            set => _additionalJoins = value;
        }

        public List<ColumnStructure> IndexColumns
        {
            get { return _indexColumns; }
            set { _indexColumns = value; }
        }

        public string ComparisonOperator => _comparisonOperator;

        public BaseRule SortRule
        {
            get => _sortRule;
            set => _sortRule = value;
        }

        public bool IsOuterJoin => _isOuterJoin;

        public bool IsJoined
        {
            get => _isJoined;
            set => _isJoined = value;
        }

        #endregion

        public void CreateQuerry()
        {
            if (_isFilled)
            {

                string left = "ERROR CreateQuerryLeft";
                string right = "ERROR CreateQuerryRight";

                #region старая тема


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
                            right = _leftSelect.Name;
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
                        right = _rightSelect.Name;
                    }
                }

                if (left == "ERROR CreateQuerryLeft")
                {
                    //MessageBox.Show("WARNING in JoinStructure" + left);
                    left = "";
                }

                if (right == "ERROR CreateQuerryRight")
                {
                    //MessageBox.Show("WARNING in JoinStructure" + right);
                    right = "";
                }

                this.CreateQuerry(left, right);

            }
        }
        
        public void CreateQuerry(string left, string right)
        {
            if (_isFilled)
            {
                if (_outTable == null)
                {
                    FillTable();
                }

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
                    if (left != "" && right != "")
                    {
                        _output += left + ", " + Environment.NewLine + "\t" + right;
                    }
                    else
                    {
                        _output += left + Environment.NewLine + "\t" + right;
                    }
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
                                _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.Name + " " +
                                           _comparisonOperator +
                                           " " + addJoin.RightColumn.Name;

                            }
                            else
                            {
                                _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.Name + " " +
                                           _comparisonOperator +
                                           " " + addJoin.LeftColumn.Name;
                            }
                        }
                        else
                        {
                            if (addJoin.LeftColumn.DotTableId != null && addJoin.RightColumn.DotTableId != null)
                            {
                                if (!_switched)
                                {
                                    _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.DotTableId +
                                               addJoin.LeftColumn.Name + " " + _comparisonOperator +
                                               " " + addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;
                                }
                                else
                                {
                                    _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.DotTableId +
                                               addJoin.RightColumn.Name + " " + _comparisonOperator +
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
                                        _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.Name + " " +
                                                   _comparisonOperator +
                                                   " " + addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;

                                    }
                                    else
                                    {
                                        _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.DotTableId +
                                                   addJoin.RightColumn.Name + " " + _comparisonOperator +
                                                   " " + addJoin.LeftColumn.Name;
                                    }

                                    addJoin.RightColumn.Name =
                                        addJoin.RightColumn.DotTableId + addJoin.RightColumn.Name;
                                }

                                if (addJoin.LeftColumn.DotTableId != null)
                                {
                                    if (!_switched)
                                    {
                                        _output += Environment.NewLine + "AND\r\n\t" + addJoin.LeftColumn.DotTableId +
                                                   addJoin.LeftColumn.Name + " " + _comparisonOperator +
                                                   " " + addJoin.RightColumn.Name;
                                    }
                                    else
                                    {
                                        _output += Environment.NewLine + "AND\r\n\t" + addJoin.RightColumn.Name + " " +
                                                   _comparisonOperator +
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
                
                _output += ";";
            }
        }

        private List<ColumnStructure> RemoveSameNames(List<ColumnStructure> columns)
        {
            List<ColumnStructure> tmpColumns = new List<ColumnStructure>();
            tmpColumns.Add(columns[0]);
            bool isForAdd;
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


        public bool CheckIsFilled()
        {
            if (_leftColumnString == null || _rightColumnString == null || _comparisonOperator == null ||
                _leftColumn == null || _rightColumn == null || _leftSelect == null || _rightSelect == null)
            {
                _isFilled = false;
            }

            if (_leftSelect == null && _leftJoin != null)
            {
                _isFirst = true;
            }

            return _isFilled;
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
                            _leftJoin.IndexColumns.Add(column);
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
                    if (_rightSelect != null)
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

        public void FillTable()
        {
            int maxRows = -1;
            int left = -2;
            int right = -3;
            if (_leftJoin != null)
            {
                maxRows = _leftJoin.OutTable.RowCount;
                left = _leftJoin.OutTable.RowCount;
                //_columns.AddRange(_leftJoin.Columns);
                foreach (ColumnStructure sColumn in _leftJoin.Columns)
                {
                    _columns.Add(new ColumnStructure(sColumn));
                }
                if (_switched)
                {
                    if (_leftSelect != null)
                    {
                        //_columns.AddRange(_leftSelect.OutColumn);
                        foreach (ColumnStructure sColumn in _leftSelect.OutColumn)
                        {
                            _columns.Add(new ColumnStructure(sColumn));
                        }
                        if (maxRows <= _leftSelect.OutTable.RowCount)
                        {
                            maxRows = _leftSelect.OutTable.RowCount;
                            
                        }
                        right = _leftSelect.OutTable.RowCount;
                    }

                }
                else
                {
                    if (_rightSelect != null)
                    {
                        //_columns.AddRange(_rightSelect.OutColumn);
                        foreach (ColumnStructure sColumn in _rightSelect.OutColumn)
                        {
                            _columns.Add(new ColumnStructure(sColumn));
                        }
                        if (maxRows <= _rightSelect.OutTable.RowCount)
                        {
                            maxRows = _rightSelect.OutTable.RowCount;
                            
                        }
                        right = _rightSelect.OutTable.RowCount;
                    }
                }
            }
            else
            {
                if (_columns.Count > 0)
                {
                   // _columns = new List<ColumnStructure>();
                }
                if (_leftSelect != null)
                {
                    //_columns.AddRange(_leftSelect.OutColumn);
                    foreach (ColumnStructure sColumn in _leftSelect.OutColumn)
                    {
                        _columns.Add(new ColumnStructure(sColumn));
                    }
                    if (maxRows <= _leftSelect.OutTable.RowCount)
                    {
                        maxRows = _leftSelect.OutTable.RowCount;

                        
                    }
                    left = _leftSelect.OutTable.RowCount;
                }

                if (_rightSelect != null)
                {
                    //_columns.AddRange(_rightSelect.OutColumn);
                    foreach (ColumnStructure sColumn in _rightSelect.OutColumn)
                    {
                        _columns.Add(new ColumnStructure(sColumn));
                    }
                    if (maxRows <= _rightSelect.OutTable.RowCount)
                    {
                        maxRows = _rightSelect.OutTable.RowCount;

                        
                    }
                    right = _rightSelect.OutTable.RowCount;
                }
            }

            foreach (JoinStructure additionalJoin in _additionalJoins)
            {
                _columns.Add(new ColumnStructure(additionalJoin.LeftColumn));
                _columns.Add(new ColumnStructure(additionalJoin.RightColumn));
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
            _outTable.RowCount = maxRows;
            _outTable.RowCountLeft = left;
            _outTable.RowCountRight = right;
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

                foreach (JoinStructure addJoin in AdditionalJoins)
                {
                    if (addJoin.LeftColumn != null && addJoin.LeftColumn.Name == column.Name)
                    {
                        column.UsageCounter--;
                        addJoin.LeftColumn.UsageCounter--;
                    }

                    if (addJoin.RightColumn != null && addJoin.RightColumn.Name == column.Name)
                    {
                        column.UsageCounter--;
                        addJoin.RightColumn.UsageCounter--;
                    }
                }
            }
        }

        private void SetSortFrom()
        {
           

            if (_additionalJoins.Count > 0)
            {
                foreach (JoinStructure addJoin in _additionalJoins)
                {
                    ///ЫЫЫ кривой хардкод, переделать. Слишком много делается, а нужно только сделать метод setSortFrom
                    addJoin.CreateQuerry(_name, _name);
                    
                }
            }
        }

        public void CheckIsDistinct()
        {
            if (_isAdditional == false)
            {
                bool isDistinct = true;

                if (_leftJoin != null)
                {
                    foreach (ColumnStructure column in _leftJoin.IndexColumns)
                    {
                        if (column.IsPrimary == 1)
                        {
                            isDistinct = false;
                            break;
                        }
                    }

                    if (_switched)
                    {
                        if (_leftSelect != null)
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
                            isDistinct = false;
                        }
                    }
                    else
                    {
                        if (_rightSelect != null)
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
                        else
                        {
                            isDistinct = false;
                        }
                    }
                }
                else
                {
                    List<ColumnStructure> possIndexColumns = new List<ColumnStructure>();
                    possIndexColumns.AddRange(_leftSelect.IndexColumns);
                    possIndexColumns.AddRange(_rightSelect.IndexColumns);
                    foreach (JoinStructure additionalJoin in _additionalJoins)
                    {
                        possIndexColumns.AddRange(additionalJoin.LeftSelect.IndexColumns);
                        possIndexColumns.AddRange(additionalJoin.RightSelect.IndexColumns);
                    }

                    int keyLenght = -1;
                    string keyTableName = "";
                    foreach (ColumnStructure column in possIndexColumns)
                    {
                        if (column.IsPrimary == 1)
                        {
                            isDistinct = false;
                            break;
                        }

                        if (column.IsPrimary > 1 && keyLenght == -1)
                        {
                            keyLenght = column.IsPrimary;
                            keyTableName = column.Table.Name;
                        }

                        if (column.IsPrimary > 1 && column.Table.Name == keyTableName)
                        {
                            keyLenght--;
                        }
                    }

                    if (keyLenght == 0)
                    {
                        isDistinct = false;
                    }
                }



                if (isDistinct && _isAdditional == false)
                {
                    _output = _output.Insert(6, " DISTINCT");
                }


                List<ColumnStructure> joiningColumns = new List<ColumnStructure>();

                foreach (JoinStructure additionalJoin in AdditionalJoins)
                {
                    joiningColumns.Add(additionalJoin.LeftColumn);
                    joiningColumns.Add(additionalJoin.RightColumn);
                }

                joiningColumns.Add(_leftColumn);
                joiningColumns.Add(_rightColumn);

                bool isPrimarykey = false;

                foreach (ColumnStructure columnStructure in joiningColumns)
                {
                    if (columnStructure.IsPrimary == 1)
                    {
                        isPrimarykey = true;
                        break;
                    }
                }

                if (!isPrimarykey && _isAdditional == false)
                {
                    if (_leftJoin != null)
                    {
                        if (!_switched)
                        {
                            if (_rightSelect != null)

                                _rightSelect.CheckForDistinct();
                        }
                        else
                        {
                            if (_leftSelect != null)
                                _leftSelect.CheckForDistinct();
                        }
                    }
                    else
                    {
                        _rightSelect.CheckForDistinct();
                        _leftSelect.CheckForDistinct();
                    }
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

        public void ChangeSort()
        {

            //Вызывать метот после всех оптимизаций, для корректного заполнения SORT запроса
            _sortRule.GetRuleBySourceInterval(_sourceInterval).Text = "";
            _sortRule.GetRuleBySourceInterval(_sourceInterval).IsRealised = true;
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

            foreach (JoinStructure additionalJoin in AdditionalJoins)
            {
                additionalJoin.ChangeSort();
            }
        }
    }
}
