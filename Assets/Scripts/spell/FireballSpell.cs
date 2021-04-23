using Pandora;
using Pandora.Pool;
using Pandora.Combat;
using UnityEngine;

namespace Pandora.Spell {
    public class FireballSpell : MonoBehaviour, ProjectileSpell {
        public MapComponent map { get; set; }
        public int damage = 20, towerDamage = 400;
        public int radius = 3;

        public void SpellCollided(GridCell cell) {
            var teamComponent = GetComponent<TeamComponent>();
            GridCell lastTargetCell = null;

            foreach (var entity in MapComponent.Instance.engine.Entities) {
                if (lastTargetCell != null) {
                    PoolInstances.GridCellPool.ReturnObject(lastTargetCell);
                }

                lastTargetCell = entity.GetPooledCurrentCell();

                var targetPosition = lastTargetCell.vector;

                var cellPosition = cell.vector;

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
        }

        void Start() {
            var projectileSpell = GetComponent<ProjectileSpellBehaviour>();

            map = projectileSpell.map;
            projectileSpell.spell = this;
        }
    }
}