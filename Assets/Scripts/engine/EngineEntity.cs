using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pandora.Engine
{
    // An entity inside the engine
    [Serializable]
    public class EngineEntity
    {
        public int Speed; // units / tick
        public int CollisionSpeed = 0; // "Bounce" from a collision speed
        public Vector2Int Position;
        public Vector2Int Direction;
        public IEnumerator<Vector2Int> Path;

        GameObject _gameObject;

        [NonSerialized] public Bounds Bounds;

        public GameObject GameObject
        {
            get => _gameObject;

            set
            {
                _gameObject = value;

                Bounds = _gameObject.GetComponent<BoxCollider2D>().bounds;
            }
        }
        // whether the entity should move on collisions, is a structure or
        // is a "map obstacle" (e.g. river)
        public bool IsRigid = true, IsStructure = false, IsMapObstacle = false;
        public CollisionCallback CollisionCallback;
        [NonSerialized] public PandoraEngine Engine;
        public int Layer = 1;
        public DateTime Timestamp;
        public Vector2Int Target;

        public bool IsEvading = false;
        public EngineEntity EvadedUnit = null;

        public string Name {
            get => GameObject.name;
        }

        public string UnitName = null;
        public string UnitId = null;
        [NonSerialized] public DiscreteHitboxComponent DiscreteHitbox = null;

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
                var path = Engine.FindPath(this, target);

                IsEvading = false;

                if (path.MoveNext()) {
                    path.MoveNext();
                }

                if (path.Current != null) {
                    Logger.Debug($"Pathfinding evading found next move {path.Current}");

                    var targetCell = Engine.GridCellToPhysics(path.Current);

                    targetCell.x += Engine.UnitsPerCell / 2;
                    targetCell.y += Engine.UnitsPerCell / 2;

                    return Bresenham.GetEnumerator(position, targetCell);
                } else {
                    Logger.DebugWarning($"Empty path found while evading for {position} -> {target}");

                    return (new LinkedList<Vector2Int>() {}).GetEnumerator();
                }

            }
            else
            {
                return Bresenham.GetEnumerator(position, target);
            }
        }

        public void SetTarget(Vector2Int target)
        {
            Target = target;

            if (!IsEvading)
            {
                Target.x += Engine.UnitsPerCell / 2;
                Target.y += Engine.UnitsPerCell / 2;
            }

            Path = FindPath(Position, Target);
        }

        public void SetEmptyPath()
        {
            Path = null;
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

        public GridCell GetPooledCurrentCell()
        {
            return Engine.PooledPhysicsToGridCell(Position);
        }

        /// <summary>Calculates the world position already adjusted for the map
        /// and the team</summary>
        public Vector2 GetWorldPosition() =>
            Engine.PhysicsToMapWorld(Position);

        public List<EngineEntity> FindInHitboxRange(int engineUnitsRange, bool countStructures)
        {
            return Engine.FindInHitboxRange(this, engineUnitsRange, countStructures);
        }

        public void PrintDebugInfo()
        {
            Engine.PrintDebugInfo(this);
        }

        public override string ToString() =>
            $"EngineEntity(GameObject: {GameObject.name}, Position: {Position})";
    }
}