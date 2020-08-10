using UnityEngine;
using Pandora;
using Pandora.Combat;

namespace Pandora.Resource.Gold.Rewards {
    public class AlliedGlobalBuffReward: MonoBehaviour, GoldReward {
        public string Id => "allied-global-buff-reward";
        public GameObject BuffObject;

        public void RewardApply(MapComponent map, int team, int playerId)
        {
            Debug.Log("Called allied global buff reward");
        }

        public void Start() {
            RewardsRepository.Instance.Register(this);
        }
    }
}