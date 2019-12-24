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
using System.IO;

namespace ClusterixN.Common.Utils
{
    public static class FileHelper
    {
        public static byte[] ReadLockedFile(string file)
        {
            if (!File.Exists(file)) return new byte[0];
            var tmpFile = Guid.NewGuid().ToString();
            File.Copy(file, tmpFile);
            byte[] buffer;
            using (var fs = File.Open(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            File.Delete(tmpFile);
            return buffer;
        }
    }
}
