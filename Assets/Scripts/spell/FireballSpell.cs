using Pandora;
using Pandora.Combat;
using UnityEngine;

namespace Pandora.Spell {
    public class FireballSpell : MonoBehaviour, ProjectileSpell {
        public MapComponent map { get; set; }
        public int damage = 20;
        public int radius = 3;

        public void SpellCollided(GridCell cell) {
            foreach (var lifeComponent in map.gameObject.GetComponentsInChildren<LifeComponent>()) {
                var targetPosition =
                    map.GetCell(lifeComponent.gameObject).vector;

                var cellPosition = cell.vector;

                Logger.Debug($"Spell evaluating {targetPosition}");

                if (
                    targetPosition.x >= cellPosition.x - radius &&
                    targetPosition.x <= cellPosition.x + radius &&
                    targetPosition.y >= cellPosition.y - radius &&
                    targetPosition.y <= cellPosition.y + radius
                ) {
                    var towerComponent = lifeComponent.gameObject.GetComponent<TowerPositionComponent>();

                    var isMiddleTower = towerComponent != null && towerComponent.EngineTowerPosition.IsMiddle();

                    Logger.Debug($"Hitting {lifeComponent.gameObject}");

                    lifeComponent.AssignDamage((!isMiddleTower) ? damage : damage / 4, new SpellDamage(gameObject));
                }
            }
        }

        void Start() {
            var projectileSpell = GetComponent<ProjectileSpellBehaviour>();

            map = projectileSpell.map;
            projectileSpell.spell = this;
        }
    }
}