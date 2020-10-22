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

using Antlr4.Runtime.Misc;
using PreTran.TestClasses.Listeners;

namespace PreTran.Q_Part_Structures
{
    public enum PredicateType : int
    {
        simple = 1,
        join = 2,
        subQ = 3,
        select = 4
    }

    class BinaryComparisionPredicateStructure
    {
        private int _type; // 1-simple predicate, 2-join predicate, 3-sub q predicate;

        private int _subQID;

        private string _leftType;
        private string _rightType;

        private string _leftString;
        private string _rightString;

        private string _comparisionSymphol;

        private Interval _sourceInterval;

        public BinaryComparisionPredicateStructure(string leftString, string comparisionSymphol, string rightString, Interval sourceInterval)
        {
            _leftString = leftString;
            _rightString = rightString;
            _comparisionSymphol = comparisionSymphol;
            _sourceInterval = sourceInterval;
        }

        public int Type
        {
            get => _type;
            set => _type = value;
        }

        public string LeftType
        {
            get => _leftType;
            set => _leftType = value;
        }

        public string RightType
        {
            get => _rightType;
            set => _rightType = value;
        }

        public string LeftString
        {
            get => _leftString;
            set => _leftString = value;
        }

        public string RightString
        {
            get => _rightString;
            set => _rightString = value;
        }

        public string ComparisionSymphol
        {
            get => _comparisionSymphol;
            set => _comparisionSymphol = value;
        }

        public int SubQid
        {
            get { return _subQID; }
            set { _subQID = value; }
        }

        public Interval SourceInterval { get => _sourceInterval; set => _sourceInterval = value; }
    }
}
