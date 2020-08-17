using UnityEngine;
using Pandora;
using Pandora.Engine;
using Pandora.Combat;
using System.Collections.Generic;

namespace Pandora.Resource.Gold.Rewards {
    public class SwipeReward : MonoBehaviour, GoldReward
    {
        public void Start() {
            RewardsRepository.Instance.Register(this);
        }

        public string Id {
            get => "swipe-reward";
        }

        public void RewardApply(MapComponent map, int team, int playerId)
        {
            var entities = new List<EngineEntity>(map.engine.Entities);
            var source = new GoldRewardEffect(gameObject);

            foreach (var entity in entities) {
                var lifeComponent = entity.GameObject.GetComponent<LifeComponent>();

                if (lifeComponent == null || lifeComponent.IsDead) continue;

                var towerComponent = lifeComponent.GetComponent<TowerPositionComponent>();

                if (towerComponent != null) continue;

                lifeComponent.Kill(source);
            }
        }
    }
}