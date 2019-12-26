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

using PreTran.DataBaseSchemeStructure;

namespace PreTran.Q_Part_Structures
{
    class LikeStructure
    {
        private string _columnName;
        private string _rightExpression;
        private bool _isNot = false;
        private TableStructure _table;
        private ColumnStructure _leftColumn;

        public LikeStructure(string rightExpression, string columnName)
        {
            _rightExpression = rightExpression;
            _columnName = columnName;
        }
        
        public string RightExpression
        {
            get { return _rightExpression; }
        }

        public string ColumnName
        {
            get { return _columnName; }
        }

        public bool IsNot
        {
            get { return _isNot; }
            set { _isNot = value; }
        }

        public TableStructure Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public ColumnStructure LeftColumn
        {
            get { return _leftColumn; }
            set { _leftColumn = value; }
        }
    }
}
