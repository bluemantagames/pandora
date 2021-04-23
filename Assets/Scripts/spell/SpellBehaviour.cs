using System.Collections;
using System.Collections.Generic;
using Pandora;
using Pandora.Pool;
using Pandora.Combat;
using Pandora.Engine;
using UnityEngine;

namespace Pandora.Spell
{
    public class SpellBehaviour : MonoBehaviour, EngineBehaviour
    {
        public GridCell Target { get; set; }
        public GridCell StartCell { get; set; }
        public int DelayMs = 1500, VFXDelayMS = 300, damage = 20, radius = 3, Speed = 0, towerDamage = 400;
        uint totalElapsed = 0;
        bool done = false, vfxPlayed = false;
        public GameObject VFX;

        EngineComponent entityComponent;

        public string ComponentName {
            get => "AreaSpellBehaviour";
        }

        void Awake()
        {
            entityComponent = GetComponent<EngineComponent>();
        }

        public void TickUpdate(uint msElapsed)
        {
            totalElapsed += msElapsed;

            if (VFX != null && !vfxPlayed && totalElapsed >= DelayMs - VFXDelayMS) {
                var vfx = Instantiate(VFX, entityComponent.Entity.GetWorldPosition(), VFX.transform.rotation);

                vfxPlayed = true;
            }

            if (totalElapsed >= DelayMs && !done) {
                done = true;

                var teamComponent = GetComponent<TeamComponent>();

                GridCell lastTargetCell = null;

                // copy entities here since they could die in the loop
                var entities = new List<EngineEntity>(MapComponent.Instance.engine.Entities);

                foreach (var entity in entities) {
                    if (lastTargetCell != null) {
                        PoolInstances.GridCellPool.ReturnObject(lastTargetCell);
                    }

                    lastTargetCell = entity.GetPooledCurrentCell();

                    var targetPosition = lastTargetCell.vector;

                    var cellPosition = Target.vector;

                    Logger.Debug($"Spell evaluating {targetPosition}");

                    if (
                        targetPosition.x >= cellPosition.x - radius &&
                        targetPosition.x <= cellPosition.x + radius &&
                        targetPosition.y >= cellPosition.y - radius &&
                        targetPosition.y <= cellPosition.y + radius
                    ) {
                        var lifeComponent = entity.GameObject.GetComponent<LifeComponent>();

                        if (lifeComponent == null || lifeComponent.IsDead) continue;

                        var towerTeamComponent = entity.GameObject.GetComponent<TowerTeamComponent>();

                        if (towerTeamComponent != null && towerTeamComponent.EngineTeam == teamComponent.Team) continue;

                        var towerComponent = lifeComponent.gameObject.GetComponent<TowerPositionComponent>();

                        var isTower = towerComponent != null;
                        var isMiddleTower = isTower && towerComponent.EngineTowerPosition.IsMiddle();

                        var dealtDamage = 
                            (isMiddleTower) ? damage / 4 :
                            (isTower)       ? towerDamage : damage;

                        Logger.Debug($"Hitting {lifeComponent.gameObject}");

                        lifeComponent.AssignDamage(dealtDamage, new SpellDamage(gameObject));
                    }
                }

                PoolInstances.GridCellPool.ReturnObject(lastTargetCell);

                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
