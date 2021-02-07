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
        public int DelayMs = 1500, damage = 20, radius = 3, Speed = 0;
        uint totalElapsed = 0;
        bool done = false;
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

            if (totalElapsed >= DelayMs && !done) {
                if (VFX != null) {
                    var vfx = Instantiate(VFX, entityComponent.Entity.GetWorldPosition(), VFX.transform.rotation);
                }

                done = true;

                var teamComponent = GetComponent<TeamComponent>();

                GridCell lastTargetCell = null;

                foreach (var entity in MapComponent.Instance.engine.Entities) {
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

                        var isMiddleTower = towerComponent != null && towerComponent.EngineTowerPosition.IsMiddle();

                        Logger.Debug($"Hitting {lifeComponent.gameObject}");

                        lifeComponent.AssignDamage((!isMiddleTower) ? damage : damage / 4, new SpellDamage(gameObject));
                    }
                }

                PoolInstances.GridCellPool.ReturnObject(lastTargetCell);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
