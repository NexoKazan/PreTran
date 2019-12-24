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
using System.IO;
using System.Threading.Tasks;
 using ClusterixN.Common;
 using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.Task;
using ClusterixN.Network.Interfaces;
using ClusterixN.QueryProcessing.Data;
using ClusterixN.QueryProcessing.Data.EventArgs;
using ClusterixN.QueryProcessing.Managers;
using ClusterixN.QueryProcessing.Services.Interfaces;
using Relation = ClusterixN.QueryProcessing.Data.Relation;

namespace ClusterixN.QueryProcessing.Services.Base
{
    abstract class RelationServiceBase : QueryProcessingServiceBase, IRelationService
    {
        protected readonly RelationManager RelationManager;
        protected readonly LoadStatusManager LoadStatusManager;
        protected readonly TaskSequenceLoadManager TaskSequenceLoadManager;
        protected readonly TaskSequenceHelper DeleteSequenceHelper;
        private readonly bool _indexAfterLoad;

        protected RelationServiceBase(ICommunicator client, RelationManager relationManager, QueryProcessConfig dbConfig,
            LoadStatusManager loadStatusManager, TaskSequenceLoadManager taskSequenceLoadManager) : base(client, dbConfig)
        {
            _indexAfterLoad = ServiceLocator.Instance.ConfigurationService.GetAppSetting("IndexRelationsAfterLoad") == "1";
            RelationManager = relationManager;
            LoadStatusManager = loadStatusManager;
            LoadStatusManager.LoadCompleteEvent += LoadStatusManagerOnLoadCompleteEvent;
            TaskSequenceLoadManager = taskSequenceLoadManager;
            DeleteSequenceHelper = new TaskSequenceHelper();
        }

        private void LoadStatusManagerOnLoadCompleteEvent(object sender, LoadCompleteEventArg loadCompleteEventArg)
        {
            var relation = RelationManager.GetRelation(loadCompleteEventArg.RelationId);
            if (relation!=null)
                LoadComplete(relation, loadCompleteEventArg);
            else 
                Logger.Error($"Отношение {loadCompleteEventArg.RelationId} не найдено");
        }

        protected string GetDirectoryName()
        {
            return Config.DataDir + Path.DirectorySeparatorChar;
        }

        public void IndexRelation(string relationName, List<Index> indexes, IDatabase database)
        {
            if (_indexAfterLoad) return;

            foreach (var index in indexes)
            {
                if (index.IsPrimary)
                {
                    database.AddPrimaryKey(relationName, index.FieldNames.ToArray());
                }
                else
                {
                    database.AddIndex(index.Name, relationName, index.FieldNames.ToArray());
                }
            }
        }

        public void IndexRelationAfterLoad(string relationName, List<Index> indexes, IDatabase database)
        {
            if (!_indexAfterLoad) return;

            foreach (var index in indexes)
            {
                if (index.IsPrimary)
                {
                    database.AddPrimaryKey(relationName, index.FieldNames.ToArray());
                }
                else
                {
                    database.AddIndex(index.Name, relationName, index.FieldNames.ToArray());
                }
            }
        }
        
        protected virtual void LoadComplete(Relation relation, LoadCompleteEventArg loadCompleteEventArg)
        {
            RelationManager.SetRelationStatus(relation.RelationId, RelationStatus.DataTransfered);
            Logger.Trace($"Загрузка данных для {relation.RelationName} завершена");
        }
        
        public void DropRealtion(Relation relation)
        {
            if (Config.SyncQueryDrop)
            {
                DropRealtion((object)relation);
            }
            else
            {
                DeleteSequenceHelper.AddTask(new Task(DropRealtion, relation));
            }
        }

        protected abstract void DropRealtion(object o);

        public Relation GetRelation(Guid relationId)
        {
            return RelationManager.GetRelation(relationId);
        }

        protected override void Dispose(bool disposing)
        {
            DeleteSequenceHelper.Dispose();
            base.Dispose(disposing);
        }

        public void SetRelationStatus(Guid relationId, RelationStatus relationStatus)
        {
            RelationManager.SetRelationStatus(relationId, relationStatus);
        }

        public Relation[] GetRelations(Guid[] relationIds)
        {
            return RelationManager.GetRelations(relationIds);
        }

        public void AddRelation(Relation newRelation)
        {
            RelationManager.AddRelation(newRelation);
        }
        
        public void Pause(bool pause)
        {
            IsPaused = pause;
        }

        public void CancelQuery(Guid queryId, Guid subQueryId, Guid relationId)
        {
            var relation = (RelationManager.GetRelation(relationId) ?? RelationManager.GetRelation(subQueryId)) ??
                           RelationManager.GetRelation(queryId);
            if (relation!=null)
            {
                TaskSequenceLoadManager.Remove(relation.RelationId);
                LoadStatusManager.CancelLoad(relation.RelationId);
                DropRealtion(relation);
                foreach (var file in Directory.EnumerateFiles(Config.DataDir, relation.RelationId + "*"))
                {
                    File.Delete(file);
                }
                RelationManager.RemoveRelation(relation.RelationId);
            }
        }

        protected override void OnIsPausedChanged(bool isPaused)
        {
            //не требуется
        }
    }
}

