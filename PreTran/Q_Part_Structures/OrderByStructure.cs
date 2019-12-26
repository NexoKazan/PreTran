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
    class OrderByStructure
    {   // сделать конструктор
        private ColumnStructure _column;
        private string _columnName;
        private bool _isDESC = false;

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public bool IsDESC  
        {
            get { return _isDESC; }
            set { _isDESC = value; }
        }

        public ColumnStructure Column
        {
            get { return _column; }
            set { _column = value; }
        }
    }
}
