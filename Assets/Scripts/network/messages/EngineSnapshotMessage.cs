using System;
using Pandora.Messages;
using Google.Protobuf;

namespace Pandora.Network.Messages {
    public class EngineSnapshotMessage {
        public string Snapshot;
        public DateTime Timestamp;
        public ulong ElapsedMs;
        public int Team;
    }
}