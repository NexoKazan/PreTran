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

﻿namespace ClusterixN.Common.Utils.StringReplacer
{
    public class StringReplacer
    {
        public static unsafe string UnsafeUnmanagedImplementation(string input, string oldValue, string newValue)
        {
            using (var fastReplacer = new FastReplacer(input, oldValue))
            {
                var replaceLength = oldValue.Length;
                var inputLength = input.Length;
                var replacedDataLength = fastReplacer.FoundIndexes * replaceLength;
                var outputLength = inputLength - replacedDataLength + fastReplacer.FoundIndexes * newValue.Length;
                var outputPtr = stackalloc char[(outputLength + 1)];

                fastReplacer.Replace(outputPtr, outputLength, newValue);
                return new string(outputPtr);
            }
        }

    }
}
