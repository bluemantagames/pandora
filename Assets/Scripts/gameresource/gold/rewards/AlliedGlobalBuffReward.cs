using UnityEngine;
using Pandora;
using Pandora.Combat;
using Pandora.Engine;
using System.Collections.Generic;
using Pandora.VFX;

namespace Pandora.Resource.Gold.Rewards {
    public class AlliedGlobalBuffReward: MonoBehaviour, GoldReward {
        public string Id => "allied-global-buff-reward";
        public GameObject BuffObject;
        public GameObject VFX;

        Effect buffEffect;

        public void RewardApply(MapComponent map, int team, int playerId)
        {
            Debug.Log("Applying buff");

            var entities = new List<EngineEntity>(map.engine.Entities);

            foreach (var entity in entities) {
                if (entity.GameObject.GetComponent<LifeComponent>() == null) continue;

                var isTower = entity.GameObject.GetComponent<TowerTeamComponent>() != null;

                if (isTower) continue;

                var entityTeam = entity.GameObject.GetComponent<TeamComponent>();

                if (entityTeam == null || entityTeam.Team != team) continue;

                buffEffect.Apply(gameObject, entity.GameObject);

                VFX.GetComponent<VFXApplier>().Apply(entity.GameObject);
            }

            Debug.Log("Called allied global buff reward");
        }

        public void Start() {
            RewardsRepository.Instance.Register(this);

            buffEffect = BuffObject.GetComponent<Effect>();
        }
    }
}