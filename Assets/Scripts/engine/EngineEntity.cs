using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Engine {
    // An entity inside the engine
    public class EngineEntity {
        public int Speed; // units / tick
        public Vector2Int Position;
        public Vector2Int Direction;
        public IEnumerator<Vector2Int> Path;
        public GameObject GameObject;
        public bool IsRigid = true; // whether the entity should move on collisions
        public CollisionCallback CollisionCallback;
        public PandoraEngine Engine;
        public int Layer = 1;

        public void SetTarget(GridCell cell) {
            var physicsTarget = Engine.GridCellToPhysics(cell);

            physicsTarget.x += Engine.UnitsPerCell / 2;
            physicsTarget.y += Engine.UnitsPerCell / 2;

            Path = Bresenham.GetEnumerator(Position, physicsTarget);
        }

        // This method actually sets the target at the _current_ entity position,
        // it doesn't follow. TODO: Make it follow
        public void SetTarget(EngineEntity entity) {
            Path = Bresenham.GetEnumerator(Position, entity.Position);
        }

        public GridCell GetCurrentCell() {
            return Engine.PhysicsToGridCell(Position);
        }

        public Vector2 GetWorldPosition() {
            return Engine.PhysicsToWorld(Position);
        }
    }
}