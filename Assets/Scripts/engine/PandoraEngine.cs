using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora;
using Pandora.Pool;

namespace Pandora.Engine
{
    public class PandoraEngine
    {
        uint tickTime = 5; // milliseconds in a tick
        public int UnitsPerCell = 400; // physics engine units per grid cell
        List<EngineEntity> entities = new List<EngineEntity> { };

        public MapComponent Map;
        uint totalElapsed = 0;

        public PandoraEngine(MapComponent map)
        {
            this.Map = map;

            AddRiverEntities();
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

        public void AddRiverEntities()
        {
            var leftRiverObject = GameObject.Find("arena_water_left");

            var leftPosition = new Vector2Int(UnitsPerCell, 13 * UnitsPerCell + UnitsPerCell / 2);

            var leftEntity =
                AddEntity(leftRiverObject, 0, leftPosition, true, null);

            var rightRiverObject = GameObject.Find("arena_water_right");

            var rightPosition = new Vector2Int(
                (Map.mapSizeX - 1) * UnitsPerCell,
                (13 * UnitsPerCell) + UnitsPerCell / 2
            );

            var rightEntity =
                AddEntity(rightRiverObject, 0, rightPosition, true, null);

            var centerPosition = new GridCell(8, 13);

            var centerRiverObject = GameObject.Find("arena_water_center");

            var centerEntity =
                AddEntity(centerRiverObject, 0, centerPosition, true, null);

            centerEntity.IsStructure = true;
            centerEntity.IsMapObstacle = true;
            rightEntity.IsStructure = true;
            rightEntity.IsMapObstacle = true;
            leftEntity.IsStructure = true;
            leftEntity.IsMapObstacle = true;
        }

        public int GetSpeed(int engineUnitsPerSecond)
        {
            return Mathf.FloorToInt((engineUnitsPerSecond / 1000f) * tickTime);
        }

        public EngineEntity AddEntity(GameObject gameObject, int engineUnitsPerSecond, GridCell position, bool isRigid, DateTime? timestamp)
        {
            var physicsPosition = GridCellToPhysics(position) + (new Vector2Int(UnitsPerCell / 2, UnitsPerCell / 2));

            return AddEntity(gameObject, engineUnitsPerSecond, physicsPosition, isRigid, timestamp);
        }

        public EngineEntity AddEntity(GameObject gameObject, int engineUnitsPerSecond, Vector2Int position, bool isRigid, DateTime? timestamp)
        {
            var speed = GetSpeed(engineUnitsPerSecond);

            Debug.Log($"Assigning speed {speed} in {position}");

            var entity = new EngineEntity
            {
                Speed = speed,
                Position = position,
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

        public void PrintDebugInfo(EngineEntity entity)
        {
            var prefix = $"({entity.GameObject.name}) PrintDebugInfo:";
            var bounds = GetPooledEntityBounds(entity);

            Debug.Log($"{prefix} Position {entity.Position}");
            Debug.Log($"{prefix} Speed {entity.Speed}");
            Debug.Log($"{prefix} Hitbox {bounds}");

            ReturnBounds(bounds);
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
            var passes = 0;

            // Check for collisions
            for (var i1 = 0; i1 < clonedEntities.Count; i1++)
            {
                for (var i2 = 0; i2 < clonedEntities.Count; i2++)
                {
                    passes++;

                    if (passes > 5000)
                    {
                        throw new Exception("Cutting collision solving");
                    }

                    var first = clonedEntities[i1];
                    var second = clonedEntities[i2];

                    var firstBox = GetPooledEntityBounds(first);
                    var secondBox = GetPooledEntityBounds(second);

                    var notCollision =
                        (!first.IsRigid && !second.IsRigid) || // there must be one rigid object
                        first == second || // entity can't collide with itself
                        !firstBox.Collides(secondBox) || // boxes must collide
                        !CheckLayerCollision(first.Layer, second.Layer); // layer must be compatible for collisions

                    // continue if they don't collide
                    if (notCollision)
                    {
                        ReturnBounds(firstBox);
                        ReturnBounds(secondBox);

                        continue;
                    }

                    if (first.CollisionCallback != null)
                    {
                        first.CollisionCallback.Collided(second, totalElapsed);
                    }

                    if (second.CollisionCallback != null)
                    {
                        second.CollisionCallback.Collided(first, totalElapsed);
                    }

                    Vector2Int direction;
                    EngineEntity moved;
                    EngineEntity unmoved;

                    var isFirstFaster = false;

                    // Use "bounce" from collision to determine which object will move, if present
                    if (first.CollisionSpeed > 0 || second.CollisionSpeed > 0)
                    {
                        isFirstFaster = first.CollisionSpeed > second.CollisionSpeed;
                    }
                    else // use normal speed otherwise
                    {
                        isFirstFaster = first.Speed > second.Speed;
                    }

                    var isSecondMoving =
                        (isFirstFaster || first.IsStructure) && !second.IsStructure;

                    // move away the entity with less speed
                    if (isSecondMoving)
                    {
                        direction = second.Position - first.Position;
                        moved = second;
                        unmoved = first;
                    }
                    else if (first.Speed == second.Speed && first.CollisionSpeed == second.CollisionSpeed && !first.IsStructure && !second.IsStructure)
                    { // if speeds are equal, use server-generated timestamps to avoid non-deterministic behaviour
                        moved = (first.Timestamp > second.Timestamp) ? first : second;
                        unmoved = (first.Timestamp > second.Timestamp) ? second : first;
                        direction = moved.Position - unmoved.Position;
                    }
                    else
                    {
                        direction = first.Position - second.Position;
                        moved = first;
                        unmoved = second;
                    }

                    direction = new Vector2Int(
                        Clamp(-1, direction.x, 1),
                        Clamp(-1, direction.y, 1)
                    );

                    if (direction.x == 0 && direction.y == 0)
                    {
                        direction.x = 1;
                        direction.y = 1;
                    }

                    if (moved.IsRigid && unmoved.IsRigid)
                    {
                        // recheck for collisions once solved
                        i1 = 0;
                        i2 = 0;

                        while (firstBox.Collides(secondBox)) // there probably is a math way to do this without a loop
                        {
                            moved.Position = moved.Position + direction; // move the entity away

                            ReturnBounds(firstBox);
                            ReturnBounds(secondBox);

                            firstBox = GetPooledEntityBounds(first);
                            secondBox = GetPooledEntityBounds(second);
                        }

                        moved.ResetTarget(); // Reset engine-side pathing
                        moved.CollisionSpeed++; // Give the moved entity some speed

                        if (unmoved.IsStructure)
                        { // Give the moved entity even more speed if pushed by a structure (to avoid nasty loops)
                            moved.CollisionSpeed++;
                        }
                    }

                    ReturnBounds(firstBox);
                    ReturnBounds(secondBox);
                }
            }

            foreach (var entity in clonedEntities)
            {
                entity.CollisionSpeed = 0; // Once collisions are solved, remove collision speed

                var engineComponent = entity.GameObject.GetComponent<EngineComponent>();

                if (engineComponent == null) continue;

                foreach (var component in entity.GameObject.GetComponent<EngineComponent>().Components)
                {
                    component.TickUpdate(tickTime);
                }
            }
        }

        public void DrawDebugGUI()
        {
            foreach (var entity in entities)
            {
                var boxBounds = GetPooledEntityBounds(entity);

                var rect = Rect.zero;

                rect.xMin = Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.UpperLeft)).x;
                rect.yMin = ScreenToGUISpace(Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.UpperLeft))).y;
                rect.xMax = Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.UpperRight)).x;
                rect.yMax = ScreenToGUISpace(Camera.main.WorldToScreenPoint(PhysicsToMap(boxBounds.LowerLeft))).y;

                GUI.Box(rect, GUIContent.none);

                ReturnBounds(boxBounds);
            }
        }

        public void RemoveEntity(EngineEntity entity)
        {
            entities.Remove(entity);
        }

        public bool IsInHitboxRangeCells(EngineEntity entity1, EngineEntity entity2, int gridCellRange)
        {
            return IsInHitboxRange(entity1, entity2, gridCellRange * UnitsPerCell);
        }

        int Square(int a) => a * a;
        public int SquaredDistance(Vector2Int first, Vector2Int second) => Square(first.x - second.x) + Square(first.y - second.y);

        public bool IsInHitboxRange(EngineEntity entity1, EngineEntity entity2, int units)
        {
            var entity1Bounds = GetPooledEntityBounds(entity1);
            var entity2Bounds = GetPooledEntityBounds(entity2);

            var distance = Math.Max(
                Math.Abs(entity1Bounds.Center.x - entity2Bounds.Center.x) - ((entity1Bounds.Width + entity2Bounds.Width) / 2),
                Math.Abs(entity1Bounds.Center.y - entity2Bounds.Center.y) - ((entity1Bounds.Height + entity2Bounds.Height) / 2)
            );

            ReturnBounds(entity1Bounds);
            ReturnBounds(entity2Bounds);

            return distance <= units;
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

        public List<EngineEntity> FindInGridCell(GridCell gridCell, bool countStructures)
        { // TODO: Maybe use a quad tree for < O(n) search
            var physics = GridCellToPhysics(gridCell);

            List<EngineEntity> targetEntities = new List<EngineEntity> { };

            foreach (var entity in entities)
            {
                var isNotTargeted = (entity.IsStructure && !countStructures) || entity.IsMapObstacle;

                if (isNotTargeted) continue;

                var box = GetPooledEntityBounds(entity);

                if (box.Contains(physics))
                {
                    targetEntities.Add(entity);
                }

                ReturnBounds(box);
            }

            return targetEntities;
        }

        public List<EngineEntity> FindInHitboxRange(EngineEntity origin, int range, bool countStructures)
        {
            List<EngineEntity> targetEntities = new List<EngineEntity> { };

            foreach (var entity in entities)
            {
                var isNotTargeted = (entity.IsStructure && !countStructures) || entity.IsMapObstacle;

                if (isNotTargeted) continue;

                if (IsInHitboxRange(origin, entity, range))
                {
                    targetEntities.Add(entity);
                }
            }

            return targetEntities;
        }

        void ReturnBounds(BoxBounds bounds)
        {
            PoolInstances.BoxBoundsPool.ReturnObject(bounds);
        }

        BoxBounds GetPooledEntityBounds(EngineEntity entity)
        {
            var bounds = PoolInstances.BoxBoundsPool.GetObject();
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

            bounds.UpperLeft = physicsUpperLeftBounds;
            bounds.UpperRight = physicsUpperRightBounds;
            bounds.LowerLeft = physicsLowerLeftBounds;
            bounds.LowerRight = physicsLowerRightBounds;
            bounds.Center = entity.Position;

            return bounds;
        }

        int Clamp(int min, int input, int max)
        {
            if (input < min) return min;
            if (input > max) return max;

            return input;
        }

        bool CheckLayerCollision(int layer1, int layer2)
        {
            return
                layer1 == layer2 ||
                (layer1 == Constants.PROJECTILES_LAYER || layer2 == Constants.PROJECTILES_LAYER);
        }
    }

}