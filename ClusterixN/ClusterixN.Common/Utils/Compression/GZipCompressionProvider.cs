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

using System.IO;
using System.IO.Compression;
using ClusterixN.Common.Interfaces;

namespace ClusterixN.Common.Utils.Compression
{
    public class GZipCompressionProvider : ICompressionProvider
    {

        public void Dispose()
        {

        }

        public byte[] CompressBytes(byte[] data)
        {
            var compressedBytes = new byte[0];

            using (var compressedStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(compressedStream,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(data,0, data.Length);
                }
                compressedBytes = compressedStream.ToArray();
            }

            return compressedBytes;
        }

        public byte[] DecompressBytes(byte[] compressedData)
        {
            var decompressedBytes = new byte[0];

            using (var decompressedStream = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(compressedData))
                {
                    using (var decompressionStream = new GZipStream(compressedStream,
                        CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                    }
                }
                decompressedBytes = decompressedStream.ToArray();
            }

            return decompressedBytes;
        }
    }
}
