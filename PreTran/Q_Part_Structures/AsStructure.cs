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

using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;

namespace PreTran.Q_Part_Structures
{
    class AsStructure
    {
        private string _asRightName;
        private string _tableName;
        private string _clearString; //переименовать
        private string _asString;
        private string _functionString;
        private string _aggregateFunctionName;
        private bool _isSelectPart = false;
        private bool _isSingleColumn = false;
        private bool _isSortPart = false;
        private ColumnStructure[] _asColumns;
        private ColumnStructure _asRightColumn;
        private Interval _sourceInterval;
        private List<string> _asColumnNames;
        private List<TableStructure> _asTables;

        public AsStructure(List<string> asColumnsNames, string asString, string functionString, string asRightName, string aggregateFunctionName, Interval sourceInterval) 
        {
            _asColumnNames = asColumnsNames;
            _clearString = asString;
            _asRightName = asRightName;
            _functionString = functionString;
            _asString = asString;
            _aggregateFunctionName = aggregateFunctionName;
            _sourceInterval = sourceInterval;
        }

        public string OldTableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public string AsString
        {
            get { return _asString; }
        }

        public string GetAsRightName
        {
            get { return _asRightName; }
        }

        public string AggregateFunctionName
        {
            get { return _aggregateFunctionName; }
        }

        public bool IsSelectPart
        {
            get { return _isSelectPart; }
            set { _isSelectPart = value; }
        }

        public bool IsSortPart
        {
            get
            {
                CheckIsSortPart();
                return _isSortPart;
            }
            set { _isSortPart = value; }
        }

        public bool IsSingleColumn
        {
            get { return _isSingleColumn; }
            set { _isSingleColumn = value; }
        }

        public ColumnStructure AsRightColumn
        {
            get { return _asRightColumn; }
            set { _asRightColumn = value; }
        }

        public Interval SourceInterval
        {
            get { return _sourceInterval; }
        }

        public List<string> ColumnNames
        {
            get { return _asColumnNames; }
        }

        public List<TableStructure> Tables
        {
            get { return _asTables; }
            set { _asTables = value; }
        }

        public ColumnStructure[] AsColumns
        {
            get { return _asColumns; }
            set { _asColumns = value; }
        }

        private bool CheckIsSortPart()
        {
            bool output = false;
            if ( _aggregateFunctionName != "EXTRACT" && (_asColumns.Length > 1 && _asColumns.Count(x=>x.UsageCounter > 1) > 1 || (_asColumns.Length == 1 && _asColumns[0].UsageCounter > 1) ) )
            {
                output = true;
            }

            _isSortPart = output;
            return output;
        }
    }
}
