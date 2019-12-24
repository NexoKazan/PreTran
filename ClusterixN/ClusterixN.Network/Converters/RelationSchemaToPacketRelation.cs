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

﻿using System.Linq;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Network.Packets.Data;
using RelationSchema = ClusterixN.Common.Data.Query.Relation.RelationSchema;

namespace ClusterixN.Network.Converters
{
    public static class RelationSchemaToPacketRelationSchema
    {
        public static Packets.Data.RelationSchema ToPacketRelationSchema(this RelationSchema relationSchema)
        {
            var data = new Packets.Data.RelationSchema();

            data.Fields = new RelationField[relationSchema.Fields.Count];
            for (var i = 0; i < data.Fields.Length; i++)
            {
                data.Fields[i] = new RelationField
                {
                    Name = relationSchema.Fields[i].Name,
                    Params = relationSchema.Fields[i].Params
                };
            }

            data.Indexes = new RelationIndex[relationSchema.Indexes.Count];
            for (var i = 0; i < data.Indexes.Length; i++)
            {
                data.Indexes[i] = new RelationIndex()
                {
                    Name = relationSchema.Indexes[i].Name,
                    Fields = relationSchema.Indexes[i].FieldNames.ToArray(),
                    IsPrimary = relationSchema.Indexes[i].IsPrimary,
                };
            }

            return data;
        }

        public static RelationSchema ToRelationSchema(this Packets.Data.RelationSchema relationSchema)
        {
            var data = new RelationSchema();


            foreach (var index in relationSchema.Indexes)
            {
                data.Indexes.Add(
                    new Index()
                    {
                        Name = index.Name,
                        FieldNames = index.Fields.ToList(),
                        IsPrimary = index.IsPrimary
                    });
            }

            foreach (var field in relationSchema.Fields)
            {
                data.Fields.Add(
                    new Field()
                    {
                        Name = field.Name,
                        Params = field.Params
                    });
            }

            return data;
        }
    }
}
