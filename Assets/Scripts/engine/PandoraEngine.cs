using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora;

namespace Pandora.Engine
{
    public class PandoraEngine
    {
        int tickTime = 5; // milliseconds in a tick
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
            var ticksNum = msLapsed / tickTime;

            totalElapsed += msLapsed;

            GameObject.Find("MsElapsedText").GetComponent<Text>().text = $"Elapsed: {totalElapsed}";

            for (var tick = 0; tick < ticksNum; tick++)
            {
                NextTick();
            }
        }

        public EngineEntity AddEntity(GameObject gameObject, float cellPerSecond, GridCell position, bool isRigid, DateTime? timestamp)
        {
            var speed = Mathf.FloorToInt((cellPerSecond * UnitsPerCell / 1000) * tickTime);

            Debug.Log($"Assigning speed {speed}");

            var physicsPosition = GridCellToPhysics(position) + (new Vector2Int(UnitsPerCell / 2, UnitsPerCell / 2));

            var entity = new EngineEntity
            {
                Speed = speed,
                Position = physicsPosition,
                GameObject = gameObject,
                Direction = new Vector2Int(0, 0),
                Engine = this,
                IsRigid = isRigid,
                Layer = gameObject.layer,
                Timestamp = timestamp ?? DateTime.Now
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
                    var notCollision =
                        (!first.IsRigid && !second.IsRigid) || // there must be one rigid object
                        first == second || // entity can't collide with itself
                        !firstBox.Collides(secondBox) || // boxes must collide
                        !CheckLayerCollision(first.Layer, second.Layer); // layer must be compatible for collisions

                    if (notCollision)
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
                    if (first.Speed > second.Speed)
                    {
                        direction = second.Position - first.Position;
                        moved = second;
                    }
                    else if (first.Speed == second.Speed) { // if speeds are equal, use server-generated timestamps to avoid non-deterministic behaviour
                        moved = (first.Timestamp > second.Timestamp) ? first : second;

                        var unmoved = (first.Timestamp > second.Timestamp) ? second : first;

                        direction = moved.Position - unmoved.Position;
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

                    if (first.IsRigid && second.IsRigid && !moved.IsStructure) // resolve collision if both objects are rigid and we don't move a structure hitbox
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
                    unitBehaviour.UnitUpdate(tickTime);
                }
            }
        }

        public void DrawDebugGUI()
        {
            foreach (var entity in entities)
            {
                var boxBounds = GetEntityBounds(entity);

                var rect = Rect.zero;

                rect.xMin = Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.UpperLeft)).x;
                rect.yMin = ScreenToGUISpace(Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.UpperLeft))).y;
                rect.xMax = Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.UpperRight)).x;
                rect.yMax = ScreenToGUISpace(Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.LowerLeft))).y;

                GUI.Box(rect, GUIContent.none);
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

            var distance = Math.Max(
                Math.Abs(entity1Bounds.Center.x - entity2Bounds.Center.x) - ((entity1Bounds.Width + entity2Bounds.Width) / 2),
                Math.Abs(entity1Bounds.Center.y - entity2Bounds.Center.y) - ((entity1Bounds.Height + entity2Bounds.Height) / 2)
            );

            return distance <= (gridCellRange * UnitsPerCell);
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

        Vector2 ScreenToGUISpace(Vector2 screen)
        {
            return new Vector2(screen.x, Screen.height - screen.y);
        }

        Rect ScreenToGUIRect(Rect rect)
        {
            return new Rect(ScreenToGUISpace(rect.position), rect.size);
        }

        public Vector2 PhysicsToMap(Vector2Int physics)
        {
            return PhysicsToWorld(physics) + (Vector2)Map.transform.position;
        }


        /// <summary>Flips the position around the map, used for e.g. render units when top team</summary>
        public Vector2 FlippedPhysicsToMap(Vector2Int physics)
        {
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            var flippedPhysics = physics;

            flippedPhysics.y = yPhysicsBounds - flippedPhysics.y;

            return PhysicsToMap(flippedPhysics);
        }


        public Vector2 PhysicsToWorld(Vector2Int physics)
        {
            var xWorldBounds = Map.cellWidth * Map.mapSizeX;
            var yWorldBounds = Map.cellHeight * Map.mapSizeY;

            var xPhysicsBounds = UnitsPerCell * Map.mapSizeX;
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            return new Vector2(
                (xWorldBounds * physics.x) / xPhysicsBounds,
                (yWorldBounds * physics.y) / yPhysicsBounds
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
            var physicsExtents = WorldToPhysics(worldBounds.size);

            var physicsUpperLeftBounds = entity.Position;

            physicsUpperLeftBounds.x -= Mathf.FloorToInt(physicsExtents.x / 2);
            physicsUpperLeftBounds.y += Mathf.FloorToInt(physicsExtents.y / 2);

            var physicsUpperRightBounds = entity.Position;

            physicsUpperRightBounds.y += Mathf.FloorToInt(physicsExtents.y / 2);
            physicsUpperRightBounds.x += Mathf.FloorToInt(physicsExtents.x / 2);

            var physicsLowerRightBounds = entity.Position;

            physicsLowerRightBounds.x += Mathf.FloorToInt(physicsExtents.x / 2);
            physicsLowerRightBounds.y -= Mathf.FloorToInt(physicsExtents.y / 2);

            var physicsLowerLeftBounds = entity.Position;

            physicsLowerLeftBounds.x -= Mathf.FloorToInt(physicsExtents.x / 2);
            physicsLowerLeftBounds.y -= Mathf.FloorToInt(physicsExtents.y / 2);

            return new BoxBounds
            {
                UpperLeft = physicsUpperLeftBounds,
                UpperRight = physicsUpperRightBounds,
                LowerLeft = physicsLowerLeftBounds,
                LowerRight = physicsLowerRightBounds,
                Center = entity.Position
            };
        }

        bool CheckLayerCollision(int layer1, int layer2)
        {
            return
                layer1 == layer2 ||
                (layer1 == Constants.PROJECTILES_LAYER || layer2 == Constants.PROJECTILES_LAYER);
        }
    }

}