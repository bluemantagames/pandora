using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.Engine
{
    public class PandoraEngine
    {
        int minTick = 5; // milliseconds, minimum tick
        public int UnitsPerCell = 400; // physics engine units per grid cell
        List<EngineEntity> entities = new List<EngineEntity> { };

        public MapComponent Map;
        uint totalElapsed = 0;

        public PandoraEngine(MapComponent map)
        {
            this.Map = map;
        }

        public void Process(uint msLapsed)
        {
            var ticksNum = msLapsed / minTick;

            totalElapsed += msLapsed;

            GameObject.Find("MsElapsedText").GetComponent<Text>().text = $"Elapsed: {totalElapsed}";

            for (var tick = 0; tick < ticksNum; tick++)
            {
                NextTick();
            }
        }

        public EngineEntity AddEntity(GameObject gameObject, float cellPerSecond, GridCell position, bool isRigid)
        {
            var speed = Mathf.FloorToInt((cellPerSecond * UnitsPerCell / 1000) * minTick);

            Debug.Log($"Assigning speed {speed}");

            var physicsPosition = GridCellToPhysics(position) + (new Vector2Int(UnitsPerCell / 2, UnitsPerCell / 2));

            var entity = new EngineEntity
            {
                Speed = speed,
                Position = physicsPosition,
                GameObject = gameObject,
                Direction = new Vector2Int(0, 0),
                Engine = this,
                IsRigid = isRigid
            };

            entities.Add(entity);

            return entity;
        }

        public void NextTick()
        {
            // Move units
            foreach (var entity in entities)
            {
                var unitsMoved = Mathf.FloorToInt(Mathf.Max(1f, entity.Speed));

                if (entity.Path == null) continue;

                for (var i = 0; i < unitsMoved; i++)
                {
                    entity.Path?.MoveNext();
                }

                if (entity.Path.Current != null)
                {
                    entity.Position = entity.Path.Current;
                }
            }

            // We clone the entities list while we iterate because the collision callbacks
            // might want to modify the entity list somehow (e.g. remove a projectile on collision)
            // TODO: A more efficient way to do this is to have a flag be true while we are checking collisions,
            // cache away all the removals/adds and execute them later
            var clonedEntities = new List<EngineEntity>(entities);

            // Check for collisions
            foreach (var first in clonedEntities)
            {
                foreach (var second in clonedEntities)
                {
                    var firstBox = GetEntityBounds(first);
                    var secondBox = GetEntityBounds(second);

                    // continue if they don't collide
                    if (first == second || !firstBox.Collides(secondBox))
                    {
                        continue;
                    }

                    if (first.CollisionCallback != null)
                    {
                        first.CollisionCallback.Collided(second);
                    }

                    if (second.CollisionCallback != null)
                    {
                        second.CollisionCallback.Collided(first);
                    }

                    Vector2Int direction;
                    EngineEntity moved;

                    // move away the entity with less speed
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

                    direction.x = (int)Mathf.Clamp(-1f, (float)direction.x, 1f);
                    direction.y = (int)Mathf.Clamp(-1f, (float)direction.y, 1f);

                    if (direction.x == 0 && direction.y == 0)
                    {
                        direction.x = 1;
                        direction.y = 1;
                    }

                    if (first.IsRigid && second.IsRigid) // resolve collision if both objects are rigid
                    {
                        while (firstBox.Collides(secondBox)) // there probably is a math way to do this without a loop
                        {
                            moved.Position = moved.Position + direction; // move the entity away

                            firstBox = GetEntityBounds(first);
                            secondBox = GetEntityBounds(second);
                        }
                    }
                }
            }

            foreach (var entity in clonedEntities)
            {
                var unitBehaviour = entity.GameObject.GetComponent<UnitBehaviour>();

                if (unitBehaviour != null)
                {
                    unitBehaviour.UnitUpdate();
                }
            }
        }

        public void RemoveEntity(EngineEntity entity)
        {
            entities.Remove(entity);
        }

        public bool IsInRange(EngineEntity entity1, EngineEntity entity2, int gridCellRange)
        {
            var entity1Bounds = GetEntityBounds(entity1);
            var entity2Bounds = GetEntityBounds(entity2);

            var distance =  Math.Max(
                Math.Abs(entity1Bounds.Center.x - entity2Bounds.Center.x) - ((entity1Bounds.Width + entity2Bounds.Width) / 2),
                Math.Abs(entity1Bounds.Center.y - entity2Bounds.Center.y) - ((entity1Bounds.Height + entity2Bounds.Height) / 2)
            );

            return Math.Max(
                Math.Abs(entity1Bounds.Center.x - entity2Bounds.Center.x) - (entity1Bounds.Width + entity2Bounds.Width) / 2,
                Math.Abs(entity1Bounds.Center.y - entity2Bounds.Center.y) - (entity1Bounds.Height + entity2Bounds.Height) / 2
            ) <= (gridCellRange * UnitsPerCell);
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

        public Vector2 PhysicsToWorld(Vector2Int physics)
        {
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

            physicsUpperLeftBounds.y += Mathf.FloorToInt(physicsExtents.y);

            var physicsUpperRightBounds = entity.Position;

            physicsUpperRightBounds.x += Mathf.FloorToInt(physicsExtents.x);
            physicsUpperRightBounds.y += Mathf.FloorToInt(physicsExtents.y);

            var physicsLowerRightBounds = entity.Position;

            physicsLowerRightBounds.x += Mathf.FloorToInt(physicsExtents.x);

            var physicsLowerLeftBounds = entity.Position;

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