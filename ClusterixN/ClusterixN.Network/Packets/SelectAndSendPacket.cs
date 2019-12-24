#region Copyright
/*
 * Copyright 2018 Roman Klassen
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

using ClusterixN.Network.Packets.Data;
using ProtoBuf;

namespace ClusterixN.Network.Packets
{
    [ProtoContract]
    public class SelectAndSendPacket : QueryPacket
    {
        private SelectAndSendPacket()
        {
            PacketType = GetType();
        }

        public SelectAndSendPacket(int queryNumber) : this()
        {
            QueryNumber = queryNumber;
        }

        [ProtoMember(1)]
        public int JoinCount { get; set; }

        [ProtoMember(2)]
        public string[] JoinAddresses { get; set; }

        [ProtoMember(3)]
        public int[] HashCount { get; set; }

        [ProtoMember(4)]
        public RelationSchema RelationShema { get; set; }

        [ProtoMember(5)]
        public int IoCount { get; set; }

        [ProtoMember(6)]
        public int IoNumber { get; set; }
    }
}