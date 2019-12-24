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
using ClusterixN.Common;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Interfaces;
using ClusterixN.QueryProcessing.Data;

namespace ClusterixN.QueryProcessing.Managers
{
    public class RelationManager
    {
        private readonly Dictionary<Guid, Relation> _relations;
        private readonly object _syncObject = new object();
        private readonly ILogger _logger;

        public RelationManager()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
            _logger.Trace("Инициализация менеджера отношений");
            _relations = new Dictionary<Guid, Relation>();
        }

        public void AddRelation(Relation relation)
        {
            lock (_syncObject)
            {
                if (_relations.ContainsKey(relation.RelationId))
                    _relations[relation.RelationId] = relation;
                else
                    _relations.Add(relation.RelationId, relation);
            }
        }

        public Relation GetRelation(Guid relationId)
        {
            lock (_syncObject)
            {
                if (_relations.ContainsKey(relationId)) return _relations[relationId];
            }
            return null;
        }

        public Relation[] GetRelations(Guid[] relationIds)
        {
            lock (_syncObject)
            {
                var relations = new List<Relation>();

                foreach (var relationId in relationIds)
                {
                    relations.Add(_relations.ContainsKey(relationId) ? _relations[relationId] : null);
                }
                return relations.ToArray();
            }
        }

        public void SetRelationStatus(Guid relationId, RelationStatus status)
        {
            lock (_syncObject)
            {
                if (_relations.ContainsKey(relationId))
                {
                    var relation = _relations[relationId];
                    if (relation.Status > status)
                    {
                        _logger.Error($"Отношение {relationId} уже получило более высокий статус ");
                    }
                    else
                    {
                        relation.Status = status;
                    }
                }
                else _logger.Error($"Отношение {relationId} не найдено");
            }
        }

        public void RemoveRelation(Guid relationId)
        {
            lock (_syncObject)
            {
                if (_relations.ContainsKey(relationId)) _relations.Remove(relationId);
                else _logger.Error($"Отношение {relationId} не найдено");
            }
        }
    }
}