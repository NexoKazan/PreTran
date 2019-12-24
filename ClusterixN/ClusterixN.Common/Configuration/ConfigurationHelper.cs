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
using ClusterixN.Common.Configuration.Connection;
using ClusterixN.Common.Configuration.Listener;

namespace ClusterixN.Common.Configuration
{
    public class ConfigurationHelper
    {
        public ListenerElement GetListenerConfiguration(string name)
        {
            var section = (ListenersConfigSection) ConfigurationManager.GetSection("ListenersConfiguration");

            if (section != null)
                for (var i = 0; i < section.ListenerItems.Count; i++)
                    if (section.ListenerItems[i].Name.Equals(name)) return section.ListenerItems[i];

            return null;
        }

        public ConnectionElement GetConnectionConfiguration(string name)
        {
            var section = (ConnectionsConfigSection) ConfigurationManager.GetSection("ConnectionsConfiguration");

            if (section != null)
                for (var i = 0; i < section.ConnectionItems.Count; i++)
                    if (section.ConnectionItems[i].Name.Equals(name)) return section.ConnectionItems[i];

            return null;
        }
    }
}