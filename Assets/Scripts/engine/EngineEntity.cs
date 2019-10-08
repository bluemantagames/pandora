using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pandora.Engine {
    // An entity inside the engine
    public class EngineEntity {
        public int Speed; // units / tick
        public int CollisionSpeed = 0; // "Bounce" from a collision speed
        public Vector2Int Position;
        public Vector2Int Direction;
        public IEnumerator<Vector2Int> Path;
        public GameObject GameObject;
        public bool IsRigid = true, IsStructure = false; // whether the entity should move on collisions
        public CollisionCallback CollisionCallback;
        public PandoraEngine Engine;
        public int Layer = 1;
        public DateTime Timestamp;
        public Vector2Int Target;

        public void SetSpeed(float GridCellSpeed) {
            Speed = Engine.GetSpeed(GridCellSpeed);
        }

        public void SetTarget(GridCell cell) {
            Target = Engine.GridCellToPhysics(cell);

            Target.x += Engine.UnitsPerCell / 2;
            Target.y += Engine.UnitsPerCell / 2;

            Path = Bresenham.GetEnumerator(Position, Target);
        }

        public void ResetTarget() {
            Path = Bresenham.GetEnumerator(Position, Target);
        }

        public void SetTarget(Vector2Int target) {
            Target = target;

            Target.x += Engine.UnitsPerCell / 2;
            Target.y += Engine.UnitsPerCell / 2;

            Path = Bresenham.GetEnumerator(Position, Target);
        }

        public void SetEmptyPath() {
            Path = null;
            Speed = 0;
        }

        // This method actually sets the target at the _current_ entity position,
        // it doesn't follow. TODO: Maybe make it follow?
        public void SetTarget(EngineEntity entity) {
            Path = Bresenham.GetEnumerator(Position, entity.Position);
        }

        public GridCell GetCurrentCell() {
            return Engine.PhysicsToGridCell(Position);
        }

        public Vector2 GetWorldPosition() {
            return Engine.PhysicsToMap(Position);
        }

        public Vector2 GetFlippedWorldPosition() {
            return Engine.FlippedPhysicsToMap(Position);
        }

        public List<EngineEntity> FindInHitboxRange(int engineUnitsRange, bool countStructures) {
            return Engine.FindInHitboxRange(this, engineUnitsRange, countStructures);
        }

        public void PrintDebugInfo() {
            Engine.PrintDebugInfo(this);
        }
    }
}