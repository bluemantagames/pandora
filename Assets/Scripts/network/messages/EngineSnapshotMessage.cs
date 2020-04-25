using System;
using Pandora.Messages;
using Google.Protobuf;

namespace Pandora.Network.Messages {
    public class EngineSnapshotMessage: Message {
        public string Snapshot;
        public DateTime Timestamp;
        public ulong ElapsedMs;

        public byte[] ToBytes(string matchToken) {
            var snapshotMessage = new Pandora.Messages.EngineSnapshot {
              Snapshot = Snapshot,
              Timestamp = (ulong)new DateTimeOffset(Timestamp).ToUnixTimeMilliseconds(),
              ElapsedMs = ElapsedMs
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                EngineSnapshot = snapshotMessage
            };

            return envelope.ToByteArray();
        }
    }
}