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
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils;
using ClusterixN.Common.Utils.LogServices;

namespace ClusterixN.Common.Data.Query.Base
{
    /// <summary>
    /// Запрос с измерением времени переключения между статусами
    /// </summary>
    public class TimeMeasureQueryBase : QueryBase, ITimeMeasured
    {
        private QueryStatus _status;
        private readonly TimeMeasureHelper _timeMeasureHelper;
        private readonly List<TimeMeasure> _timeMeasures;
        private TimeLogHelper _timeLog;

        /// <summary>
        /// Запрос с измерением времени переключения между статусами
        /// </summary>
        protected TimeMeasureQueryBase()
        {
            _timeMeasureHelper = new TimeMeasureHelper();
            _timeMeasures = new List<TimeMeasure>();
            _timeMeasureHelper.Start();
            _timeLog =
                TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.WaitStart, Guid.Empty, QueryId, Guid.Empty);
        }

        /// <summary>
        /// Статус
        /// </summary>
        public new QueryStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    MeasureTime(_status, value);
                    _status = value;
                }
            }
        }

        private void MeasureTime(QueryStatus oldStatus, QueryStatus newStatus)
        {
            _timeMeasureHelper.Stop();
            _timeMeasures.Add(new TimeMeasure($"{oldStatus} -> {newStatus}", _timeMeasureHelper.Elapsed));

            switch (Status)
            {
                case QueryStatus.Wait:
                    break;
                case QueryStatus.SelectProcessing:
                    _timeLog.Stop();
                    break;
                case QueryStatus.TransferSelectResult:
                    break;
                case QueryStatus.SelectProcessed:
                    _timeLog =
                        TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.WaitJoin, Guid.Empty, QueryId, Guid.Empty);
                    break;
                case QueryStatus.TransferToJoin:
                    _timeLog.Stop();
                    break;
                case QueryStatus.JoinProcessing:
                    break;
                case QueryStatus.JoinProcessed:
                    break;
                case QueryStatus.TransferJoinResult:
                    break;
                case QueryStatus.JoinResultTransfered:
                    _timeLog =
                        TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.WaitSort, Guid.Empty, QueryId, Guid.Empty);
                    break;
                case QueryStatus.TransferToSort:
                    _timeLog.Stop();
                    break;
                case QueryStatus.TransferedToSort:
                    break;
                case QueryStatus.ProcessingSort:
                    break;
                case QueryStatus.SortProcessed:
                    break;
                case QueryStatus.TransferSortResult:
                    break;
                case QueryStatus.SortResultTransfered:
                    break;
                case QueryStatus.Ready:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            _timeMeasureHelper.Start();
        }

        /// <summary>
        /// Получить измеренные времена по статусам
        /// </summary>
        /// <returns>Список измерений</returns>
        public List<TimeMeasure> GetTimeMeasures()
        {
            return _timeMeasures;
        }

        /// <summary>
        /// Очистить измерения
        /// </summary>
        public void ClearTimeMeasures()
        {
            _timeMeasures.Clear();
        }

        /// <summary>
        /// Получить время работы текущего статуса
        /// </summary>
        public TimeSpan CurrentStatusDuration => _timeMeasureHelper.CurrentTime;
    }
}