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

namespace ClusterixN.QueryProcessing.Data
{
    public class QueryBuffer
    {
        public Guid QueryId { get; set; }

        public byte[] Data { get; set; }

        public int OrderNumber { get; set; }

        public bool IsLast { get; set; }

        public bool IsPreparedToTransfer { get; set; }

        public bool IsFlushedToDisk { get; set; }

        public Guid BufferId { get; set; }

        public QueryBuffer()
        {
            BufferId = Guid.NewGuid();
        }
    }
}
