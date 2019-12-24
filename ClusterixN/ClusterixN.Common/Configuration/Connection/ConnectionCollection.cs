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

﻿using System.Configuration;

namespace ClusterixN.Common.Configuration.Connection
{
    [ConfigurationCollection(typeof(ConnectionElement))]
    public class ConnectionsCollection : ConfigurationElementCollection
    {
        public ConnectionElement this[int idx] => (ConnectionElement) BaseGet(idx);

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new ConnectionElement();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((ConnectionElement) element).Name;
        }
    }
}