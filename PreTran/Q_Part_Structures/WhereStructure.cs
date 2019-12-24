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
using System.Text;
using System.Threading.Tasks;
using MySQL_Clear_standart.DataBaseSchemeStructure;

namespace MySQL_Clear_standart
{   // добавить структуры БД
    class WhereStructure
    {
        private string _string; //переименовать!
        private string _leftColumn;
        private string _rightExpr;
        private string _table;
        private string _comparisionOperator;
        private ColumnStructure _column;


        public WhereStructure(string fullString, string leftColumn)
        {
            _string = fullString;
            _leftColumn = leftColumn;
        }

        public WhereStructure(string leftColumn, string comparisionOperator, string rightColumn)
        {
            _comparisionOperator = comparisionOperator;
            _leftColumn = leftColumn;
            _rightExpr = rightColumn;
            _string = _leftColumn + " " + _comparisionOperator + " " + _rightExpr;
        }

        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public string getWhereString
        {
            get { return _string; }
        }

        public string LeftColumn
        {
            get { return _leftColumn; }
        }

        public string RightExpr
        {
            get { return _rightExpr; }
            set { _rightExpr = value; }
        }

        public string ComparisionOperator
        {
            get { return _comparisionOperator; }
            set { _comparisionOperator = value; }
        }

        public ColumnStructure Column
        {
            get { return _column; }
            set { _column = value; }
        }
    }
}
