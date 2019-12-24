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

﻿using JoinTreeLeaf = ClusterixN.Common.Data.Query.JoinTree.JoinTreeLeaf;

namespace ClusterixN.Network.Converters
{
    public static class JoinLeafToPacketJoinLeaf
    {
        public static Packets.Data.JoinTreeLeaf ToPacketJoinLeaf(this JoinTreeLeaf joinTreeLeaf)
        {
            return CopyToPacketJoinLeaf(joinTreeLeaf);
        }
        
        private static Packets.Data.JoinTreeLeaf CopyToPacketJoinLeaf(JoinTreeLeaf joinTreeLeaf)
        {
            if (joinTreeLeaf == null) return null;

            return new Packets.Data.JoinTreeLeaf()
            {
                RelationId = joinTreeLeaf.RelationId,
                Query = joinTreeLeaf.Query,
                LeftRelation = CopyToPacketJoinLeaf(joinTreeLeaf.LeftRelation),
                RightRelation = CopyToPacketJoinLeaf(joinTreeLeaf.RightRelation),
            };
        }

        private static JoinTreeLeaf CopyToPacketJoinLeaf(Packets.Data.JoinTreeLeaf joinTreeLeaf)
        {
            if (joinTreeLeaf == null) return null;

            return new JoinTreeLeaf()
            {
                RelationId = joinTreeLeaf.RelationId,
                Query = joinTreeLeaf.Query,
                LeftRelation = CopyToPacketJoinLeaf(joinTreeLeaf.LeftRelation),
                RightRelation = CopyToPacketJoinLeaf(joinTreeLeaf.RightRelation),
            };
        }

        public static JoinTreeLeaf ToJoinTreeLeaf(this Packets.Data.JoinTreeLeaf relationSchema)
        {
            return CopyToPacketJoinLeaf(relationSchema);
        }
    }
}
