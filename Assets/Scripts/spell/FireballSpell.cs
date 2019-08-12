using Pandora;
using UnityEngine;

namespace Pandora.Spell {
    public class FireballSpell : MonoBehaviour, ProjectileSpell {
        public MapComponent map { get; set; }
        public float damage = 20f;
        public float radius = 3;

        public void SpellCollided(GridCell cell) {
            foreach (var lifeComponent in map.gameObject.GetComponentsInChildren<LifeComponent>()) {
                var targetPosition =
                    map.GetCell(lifeComponent.gameObject).vector;

                var cellPosition = cell.vector;

                Debug.Log($"Spell evaluating {targetPosition}");

                if (
                    targetPosition.x >= cellPosition.x - radius &&
                    targetPosition.x <= cellPosition.x + radius &&
                    targetPosition.y >= cellPosition.y - radius &&
                    targetPosition.y <= cellPosition.y + radius
                ) {
                    Debug.Log($"Hitting {lifeComponent.gameObject}");

                    lifeComponent.AssignDamage(damage);
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