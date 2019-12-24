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
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MySQL_Clear_standart.DataBaseSchemeStructure;

namespace MySQL_Clear_standart
{
    class SchemeCreator
    {
        private string _return = "Return:\r\n";
        private DataBaseStructure _inDatabase;
        private List<SelectStructure> _inSelectStructures;

        #region Propirties

        public string Return { get; }
        #endregion

        #region Constructors

        public SchemeCreator(DataBaseStructure inDataBase, List<SelectStructure> inSelectStructures)
        {
            _inDatabase = inDataBase;
            _inSelectStructures = inSelectStructures;
        }

        #endregion

        public void TestMethod()
        {
            foreach (SelectStructure inSelectStructure in _inSelectStructures)
            {
                _return += inSelectStructure;
            }
        }
    }
}
