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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;

namespace ClusterixN.Manager.Managers
{
    internal class NodesManager
    {
        private readonly Dictionary<Guid, Node> _nodes;
        private readonly object _syncObject = new object();

        public NodesManager()
        {
            _nodes = new Dictionary<Guid, Node>();
        }

        public void AddNode(Node node)
        {
            lock (_syncObject)
            {
                _nodes.Add(node.Id, node);
            }
        }

        public void RemoveNode(Guid nodeId)
        {
            lock (_syncObject)
            {
                if (_nodes.ContainsKey(nodeId))
                    _nodes.Remove(nodeId);
            }
        }

        public Node GetNode(Guid nodeId)
        {
            lock (_syncObject)
            {
                if (_nodes.ContainsKey(nodeId))
                    return _nodes[nodeId];

                return null;
            }
        }

        public List<Node> GetFreeNodes(NodeType nodeType)
        {
            lock (_syncObject)
            {
                return _nodes.Select(n => n.Value).Where(n => n.NodeType == nodeType && n.CanSendQuery).ToList();
            }
        }

        public List<Node> GetNodes()
        {
            lock (_syncObject)
            {
                return _nodes.Select(n => n.Value).ToList();
            }
        }

        public List<Node> GetNodes(NodeType nodeType)
        {
            lock (_syncObject)
            {
                return _nodes.Select(n => n.Value).Where(n => n.NodeType == nodeType).ToList();
            }
        }
    }
}