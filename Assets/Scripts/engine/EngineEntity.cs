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
        public PandoraEngine Engine;

        public void SetTarget(GridCell cell) {
            var physicsTarget = Engine.GridCellToPhysics(cell);

            physicsTarget.x += Engine.UnitsPerCell / 2;
            physicsTarget.y += Engine.UnitsPerCell / 2;

            Path = Bresenham.GetEnumerator(Position, physicsTarget);
        }

        public GridCell GetCurrentCell() {
            return Engine.PhysicsToGridCell(Position);
        }
    }
}