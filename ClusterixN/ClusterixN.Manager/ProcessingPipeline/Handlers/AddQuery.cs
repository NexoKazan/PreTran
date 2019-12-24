#region Copyright
/*
 * Copyright 2019 Roman Klassen
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

using System;
using System.IO;
using System.Text;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Utils;
using ClusterixN.Manager.Interfaces;
using ClusterixN.Manager.Managers;
using ClusterixN.Manager.ProcessingPipeline.Handlers.Base;
using ClusterixN.Network.Interfaces;
using ClusterixN.Network.Packets;
using ClusterixN.Network.Packets.Base;
using ClusterixN.QueryProcessing.Managers;

namespace ClusterixN.Manager.ProcessingPipeline.Handlers
{
    internal class AddQuery : HandlerBase
    {
        public AddQuery(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<SqlQueryPacket>(SqlQueryPacketHandler);
            server.SubscribeToPacket<XmlQueryPacket>(XmlQueryPacketHandler);
        }

        private void XmlQueryPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as XmlQueryPacket;
            if (packet == null) return;

            var bytes = Encoding.UTF8.GetBytes(packet.XmlQuery);
            using (var stream = new MemoryStream(bytes))
            {
                Query query = null;
                try
                {
                    query = Query.Load(stream);
                }
                catch (Exception e)
                {
                    Logger.Error($"Ошибка обработки нового XML запроса: {e}");
                }

                if (query != null)
                {
                    QueryManager.AddQuery(query);
                }
            }
        }

        private void SqlQueryPacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as SqlQueryPacket;
            if (packet == null) return;

            var query = TranslateQuery(packet.Query);
            QueryManager.AddQuery(query);
        }

        private Query TranslateQuery(string query)
        {
            Logger.Trace($"Начата трансляция запроса: {query}");

            throw new NotImplementedException();

            Logger.Trace($"Трансляция запроса {query} завершена.");
        }

        public override void DoAction()
        { }
    }
}