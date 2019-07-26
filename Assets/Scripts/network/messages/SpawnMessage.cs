using Pandora.Messages;
using Google.Protobuf;

namespace CRclone.Network.Messages {
    public class SpawnMessage: Message {
        public string unitName;
        public int cellX;
        public int cellY;
        public int team;

        public byte[] ToBytes(string matchToken) {
            var spawn = new Spawn {
                UnitName = unitName,
                X = cellX,
                Y = cellY,
                Team = team
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                Spawn = spawn
            };

            return envelope.ToByteArray();
        }
    }
}