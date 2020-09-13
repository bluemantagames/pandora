using UnityEngine;
using Pandora.Combat;

namespace Pandora.Engine {
    public class DiscreteHitboxComponent: MonoBehaviour {
        public Vector2Int? Hitbox = null;
        public string HitboxName;

        public void Load() {
            var hitboxLoader = HitboxLoader.Instance;
            var projectileBehaviour = GetComponent<ProjectileBehaviour>();

            HitboxName = (projectileBehaviour == null) ? gameObject.name : projectileBehaviour.originalPrefab.name;

            if (hitboxLoader.Hitboxes.ContainsKey(HitboxName)) {
                Logger.Debug($"Loading static hitbox for {HitboxName}");

                var hitbox = hitboxLoader.Hitboxes[HitboxName];

                Hitbox = new Vector2Int(hitbox.HitboxSizeX, hitbox.HitboxSizeY);
            }
        }
    }
}