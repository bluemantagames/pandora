using UnityEngine;

namespace Pandora.Engine {
    public class DiscreteHitboxComponent: MonoBehaviour {
        public Vector2Int? Hitbox = null;

        public void Load() {
            var hitboxLoader = HitboxLoader.Instance;

            if (hitboxLoader.Hitboxes.ContainsKey(gameObject.name)) {
                Logger.Debug($"Loading static hitbox for {gameObject.name}");

                var hitbox = hitboxLoader.Hitboxes[gameObject.name];

                Hitbox = new Vector2Int(hitbox.HitboxSizeX, hitbox.HitboxSizeY);
            }
        }
    }
}