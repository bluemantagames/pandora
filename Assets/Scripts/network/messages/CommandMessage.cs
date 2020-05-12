
using Pandora.Messages;
using Google.Protobuf;
using System;

namespace Pandora.Network.Messages {
    public class CommandMessage: Message {
        public int team;
        public string unitId;
        public ulong elapsedMs;

        public byte[] ToBytes(string matchToken) {
            var command = new Pandora.Messages.Command {
                Team = team,
                PlayerId = NetworkControllerSingleton.instance.PlayerId ?? throw new Exception("Could not find player id"),
                UnitId = unitId,
                ElapsedMs = elapsedMs
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                Command = command
            };

            return envelope.ToByteArray();
        }
    }
}