using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Engine
{
    public class PandoraEngine
    {
        int minTick = 10; // milliseconds, minimum tick
        public int UnitsPerCell = 100; // physics engine units per grid cell
        List<EngineEntity> entities = new List<EngineEntity> { };
        public MapComponent Map;

        public PandoraEngine(MapComponent map) {
            this.Map = map;
        }

        public void Process(int msLapsed) {
            var ticksNum = msLapsed / minTick;

            Debug.Log($"Advancing {ticksNum} ticks {msLapsed}");

            for (var tick = 0; tick < ticksNum; tick++)
            {
                NextTick();
            }
        }

        public EngineEntity AddEntity(GameObject gameObject, float cellPerSecond, GridCell position, bool isRigid) {
            var speed = Mathf.FloorToInt((cellPerSecond * UnitsPerCell) / (1000 / minTick));

            var physicsPosition = GridCellToPhysics(position) + (new Vector2Int(UnitsPerCell / 2, UnitsPerCell / 2));

            var entity = new EngineEntity {
                Speed = speed,
                Position = physicsPosition,
                GameObject = gameObject,
                Direction = new Vector2Int(0, 0),
                Engine = this
            };

            entities.Add(entity);

            return entity;
        }

        // Start with a reasonable capacity already
        public void NextTick()
        {
            // Move units
            foreach (var entity in entities)
            {
                var unitsMoved = Mathf.FloorToInt(Mathf.Max(1f, entity.Speed));

                for (var i = 0; i < unitsMoved; i++) {
                    entity.Path?.MoveNext();
                }

                entity.Position = entity.Path.Current;
            }

            // Check for collisions
            foreach (var first in entities)
            {
                foreach (var second in entities)
                {
                    var firstBox = GetEntityBounds(first);
                    var secondBox = GetEntityBounds(second);

                    if (first == second || !firstBox.Collides(secondBox)) continue; // continue if they don't collide

                    Vector2Int direction;
                    EngineEntity moved;

                    if (first.Speed >= second.Speed)
                    {
                        direction = second.Position - first.Position;
                        moved = second;
                    }
                    else
                    {
                        direction = first.Position - second.Position;
                        moved = first;
                    }

                    while (firstBox.Collides(secondBox)) // there probably is a math way to do this without a loop
                    {
                        moved.Position = moved.Position + direction; // move the entity away
                    }
                }
            }
        }

        // converts a world point to a physics engine point using linear interpolation 
        // TODO: this is only used on BoxCollider2D to let people use the collider tool to define boundaries
        // if this turns up to create problems we have to define int boundaries in EngineEntity
        Vector2Int WorldToPhysics(Vector2 world)
        {
            var xWorldBounds = Map.cellWidth * Map.mapSizeX;
            var yWorldBounds = Map.cellHeight * Map.mapSizeY;

            var xPhysicsBounds = UnitsPerCell * Map.mapSizeX;
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            return new Vector2Int(
                Mathf.RoundToInt((xPhysicsBounds * world.x) / xWorldBounds),
                Mathf.RoundToInt((yPhysicsBounds * world.y) / yWorldBounds)
            );
        }

        public Vector2 PhysicsToWorld(Vector2Int physics) {
            var xWorldBounds = Map.cellWidth * Map.mapSizeX;
            var yWorldBounds = Map.cellHeight * Map.mapSizeY;

            var xPhysicsBounds = UnitsPerCell * Map.mapSizeX;
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            return new Vector2(
                Map.transform.position.x + (xWorldBounds * physics.x) / xPhysicsBounds,
                Map.transform.position.y + (yWorldBounds * physics.y) / yPhysicsBounds
            );
        }

        public Vector2Int GridCellToPhysics(GridCell cell)
        {
            return new Vector2Int(
                Mathf.RoundToInt(cell.vector.x * UnitsPerCell),
                Mathf.RoundToInt(cell.vector.y * UnitsPerCell)
            );
        }


        public GridCell PhysicsToGridCell(Vector2Int physics)
        {
            var xCell = physics.x / UnitsPerCell;
            var yCell = physics.y / UnitsPerCell;

            return new GridCell(xCell, yCell);
        }

        BoxBounds GetEntityBounds(EngineEntity entity)
        {
            var worldBounds = entity.GameObject.GetComponent<BoxCollider2D>().bounds;
            var physicsExtents = WorldToPhysics(worldBounds.extents);

            var physicsUpperLeftBounds = entity.Position;

            physicsUpperLeftBounds.x -= Mathf.FloorToInt(physicsExtents.x / 2);
            physicsUpperLeftBounds.y += Mathf.FloorToInt(physicsExtents.y / 2);

            var physicsUpperRightBounds = entity.Position;

            physicsUpperRightBounds.x += Mathf.FloorToInt(physicsExtents.x / 2);
            physicsUpperRightBounds.y += Mathf.FloorToInt(physicsExtents.y / 2);

            var physicsLowerRightBounds = entity.Position;

            physicsLowerRightBounds.x += Mathf.FloorToInt(physicsExtents.x) / 2;
            physicsLowerRightBounds.y -= Mathf.FloorToInt(physicsExtents.y) / 2;

            var physicsLowerLeftBounds = entity.Position;

            physicsLowerLeftBounds.x -= Mathf.FloorToInt(physicsExtents.x) / 2;
            physicsLowerLeftBounds.y -= Mathf.FloorToInt(physicsExtents.y) / 2;

            return new BoxBounds
            {
                UpperLeft = physicsUpperLeftBounds,
                UpperRight = physicsUpperRightBounds,
                LowerLeft = physicsLowerLeftBounds,
                LowerRight = physicsLowerRightBounds
            };
        }
    }

}