#region Copyright
/*
 * Copyright 2017 Roman Klassen
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

using ClusterixN.Common.Utils;

namespace ClusterixN.Database.MySQL
{
    public class ConnectionStringParser : ConnectionStringParserBase
    {
        static readonly string[] Database = { "Database" };

        public static string GetDatabase(string connectionString)
        {
            return GetValue(connectionString, Database);
        }
    }
}
