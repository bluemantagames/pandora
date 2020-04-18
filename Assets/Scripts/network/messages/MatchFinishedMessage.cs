
using Pandora.Messages;
using Google.Protobuf;

namespace Pandora.Network.Messages {
    public class MatchFinishedMessage: Message {
        public int WinnerTeam;

        public byte[] ToBytes(string matchToken) {
            var matchFinished = new Pandora.Messages.MatchFinished {
                WinnerTeam = WinnerTeam
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                MatchFinished = matchFinished
            };

            return envelope.ToByteArray();
        }
    }
}