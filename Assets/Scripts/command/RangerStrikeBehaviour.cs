using UnityEngine;
using Pandora;
using Pandora.Combat;

namespace Pandora.Command {
    public class RangerStrikeBehaviour: MonoBehaviour, CombatVFXCallback {
        public GameObject Target { get; set; }

        void Awake() {
            if (Target == null) {
                Logger.DebugWarning("Target not initialized");

                return;
            }

            var spriteRenderer = Target.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null) {
                spriteRenderer = Target.GetComponentInChildren<SpriteRenderer>();
            }

            var worldRect = spriteRenderer.sprite.bounds;

            var position = new Vector2(
                worldRect.min.x + worldRect.extents.x / 2,
                worldRect.min.y
            );

            transform.position = position;
        }
    }
}