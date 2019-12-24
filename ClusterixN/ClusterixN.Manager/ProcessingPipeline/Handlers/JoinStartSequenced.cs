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
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Enums;
using ClusterixN.Common.Data.Query.Enum;
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
    internal class JoinStartSequenced : JoinStartBase
    {
        public JoinStartSequenced(IServerCommunicator server, IQueryManager queryManager, QueryBufferManager queryBufferManager,
            NodesManager nodesManager,
            PauseLogManager pauseLogManager, QueueManager queueManager) : base(server,
            queryManager, nodesManager, pauseLogManager, queryBufferManager, queueManager)
        {
            server.SubscribeToPacket<JoinCompletePacket>(JoinCompletePacketHandler);
        }

        private void StartJoin()
        {
            foreach (var node in NodesManager.GetNodes(NodeType.Join)
                .Where(n => n.QueriesInProgress.Count > 0 && n.CanTransferDataQuery))
            {
                StartJoinProcessing(node);
            }
        }
        
        private void StartJoinProcessing(Node node)
        {
            StartJoinProcessing(node, false);
        }
        
        private void JoinCompletePacketHandler(PacketBase packetBase)
        {
            var packet = packetBase as JoinCompletePacket;
            if (packet != null)
            {
                QueueManager.Add(() => JoinCompletePacketReceived(packet));
            }
        }
        
        private void JoinCompletePacketReceived(JoinCompletePacket packet)
        {
            var node = NodesManager.GetNode(packet.Id.ClientId);
            if (node != null && node.NodeType == NodeType.Join)
            {
                node.QueryInProgress--;

                QueryManager.SetJoinRelationStatus(packet.LeftRelationId, QueryRelationStatus.Processed);
                QueryManager.SetJoinRelationStatus(packet.RightRelationId, QueryRelationStatus.Processed);
                node.PendingRam -= QueryManager.GetRelationById(packet.LeftRelationId)?.DataAmount ?? 0;
                node.PendingRam -= QueryManager.GetRelationById(packet.RightRelationId)?.DataAmount ?? 0;

                QueryManager.SetJoinRelationStatus(packet.NewRelationId, QueryRelationStatus.Transfered);
            }
        }

        public override void DoAction()
        {
            StartJoin();
        }
    }
}