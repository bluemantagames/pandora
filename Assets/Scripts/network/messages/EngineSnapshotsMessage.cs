
using System;
using System.Collections.Generic;
using Pandora.Messages;
using Google.Protobuf;

namespace Pandora.Network.Messages
{
    public class EngineSnapshotsMessage : Message
    {
        List<EngineSnapshotMessage> snapshotMessages;

        public EngineSnapshotsMessage(List<EngineSnapshotMessage> snapshots)
        {
            this.snapshotMessages = snapshots;
        }

        public byte[] ToBytes(string matchToken)
        {
            List<EngineSnapshot> snapshots = new List<EngineSnapshot> { };
            
            var engineSnapshots = new EngineSnapshots();

            foreach (var message in snapshotMessages)
            {
                var snapshot = new Pandora.Messages.EngineSnapshot
                {
                    Snapshot = message.Snapshot,
                    Timestamp = (ulong)new DateTimeOffset(message.Timestamp).ToUnixTimeMilliseconds(),
                    ElapsedMs = message.ElapsedMs,
                    PlayerId = NetworkControllerSingleton.instance.PlayerId ?? throw new Exception("Could not find player id"),
                    Team = message.Team
                };

                
                engineSnapshots.Snapshots.Add(snapshot);
            }

            var envelope = new ClientEnvelope {
                Token = matchToken,
                EngineSnapshots = engineSnapshots
            };

            return envelope.ToByteArray();
        }
    }
}