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
using ClusterixN.Network.Packets.Data;
using ProtoBuf;

namespace ClusterixN.Network.Packets.Base
{
    [ProtoContract]
    [ProtoInclude(10, typeof(QueryPacketBase))]
    [ProtoInclude(120, typeof(InfoRequestPacket))]
    [ProtoInclude(130, typeof(InfoResponcePacket))]
    [ProtoInclude(140, typeof(CommandPacket))]
    [ProtoInclude(150, typeof(StatusPacket))]
    [ProtoInclude(160, typeof(GetFileRequestPacket))]
    [ProtoInclude(170, typeof(GetFileResponcePacket))]
    [ProtoInclude(180, typeof(TimeAdjustPacket))]
    [ProtoInclude(200, typeof(XmlQueryPacket))]
    [ProtoInclude(210, typeof(SqlQueryPacket))]
    public abstract class PacketBase
    {
        public PacketBase()
        {
            Id = new Identify();
            PacketType = GetType();
        }

        [ProtoMember(1)]
        public Identify Id { get; set; }

        [ProtoMember(2)]
        public Type PacketType { get; set; }
    }
}