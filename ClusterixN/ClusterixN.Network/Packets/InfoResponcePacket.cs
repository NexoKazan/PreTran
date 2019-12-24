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

﻿using ClusterixN.Common.Data.Enums;
using ClusterixN.Network.Packets.Base;
using ProtoBuf;

namespace ClusterixN.Network.Packets
{
    [ProtoContract]
    public class InfoResponcePacket : PacketBase
    {
        public InfoResponcePacket()
        {
            PacketType = GetType();
            IsHardNode = false;
        }

        [ProtoMember(1)]
        public NodeType NodeType { get; set; }

        [ProtoMember(2)]
        public int CpuCount { get; set; }

        [ProtoMember(3)]
        public bool IsHardNode { get; set; }

        [ProtoMember(4)]
        public float MinRamAvaible { get; set; }

        [ProtoMember(5)]
        public int ServerPort { get; set; }
    }
}