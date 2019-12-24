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
using ClusterixN.Common.Data.Enums;

namespace ClusterixN.Common.Data
{
    /// <summary>
    /// Узел
    /// </summary>
    public class Node
    {
        private readonly bool _useHardDisk;
        private readonly float _minRamAvaible;
        private int _queryInProgress;

        /// <summary>
        /// Узел
        /// </summary>
        /// <param name="id">Идентификатор узла</param>
        /// <param name="useHardDisk">Узел использует диск</param>
        /// <param name="minRamAvaible">Минимальный объем свободной памяти в узле для его работы</param>
        public Node(Guid id, bool useHardDisk, float minRamAvaible)
        {
            Id = id;
            _useHardDisk = useHardDisk;
            _minRamAvaible = minRamAvaible;
            QueriesInProgress = new List<Query.Query>();
            RamAvaible = int.MaxValue; //по умолчанию памяти достаточно
        }

        /// <summary>
        /// Уникальный идентификатор узла
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Количество запросов в работе на текущий момент
        /// </summary>
        public int QueryInProgress 
        {
            get { return _queryInProgress; }
            set
            {
                _queryInProgress = value;
                if (_queryInProgress < 0) _queryInProgress = 0;
            }
        }

        /// <summary>
        /// Количество ядер узла
        /// </summary>
        public int CpuCount { get; set; }

        /// <summary>
        /// Узел может принят очередной запрос
        /// </summary>
        public bool CanSendQuery => CheckSendPossibility(_minRamAvaible);

        /// <summary>
        /// В узел можно передать данные для отношения
        /// </summary>
        public bool CanTransferDataQuery => RamAvaible > _minRamAvaible;

        /// <summary>
        /// Запросы в работе
        /// </summary>
        public List<Query.Query> QueriesInProgress { get; set; }

        /// <summary>
        /// Тип узла
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// Узел предназначен для сложных запросов
        /// </summary>
        public bool IsForHardQueries { get; set; }

        /// <summary>
        /// Время последней отправки запроса в узел
        /// </summary>
        public DateTime LastSendTime { get; set; }

        /// <summary>
        /// Текущий объем свободной оперативной памяти
        /// </summary>
        public float RamAvaible { get; set; }

        /// <summary>
        /// Объем памяти, который будет занят после передачи отношения
        /// </summary>
        public float PendingRam { get; set; }

        /// <summary>
        /// Текущая нагрузка на процессор
        /// </summary>
        public float CpuUsage { get; set; }

        /// <summary>
        /// Порт сервера узла
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Статус узла
        /// </summary>
        public uint Status
        {
            get
            {
                uint status = 0;
                if (RamAvaible <= _minRamAvaible && !_useHardDisk) status |= (uint)NodeStatus.LowMemory;
                if (QueryInProgress >= CpuCount) status |= (uint)NodeStatus.Full;
                if (status == 0) status |= (uint)NodeStatus.Ok;
                return status;
            }
        }

        private bool CheckSendPossibility(float minRamAvaible)
        {
            return QueryInProgress < CpuCount && (RamAvaible > minRamAvaible || _useHardDisk);
        }

        /// <summary>
        /// В узел можно передать данные для отношения 
        /// </summary>
        /// <param name="dataVolume">Объем отношения для передачи</param>
        /// <returns>Возможность передачи данных</returns>
        public bool CanSendData(float dataVolume = 0f)
        {
            return RamAvaible - _minRamAvaible - (PendingRam) > dataVolume || _useHardDisk;
        }
    }
}