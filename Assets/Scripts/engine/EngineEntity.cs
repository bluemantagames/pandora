using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pandora.Engine
{
    // An entity inside the engine
    public class EngineEntity
    {
        public int Speed; // units / tick
        public int CollisionSpeed = 0; // "Bounce" from a collision speed
        public Vector2Int Position;
        public Vector2Int Direction;
        public IEnumerator<Vector2Int> Path;
        public GameObject GameObject;
        // whether the entity should move on collisions, is a structure or
        // is a "map obstacle" (e.g. river)
        public bool IsRigid = true, IsStructure = false, IsMapObstacle = false;
        public CollisionCallback CollisionCallback;
        public PandoraEngine Engine;
        public int Layer = 1;
        public DateTime Timestamp;
        public Vector2Int Target;

        public bool IsEvading = false;
        public EngineEntity EvadedUnit = null;

        public void SetSpeed(int engineUnitsPerSecond)
        {
            Speed = Engine.GetSpeed(engineUnitsPerSecond);
        }

        public void SetTarget(GridCell cell)
        {
            Target = Engine.GridCellToPhysics(cell);

            Target.x += Engine.UnitsPerCell / 2;
            Target.y += Engine.UnitsPerCell / 2;

            Path = FindPath(Position, Target);
        }

        public void ResetTarget()
        {
            if (Target != null && Path != null)
            {
                Path = FindPath(Position, Target);
            }
        }
 
        public IEnumerator<Vector2Int> FindPath(Vector2Int position, Vector2Int target)
        {
            if (IsEvading)
            {
                return Engine.FindPath(this, target);
            }
            else
            {
                return Bresenham.GetEnumerator(position, target);
            }
        }

        public void SetTarget(Vector2Int target)
        {
            Target = target;

            Target.x += Engine.UnitsPerCell / 2;
            Target.y += Engine.UnitsPerCell / 2;

            Path = FindPath(Position, Target);
        }

        public void SetEmptyPath()
        {
            Path = null;
            Speed = 0;
        }

        // This method actually sets the target at the _current_ entity position,
        // it doesn't follow. TODO: Maybe make it follow?
        public void SetTarget(EngineEntity entity)
        {
            Path = FindPath(Position, entity.Position);
        }

        public GridCell GetCurrentCell()
        {
            return Engine.PhysicsToGridCell(Position);
        }

        public Vector2 GetWorldPosition()
        {
            return Engine.PhysicsToMap(Position);
        }

        public Vector2 GetFlippedWorldPosition()
        {
            return Engine.FlippedPhysicsToMap(Position);
        }

        public List<EngineEntity> FindInHitboxRange(int engineUnitsRange, bool countStructures)
        {
            return Engine.FindInHitboxRange(this, engineUnitsRange, countStructures);
        }

        public void PrintDebugInfo()
        {
            Engine.PrintDebugInfo(this);
        }
    }
}