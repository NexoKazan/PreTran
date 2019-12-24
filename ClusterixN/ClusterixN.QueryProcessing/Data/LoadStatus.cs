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
using ClusterixN.Common;
using ClusterixN.Common.Interfaces;
using ClusterixN.QueryProcessing.Data.Enums;

namespace ClusterixN.QueryProcessing.Data
{
    class LoadStatus
    {
        public bool IsLastPacketReceived
        {
            get
            {
                lock (_syncObject)
                {
                    return _lastPacketReceivedNumbersList.Count == _nodeCount;
                }
            }
        }

        public int NodeCount
        {
            get { return _nodeCount; }
        }

        private readonly List<int> _lastPacketReceivedNumbersList;

        private readonly object _syncObject = new object();

        private readonly int _nodeCount;
        private Dictionary<Guid, Tuple<LoadStatusEnum, int>> Statuses { get; set; }
        private ILogger _logger;

        public LoadStatus(int nodeCount)
        {
            _nodeCount = nodeCount;
            Statuses = new Dictionary<Guid, Tuple<LoadStatusEnum, int>>();
            _lastPacketReceivedNumbersList = new List<int>();
            _logger = ServiceLocator.Instance.LogService.GetLogger("defaultLogger");
        }
        
        public void SetStatus(Guid taskId, LoadStatusEnum status, int orderNumber, bool isLast = false)
        {
            lock (_syncObject)
            {
                if (Statuses.ContainsKey(taskId))
                {
                    Statuses[taskId] = new Tuple<LoadStatusEnum, int>(status, orderNumber);
                }
                else
                {
                    Statuses.Add(taskId, new Tuple<LoadStatusEnum, int>(status, orderNumber));
                }
                if (isLast) _lastPacketReceivedNumbersList.Add(orderNumber);
            }
        }

        public bool CheckStatuses(LoadStatusEnum status)
        {
            lock (_syncObject)
            {
                return Statuses.Values.All(s => s.Item1 == status);
            }
        }

        private List<int> GetPacketCounts(List<int> lastNumbers)
        {
            var packetsCount = new List<int>();
            var packetCounter = lastNumbers.Count;
            var counter = lastNumbers.Count;

            for (int i = 0; i < lastNumbers.Count - 1; i++)
            {
                packetsCount.Add(counter);
                packetCounter--;
                if (lastNumbers[i] != lastNumbers[i + 1])
                {
                    counter = packetCounter;
                }
            }

            packetsCount.Add(counter); // для последнего пакета
            return packetsCount;
        }

        public bool CheckOrder()
        {
            if (!IsLastPacketReceived) return false;

            lock (_syncObject)
            {
                var groupedOrdersCount = Statuses.Values.Select(s => s.Item2).GroupBy(g => g).ToDictionary(keys => keys.Key, ints => ints.Count());
                var lastNumbers = _lastPacketReceivedNumbersList.OrderBy(l => l).ToList();
                var packetsCount = GetPacketCounts(lastNumbers);
                var nextNumberCheck = 0;

                for (var i = 0; i < lastNumbers.Count; i++)
                {
                    for (int j = nextNumberCheck; j <= lastNumbers[i]; j++)
                    {
                        //пакета не хватает
                        if (!groupedOrdersCount.ContainsKey(j))
                        {
                            _logger.Warning($"Не получен пакет под номером {j}");
                            return false;
                        }

                        //пакет получен не от всех узлов или пакетов получено больше, чем последних пакетов
                        if (groupedOrdersCount[j] != packetsCount[i])
                        {
                            _logger.Warning($"Количество полученных пакетов не соответствует ожидаемому {groupedOrdersCount[j]}!={packetsCount[i]}");
                            return false;
                        }
                    }

                    nextNumberCheck = lastNumbers[i]+1;
                }

                _logger.Info("Проверка целостности переданных данных прошла успешно");
                return true;
            }
        }
    }
}
