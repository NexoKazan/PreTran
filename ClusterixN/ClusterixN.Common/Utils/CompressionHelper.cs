#region Copyright
/*
 * Copyright 2019 Roman Klassen
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
using ClusterixN.Common.Data.Log.Enum;
using ClusterixN.Common.Interfaces;
using ClusterixN.Common.Utils.LogServices;

namespace ClusterixN.Common.Utils
{
    public class CompressionHelper
    {
        private readonly ILogger _loger;

        public CompressionHelper(ILogger loger)
        {
            _loger = loger;
        }

        public byte[] CompressData(byte[] data, Guid queryId, Guid subQueryId, Guid relationId, string info = "")
        {
            var infoSrt =
                $"{(queryId != Guid.Empty ? queryId.ToString() : "")} {(subQueryId != Guid.Empty ? subQueryId.ToString() : "")} {(relationId != Guid.Empty ? relationId.ToString() : "")} {info}";
            _loger.Trace($"Запущено сжатие данных для {infoSrt}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.Compression, queryId, subQueryId,
                relationId);

            var originalSize = data.Length;
            byte[] compressedData;
            using (var compression = ServiceLocator.Instance.CompressionService)
            {
                compressedData = compression.CompressBytes(data);
            }
            var compressedSize = compressedData.Length;

            timeLog.Stop();
            _loger.Trace($"Завершено сжатие данных для {infoSrt} за {timeLog.Duration} мс");
            _loger.Trace($"Степень сжатия данных для {infoSrt} \t{originalSize/(double)compressedSize:N}\t {originalSize / 1024.0 / 1024.0:N} MB / {compressedSize / 1024.0 / 1024.0:N} MB");

            return compressedData;
        }

        public byte[] DecompressData(byte[] data, Guid queryId, Guid subQueryId, Guid relationId, string info = "")
        {
            var infoSrt =
                $"{(queryId != Guid.Empty ? queryId.ToString() : "")} {(subQueryId != Guid.Empty ? subQueryId.ToString() : "")} {(relationId != Guid.Empty ? relationId.ToString() : "")} {info}";
            _loger.Trace($"Запущено восстановление данных для {infoSrt}");
            var timeLog = TimeLogService.Instance.GeTimeLogHelper(MeasuredOperation.Decompression, queryId, subQueryId,
                relationId);

            var originalSize = data.Length;
            byte[] decompressedData;
            using (var compression = ServiceLocator.Instance.CompressionService)
            {
                decompressedData = compression.DecompressBytes(data);
            }
            var decompressedSize = decompressedData.Length;

            timeLog.Stop();
            _loger.Trace($"Завершено восстановление данных для {infoSrt} за {timeLog.Duration} мс {originalSize / 1024.0 / 1024.0:N} MB / {decompressedSize / 1024.0 / 1024.0:N} MB");

            return decompressedData;
        }
    }
}
