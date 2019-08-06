using CRclone;
using UnityEngine;

namespace CRclone.Spell {
    public class FireballSpell : MonoBehaviour, ProjectileSpell {
        public MapComponent map { get; set; }
        public float damage = 20f;
        public float radius = 3;

        public void SpellCollided(Vector2 cell) {
            foreach (var lifeComponent in map.gameObject.GetComponentsInChildren<LifeComponent>()) {
                var cellPosition =
                    map.WorldPositionToGridCell(
                        lifeComponent.gameObject.transform.position
                    );

                Debug.Log($"Spell evaluating {cellPosition}");

                if (
                    cellPosition.x >= cell.x - radius &&
                    cellPosition.x <= cell.x + radius &&
                    cellPosition.y >= cell.y - radius &&
                    cellPosition.y <= cell.y + radius
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