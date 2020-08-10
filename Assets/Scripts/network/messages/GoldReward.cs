
using Pandora.Messages;
using Google.Protobuf;
using System;

namespace Pandora.Network.Messages {
    public class GoldRewardMessage : Message
    {
        public int playerId;
        public int team;
        public int goldSpent;
        public string rewardId;
        public int elapsedMs; 

        public byte[] ToBytes(string matchToken)
        {
            var goldReward = new GoldReward {
                PlayerId = playerId,
                Team = team,
                GoldSpent = goldSpent,
                RewardId = rewardId,
                ElapsedMs = (ulong) elapsedMs
            };

            var envelope = new ClientEnvelope {
                Token = matchToken,
                GoldReward = goldReward
            };

            return envelope.ToByteArray();
        }
    }
}