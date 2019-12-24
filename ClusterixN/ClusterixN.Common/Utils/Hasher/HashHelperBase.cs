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

﻿namespace ClusterixN.Common.Utils.Hasher
{
    public class HashHelperBase
    {
        protected static int GetNodeNumber(int nodeCount, params int[] keys)
        {
            int hashSum = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < keys.Length; i++)
            {
                hashSum += keys[i] % nodeCount;
            }
            return hashSum % nodeCount;
        }

        protected static int ParseField(string field)
        {
            if (field == "NULL") return 0;

            var number = 0;
            var clearField = field.Replace("\"", "");

            if (!int.TryParse(clearField, out number))
            {
                if (clearField.Length > 0) return field[0];
            }

            return number;
        }
    }
}
