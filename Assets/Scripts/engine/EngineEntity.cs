using UnityEngine;

namespace Pandora.Engine {
    // An entity inside the engine
    public class EngineEntity {
        public int Speed; // units / sec
        public Vector2Int Position;
        public Vector2Int Direction;
        public GameObject GameObject;
        public bool IsRigid; // whether the entity should move on collisions
    }
}