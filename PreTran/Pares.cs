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

namespace PreTran
{
    class Pares
    {
        private string _left;
        private string _right;
        private bool _isForDelete;

        public Pares(string left, string right)
        {
            _left = left;
            _right = right;
            _isForDelete = false;
        }

        public string Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public string Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public bool IsForDelete
        {
            get { return _isForDelete; }
            set { _isForDelete = value; }
        }
    }
}
