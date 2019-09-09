
using Pandora.Messages;
using Google.Protobuf;
using System;

namespace Pandora.Network.Messages {
    public class CommandMessage: Message {
        public int team;
        public string unitId;

        public byte[] ToBytes(string matchToken) {
            var command = new Pandora.Messages.Command {
                Team = team,
                PlayerId = NetworkControllerSingleton.instance.PlayerId ?? throw new Exception("Could not find player id"),
                UnitId = unitId
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                Command = command
            };

            return envelope.ToByteArray();
        }
    }
}