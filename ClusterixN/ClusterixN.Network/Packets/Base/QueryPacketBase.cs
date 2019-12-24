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
using ProtoBuf;

namespace ClusterixN.Network.Packets.Base
{
    [ProtoContract]
    [ProtoInclude(100, typeof(SelectResult))]
    [ProtoInclude(110, typeof(QueryPacket))]
    [ProtoInclude(120, typeof(QueryStatusPacket))]
    [ProtoInclude(130, typeof(JoinCompletePacket))]
    [ProtoInclude(140, typeof(RelationDataPacket))]
    [ProtoInclude(150, typeof(RelationPreparePacket))]
    [ProtoInclude(160, typeof(JoinStartPacket))]
    [ProtoInclude(170, typeof(RelationPreparedPacket))]
    [ProtoInclude(180, typeof(SortCompletePacket))]
    [ProtoInclude(190, typeof(SortStartPacket))]
    [ProtoInclude(200, typeof(IntegratedJoinStartPacket))]
    [ProtoInclude(210, typeof(IntegratedJoinCompletePacket))]
    [ProtoInclude(220, typeof(DropQueryPacket))]
    public class QueryPacketBase : PacketBase
    {
        public QueryPacketBase()
        {
            PacketType = GetType();
        }

        [ProtoMember(1)]
        public Guid QueryId { get; set; }

        [ProtoMember(2)]
        public Guid SubQueryId { get; set; }

        [ProtoMember(3)]
        public Guid RelationId { get; set; }

        [ProtoMember(4)]
        public int QueryNumber { get; set; }
    }
}