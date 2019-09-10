using Pandora.Messages;
using Google.Protobuf;
using System;

namespace Pandora.Network.Messages {
    public class SpawnMessage: Message {
        public string unitName;
        public int cellX;
        public int cellY;
        public int team;
        public string unitId;
        public DateTime timestamp;
        public int manaUsed;

        public byte[] ToBytes(string matchToken) {
            var spawn = new Spawn {
                UnitName = unitName,
                X = cellX,
                Y = cellY,
                Team = team,
                PlayerId = NetworkControllerSingleton.instance.PlayerId ?? throw new Exception("Could not find player id"),
                UnitId = unitId,
                ManaUsed = manaUsed
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                Spawn = spawn
            };

            return envelope.ToByteArray();
        }
    }
}