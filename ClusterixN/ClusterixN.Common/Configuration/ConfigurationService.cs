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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Configuration
{
    internal class ConfigurationService : IConfigurationService
    {
        public string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public string GetPathToConfiguration()
        {
            return Assembly.GetExecutingAssembly().Location + ".config";
        }

        public string[] GetAllConnetctionStrings()
        {
            var list = new List<string>();
            foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
            {
                list.Add(connectionString.ConnectionString);
            }

            return list.ToArray();
        }

        public string[] GetConnetctionStrings(Func<string,bool> nameConstrains)
        {
            var list = new List<string>();
            foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
            {
                if (nameConstrains.Invoke(connectionString.Name))
                {
                    list.Add(connectionString.ConnectionString);
                }
            }

            return list.ToArray();
        }
    }
}