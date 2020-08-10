using System.Collections.Generic;

namespace Pandora.Resource.Gold.Rewards {
    public class RewardsRepository {
        static private RewardsRepository _instance = null;
        
        static public RewardsRepository Instance {
            get {
                if (_instance == null) {
                    _instance = new RewardsRepository();
                }

                return _instance;
            }
        }

        /// <summary>A map of id -> reward</summary>
        Dictionary<string, GoldReward> rewards = new Dictionary<string, GoldReward>();

        public void Register(GoldReward reward) {
            rewards[reward.Id] = reward;
        }

        public GoldReward GetReward(string id) {
            return rewards[id];
        }

    }
}