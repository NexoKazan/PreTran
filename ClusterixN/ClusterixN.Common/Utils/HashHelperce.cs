#region Copyright
/*
 * Copyright 2018 Roman Klassen
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
using System.Linq;
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.Hasher;
using ClusterixN.Common.Utils.LogServices;
using ClusterixN.Common.Utils.Task;

namespace ClusterixN.Common.Utils
{
    public class HashHelper
    {
        private static HashHelper _hashHelper;
        private static string _loggerName = "NA";
        private readonly ILogger _logger;
        private readonly IHasher _hasher;
        private readonly ParallelQeueHelper _qeueHelper;
        private static int[] _queueNumbers;

        public static HashHelper Instance => _hashHelper ?? (_hashHelper = new HashHelper());

        public static void Initialize(string logger)
        {
            _loggerName = logger;
            var queueBind = ServiceLocator.Instance.ConfigurationService.GetAppSetting("HashQueueBind").Split(',');
            _queueNumbers = queueBind.Select(int.Parse).ToArray();
        }

        private HashHelper()
        {
            _logger = ServiceLocator.Instance.LogService.GetLogger(_loggerName);
            _hasher = ServiceLocator.Instance.HashService;
            _qeueHelper = new ParallelQeueHelper(_queueNumbers);
        }

        public void HashDataAsync(Relation relation, byte[] data, int hashCount, Action<List<byte[]>, object> callback,
            object callbackParams)
        {
            _qeueHelper.AddToQueue(new QueueTask((i, obj) =>
                {
                    var tup = (Tuple<Relation, byte[], int, Action<List<byte[]>, object>, object>) obj;
                    var hashedData = HashData(tup.Item1, tup.Item2, tup.Item3, i);
                    tup.Item4.Invoke(hashedData, tup.Item5);
                },
                new Tuple<Relation, byte[], int, Action<List<byte[]>, object>, object>(relation, data, hashCount,
                    callback, callbackParams)));
        }

        private List<byte[]> HashData(Relation relation, byte[] data, int hashCount, int queueNumber)
        {
            _logger.Trace($"Хеширование данных для {relation.RelationId}");
            var hashTime = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.HashData, Guid.Empty,
                Guid.Empty,
                relation.RelationId);

            var keyIndexes = new List<int>();
            var colIndex = 0;
            foreach (var field in relation.Shema.Fields)
            {
                if (relation.Shema.Indexes.Any(
                    index => index.FieldNames.Any(fieldIndex => fieldIndex == field.Name)))
                {
                    keyIndexes.Add(colIndex);
                }
                colIndex++;
            }

            List<byte[]> result;
            var gpuHashHelper = _hasher as GpuHashHelper;
            if (gpuHashHelper != null)
            {
                result = gpuHashHelper.ProcessData(data, hashCount,
                    keyIndexes.ToArray(), queueNumber);
            }
            else
            {
                result = _hasher.ProcessData(data, hashCount, keyIndexes.ToArray());
            }

            hashTime.Stop();

            return result;
        }
    }
}
