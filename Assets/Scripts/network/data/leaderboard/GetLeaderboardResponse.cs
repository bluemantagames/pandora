using System.Collections.Generic;

namespace Pandora.Network.Data.Leaderboard
{
    [System.Serializable]
    public class GetLeaderboardResponse
    {
        public List<LeaderboardValue> players;
    }

    [System.Serializable]
    public class LeaderboardValue
    {
        public string username;
        public int position;
        public int points;
    }
}