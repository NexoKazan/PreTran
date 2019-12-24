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
using ClusterixN.Network.Packets.Base;
using ProtoBuf;

namespace ClusterixN.Network.Packets
{
    [ProtoContract]
    public class JoinCompletePacket : QueryPacketBase
    {
        public JoinCompletePacket()
        {
            PacketType = GetType();
        }

        [ProtoMember(1)]
        public Guid NewRelationId { get; set; }

        [ProtoMember(2)]
        public Guid LeftRelationId { get; set; }

        [ProtoMember(3)]
        public Guid RightRelationId { get; set; }
    }
}