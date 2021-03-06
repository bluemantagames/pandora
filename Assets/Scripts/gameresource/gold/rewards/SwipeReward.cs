using UnityEngine;
using Pandora;
using Pandora.Engine;
using Pandora.Combat;
using System.Collections.Generic;
using System;

namespace Pandora.Resource.Gold.Rewards {

    public class SwipeReward : MonoBehaviour, GoldReward
    {
        // Mapping between sector "center" position and its extreme points
        Dictionary<Vector2Int, (Vector2Int, Vector2Int)> sectors = new Dictionary<Vector2Int, (Vector2Int, Vector2Int)>(1000);
        public float SectorDistanceThreshold = 2000f; 
        public GameObject SmiteVFX;

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

                Vector2Int? sectorPosition = null;

                foreach (var existingSector in sectors.Keys) {
                    var distance = Vector2Int.Distance(entity.Position, existingSector);

                    if (distance < SectorDistanceThreshold) {
                        sectorPosition = existingSector;

                        break;
                    }
                }

                if (sectorPosition.HasValue) {
                    var (lowerExtreme, higherExtreme) = sectors[sectorPosition.Value];
                    
                    lowerExtreme.Set(
                        Math.Min(lowerExtreme.x, entity.Position.x),
                        Math.Min(lowerExtreme.y, entity.Position.y)
                    );

                    higherExtreme.Set(
                        Math.Max(higherExtreme.x, entity.Position.x),
                        Math.Max(higherExtreme.y, entity.Position.y)
                    );
                } else {
                    sectors[entity.Position] = (entity.Position, entity.Position);
                }

                lifeComponent.Kill(source);
            }

            // for every sector, find the middle point and spawn the vfx there
            foreach (var sectorKey in sectors.Keys) {
                var (lo, hi) = sectors[sectorKey];

                var position = new Vector2Int(
                    (lo.x + hi.x) / 2, (lo.y + hi.y) / 2
                );

                Instantiate(
                    SmiteVFX,
                    map.engine.PhysicsToMapWorld(position),
                    SmiteVFX.transform.rotation
                );
            }

            sectors.Clear();
        }
    }
}