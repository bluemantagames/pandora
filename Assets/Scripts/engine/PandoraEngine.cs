using Pandora.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.Engine
{
    public class PandoraEngine
    {
        uint tickTime = 5; // milliseconds in a tick
        public int UnitsPerCell = 400; // physics engine units per grid cell
        List<EngineEntity> entities = new List<EngineEntity> { };

        public MapComponent Map;
        uint totalElapsed = 0;

        Decimal DPi = new Decimal(3.141592653589);

        // Debug settings
        bool debugLines = true;
        float debugLinesDuration = 20f;

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
                    var prevPosition = entity.Path.Current;

                    entity.Path?.MoveNext();

                    var currentPosition = entity.Path.Current;

                    entity.Direction = currentPosition - prevPosition;
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

        /// <summary>
        /// Check if the target entity is inside a circular area with the
        /// source entity as its center
        /// </summary>
        /// <param name="sourceEntity">The source entity</param>
        /// <param name="targetEntity">The target entity</param>
        /// <param name="radius">The circle's radius</param>
        /// <returns>A boolean describing if the target entity is inside the circle</returns>
        public bool IsInCircularRange(EngineEntity sourceEntity, EngineEntity targetEntity, int radius)
        {
            // Here we are using the simple Euclidean Distance

            var sourceEntityBound = GetPooledEntityBounds(sourceEntity);
            var targetEntityBound = GetPooledEntityBounds(targetEntity);

            var p1 = new Vector2Int(sourceEntityBound.Center.x, sourceEntityBound.Center.y);
            var p2 = new Vector2Int(targetEntityBound.Center.x, targetEntityBound.Center.y);

            var distance = ISqrt(
                ((p1.x - p2.x) * (p1.x - p2.x)) + ((p1.y - p2.y) * (p1.y - p2.y))
            );

            if (Debug.isDebugBuild && debugLines)
            {
                var source = PhysicsToWorldArena(p1);
                var north = PhysicsToWorldArena(new Vector2Int(p1.x, p1.y + radius));
                var south = PhysicsToWorldArena(new Vector2Int(p1.x, p1.y - radius));
                var east = PhysicsToWorldArena(new Vector2Int(p1.x + radius, p1.y));
                var west = PhysicsToWorldArena(new Vector2Int(p1.x - radius, p1.y));

                Debug.DrawLine(new Vector3(source.x, source.y, 0f), new Vector3(north.x, north.y, 0f), Color.blue, debugLinesDuration, false);
                Debug.DrawLine(new Vector3(source.x, source.y, 0f), new Vector3(south.x, south.y, 0f), Color.blue, debugLinesDuration, false);
                Debug.DrawLine(new Vector3(source.x, source.y, 0f), new Vector3(east.x, east.y, 0f), Color.blue, debugLinesDuration, false);
                Debug.DrawLine(new Vector3(source.x, source.y, 0f), new Vector3(west.x, west.y, 0f), Color.blue, debugLinesDuration, false);
            }

            return distance <= radius;
        }

        /// <summary>
        /// Check if the target entity is inside a 2D triangle with the
        /// source entity as the main vertex
        /// </summary>
        /// <param name="sourceEntity">The source entity</param>
        /// <param name="targetEntity">The target entity</param>
        /// <param name="width">The triangle's width (as the "base")</param>
        /// <param name="height">The triangle's height (distance from the source entity)</param>
        /// <param name="unitsLeniency">Fix distance of the source entity from the main vertex</param>
        /// <returns>A boolean describing if the target entity is inside the triangle</returns>
        public bool IsInTriangularRange(EngineEntity sourceEntity, EngineEntity targetEntity, int width, int height, int unitsLeniency)
        {
            // Using barycentric coordinate system
            // (http://totologic.blogspot.com/2014/01/accurate-point-in-triangle-test.html)
            // This is NOT the most precise way.

            var sourceEntityBound = GetPooledEntityBounds(sourceEntity);
            var targetEntityBound = GetPooledEntityBounds(targetEntity);

            var v1 = PoolInstances.Vector2IntPool.GetObject();
            v1.x = sourceEntityBound.Center.x - (width / 2);
            v1.y = sourceEntityBound.Center.y + height - unitsLeniency;

            var v2 = PoolInstances.Vector2IntPool.GetObject();
            v2.x = sourceEntityBound.Center.x + (width / 2);
            v2.y = sourceEntityBound.Center.y + height - unitsLeniency;

            var v3 = PoolInstances.Vector2IntPool.GetObject();
            v3.x = sourceEntityBound.Center.x;
            v3.y = sourceEntityBound.Center.y - unitsLeniency;

            var target = PoolInstances.Vector2IntPool.GetObject();
            target.x = targetEntityBound.Center.x;
            target.y = targetEntityBound.Center.y;

            // Rotate the triangle
            var rotatedFigure = RotateFigureByDirection(
                new List<Vector2Int> { v1, v2, v3 },
                v3,
                sourceEntity.Direction
            );

            v3 = rotatedFigure[0];
            v1 = rotatedFigure[1];
            v2 = rotatedFigure[2];

            var denominator = ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));
            var a = ((v2.y - v3.y) * (target.x - v3.x) + (v3.x - v2.x) * (target.y - v3.y)) / denominator;
            var b = ((v3.y - v1.y) * (target.x - v3.x) + (v1.x - v3.x) * (target.y - v3.y)) / denominator;
            var c = 1 - a - b;

            // Debug the triangle
            if (Debug.isDebugBuild && debugLines)
            {
                var wv1 = PhysicsToWorldArena(v1);
                var wv2 = PhysicsToWorldArena(v2);
                var wv3 = PhysicsToWorldArena(v3);

                Debug.DrawLine(new Vector3(wv1.x, wv1.y, 0f), new Vector3(wv2.x, wv2.y, 0f), Color.red, debugLinesDuration, false);
                Debug.DrawLine(new Vector3(wv2.x, wv2.y, 0f), new Vector3(wv3.x, wv3.y, 0f), Color.red, debugLinesDuration, false);
                Debug.DrawLine(new Vector3(wv3.x, wv3.y, 0f), new Vector3(wv1.x, wv1.y, 0f), Color.red, debugLinesDuration, false);
            }

            return 0 <= a && a <= 1 && 0 <= b && b <= 1 && 0 <= c && c <= 1;
        }

        /// <summary>
        /// Check if the target entity is inside a "triangle with a rounded base"
        /// area with the source entity as the main vertex
        /// </summary>
        /// <param name="sourceEntity">The source entity</param>
        /// <param name="targetEntity">The target entity</param>
        /// <param name="width">The base of the triangle (not considering the rounded part)</param>
        /// <param name="height">The height of the triangle</param>
        /// <param name="unitsLeniency">Fix distance of the source entity from the main vertex</param>
        /// <returns></returns>
        public bool IsInConicRange(EngineEntity sourceEntity, EngineEntity targetEntity, int width, int height, int unitsLeniency)
        {
            var isInTriangularRange = IsInTriangularRange(sourceEntity, targetEntity, width, height, unitsLeniency);
            var isInCircularRange = IsInCircularRange(sourceEntity, targetEntity, height - unitsLeniency);

            return isInTriangularRange && isInCircularRange;
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

        /// <summary>
        /// [DEBUG FUNCTION] Transform physics coordinates to world coordinates
        /// with (0, 0) as the botton-left corner of the arena (I set them by hand
        /// so IT WILL BREAK)
        /// </summary>
        /// <param name="physics">The physics coordinates</param>
        /// <returns></returns>
        public Vector2 PhysicsToWorldArena(Vector2Int physics)
        {
            var xFix = Map.transform.position.x;
            var yFix = Map.transform.position.y;

            var xWorldBounds = Map.cellWidth * Map.mapSizeX;
            var yWorldBounds = Map.cellHeight * Map.mapSizeY;

            var xPhysicsBounds = UnitsPerCell * Map.mapSizeX;
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            return new Vector2(
                (xWorldBounds * physics.x) / xPhysicsBounds + xFix,
                (yWorldBounds * physics.y) / yPhysicsBounds + yFix
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

        /// <summary>
        /// Integer square root of a positive number
        /// </summary>
        /// <param name="num">The target number</param>
        /// <returns>The square root of the target number</returns>
        int ISqrt(int num)
        {
            if (num == 0) return 0;

            int n = (num / 2) + 1;  
            int n1 = (n + (num / n)) / 2;

            while (n1 < n)
            {
                n = n1;
                n1 = (n + (num / n)) / 2;
            }

            return n;
        }

        /// <summary>
        /// Calculate the square of a number
        /// </summary>
        int ISquare(int n) => n * n;

        Decimal DPow(Decimal n, int y)
        {
            if (y == 0) return 1;

            var result = PoolInstances.DecimalPool.GetObject();
            result = DPow(n, y / 2);

            if (y % 2 == 0)
                return result * result;
            else
                return n * result * result;
        }

        /// <summary>
        /// Transform a Def angle into a Rad angle
        /// using Decimal
        /// </summary>
        /// <param name="deg">The angle in Deg</param>
        /// <returns>A Rad angle</returns>
        Decimal DegToRad(Decimal deg) => deg * (DPi / 180);

        /// <summary>
        /// Normalize an angle into [0, 360]
        /// </summary>
        /// <param name="angle">A DEG angle in Decimal</param>
        /// <returns>A normalized angle</returns>
        Decimal NormalizeAngle(Decimal angle)
        {
            var normalized = PoolInstances.DecimalPool.GetObject();
            normalized = angle;

            // Reduce to [0, 360]
            while (normalized > 360) normalized -= 360;

            return normalized;
        }

        /// <summary>
        /// The the quadrant relative
        /// to an angle
        /// </summary>
        /// <param name="angle">A DEG angle in Decimal</param>
        /// <returns>A quadrant [1, 4]</returns>
        int GetAngleQuadrant(Decimal angle)
        {
            if (angle > 360)
                throw new Exception("Angle must be 0 <= a <= 360");

            if (angle <= 90)
                return 1;
            else if (angle <= 180)
                return 2;
            else if (angle <= 270)
                return 3;
            else
                return 4;
        }

        /// <summary>
        /// Map a [0, 360] DEG angle
        /// to a [0, 90] one
        /// </summary>
        /// <param name="angle">A DEG angle in Decimal</param>
        /// <returns>The new angle</returns>
        Decimal AngleToFirstQuadrant(Decimal angle)
        {
            var result = PoolInstances.DecimalPool.GetObject();
            var quadrant = GetAngleQuadrant(angle);

            if (quadrant == 2) result = 180 - angle;
            else if (quadrant == 3) result = angle - 180;
            else if (quadrant == 4) result = 360 - angle;
            else result = angle;

            return result;
        }

        /// <summary>
        /// Calculate the sine using Decimal
        /// </summary>
        /// <param name="angle">A DEG angle in Decimal</param>
        /// <returns>The sine of the angle</returns>
        Decimal DSin(Decimal angle)
        {
            var tempSin = PoolInstances.DecimalPool.GetObject();

            var fixedAngle = NormalizeAngle(angle);
            var quadrant = GetAngleQuadrant(fixedAngle);
            var fqAngle = AngleToFirstQuadrant(fixedAngle);

            if (fqAngle > 45)
                tempSin = DCos(90 - fqAngle);
            else
            {
                var radAngle = DegToRad(fqAngle);
                tempSin = radAngle - (DPow(radAngle, 3) / 6) + (DPow(radAngle, 5) / 120);
            }

            if (quadrant == 3 || quadrant == 4)
                return -tempSin;
            else
                return tempSin;
        }

        /// <summary>
        /// Calculate the cosine using Decimal
        /// </summary>
        /// <param name="angle">A DEG angle in Decimal</param>
        /// <returns>The cosine of the angle</returns>
        Decimal DCos(Decimal angle)
        {
            var tempCos = PoolInstances.DecimalPool.GetObject();

            var fixedAngle = NormalizeAngle(angle);
            var quadrant = GetAngleQuadrant(fixedAngle);
            var fqAngle = AngleToFirstQuadrant(fixedAngle);

            if (fqAngle > 45)
                tempCos = DSin(90 - fqAngle);
            else
            {
                var radAngle = DegToRad(fqAngle);
                tempCos = 1 - (DPow(radAngle, 2) / 2) + (DPow(radAngle, 4) / 24) - (DPow(radAngle, 6) / 720);
            }

            if (quadrant == 2 || quadrant == 3)
                return -tempCos;
            else
                return tempCos;
        }

        /// <summary>
        /// Rotate a figure _anticlockwise_ in a 2D space using Decimal
        /// (http://mathonweb.com/help_ebook/html/algorithms.htm#sin)
        /// </summary>
        /// <param name="figure">A list of vertex</param>
        /// <param name="pivot">The point of rotation</param>
        /// <param name="angle">The angle of rotatio in deg</param>
        /// <returns>The rotated figure</returns>
        List<Vector2Int> RotateFigureByAngle(List<Vector2Int> figure, Vector2Int pivot, Decimal angle)
        {
            List<Vector2Int> rotatedFigure = new List<Vector2Int>();

            // It should be safe (?)
            var sinAngle = DSin(angle);
            var cosAngle = DCos(angle);

            foreach (Vector2Int point in figure)
            {
                var fixedPoint = PoolInstances.Vector2IntPool.GetObject();
                var rotatedPoint = PoolInstances.Vector2IntPool.GetObject();

                fixedPoint.x = point.x - pivot.x;
                fixedPoint.y = point.y - pivot.y;

                // This truncate the decimal part, dunno if it's really ok
                rotatedPoint.x = Decimal.ToInt32((fixedPoint.x * cosAngle) - (fixedPoint.y * sinAngle) + pivot.x);
                rotatedPoint.y = Decimal.ToInt32((fixedPoint.x * sinAngle) + (fixedPoint.y * cosAngle) + pivot.y);

                rotatedFigure.Add(rotatedPoint);
            }

            return rotatedFigure;
        }

        /// <summary>
        /// Rotate a figure based on the direction using Decimal
        /// (It uses "RotateFigureByAngle" under the hood)
        /// </summary>
        /// <param name="figure">A list of vertex</param>
        /// <param name="pivot">The point of rotation</param>
        /// <param name="direction">A Vertex2int direction</param>
        /// <returns>The rotated figure</returns>
        List<Vector2Int> RotateFigureByDirection(List<Vector2Int> figure, Vector2Int pivot, Vector2Int direction)
        {
            if (direction.x == -1 && direction.y == 1)
                return RotateFigureByAngle(figure, pivot, 45);
            else if (direction.x == -1 && direction.y == 0)
                return RotateFigureByAngle(figure, pivot, 90);
            else if (direction.x == -1 && direction.y == -1)
                return RotateFigureByAngle(figure, pivot, 135);
            else if (direction.x == 0 && direction.y == -1)
                return RotateFigureByAngle(figure, pivot, 180);
            else if (direction.x == 1 && direction.y == -1)
                return RotateFigureByAngle(figure, pivot, 225);
            else if (direction.x == 1 && direction.y == 0)
                return RotateFigureByAngle(figure, pivot, 270);
            else if (direction.x == 1 && direction.y == 1)
                return RotateFigureByAngle(figure, pivot, 315);

            return figure;
        }
    }
}