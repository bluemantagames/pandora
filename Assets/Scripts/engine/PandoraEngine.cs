using Pandora.Pool;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using Pandora.Engine.Grid;
using Pandora.Network;
using Pandora.Network.Messages;
using System.Threading.Tasks;
using System.Threading;

namespace Pandora.Engine
{
    [Serializable]
    public class PandoraEngine : ScriptableObject
    {
        public uint TickTime = 40; // milliseconds in a tick
        public int UnitsPerCell = 400; // physics engine units per grid cell
        public List<EngineEntity> Entities = new List<EngineEntity> { };
        public List<EngineBehaviour> Behaviours = new List<EngineBehaviour> { };
        [NonSerialized] public MapComponent Map;
        public uint TotalElapsed = 0;
        BoxBounds mapBounds, riverBounds;
        CustomSampler collisionsSampler, collisionsSolveSampler, collisionsCheckSampler, collisionsCallbackSampler, movementSampler, scriptSampler, gridSampler, enginePathfindingSampler;
        Dictionary<(EngineEntity, EngineEntity), int> collisionsCount = new Dictionary<(EngineEntity, EngineEntity), int>(4000);
        public List<(long, Action)> DelayedJobs = new List<(long, Action)>(300);

        [NonSerialized] public bool DebugEngine;

        Decimal DPi = new Decimal(3.141592653589);

        TightGrid grid;

        Astar<GridCell> astar = new Astar<GridCell>(
            PoolInstances.GridCellHashSetPool,
            PoolInstances.GridCellQueueItemPool,
            PoolInstances.GridCellPool,
            PoolInstances.GridCellListPool
        );

        // Debug settings
        float debugLinesDuration = 1f;

        // Engine snapshow settinsg
        uint snapshotEvery = 1000;

        // This is used just to serialize the behaviours
        public List<SerializableEngineBehaviour> SerializableBehaviours = new List<SerializableEngineBehaviour> { };

        public void Init(MapComponent map)
        {
            this.Map = map;

            AddRiverEntities();

            mapBounds = new BoxBounds();

            var xBounds = UnitsPerCell * map.mapSizeX;
            var yBounds = UnitsPerCell * map.mapSizeY;

            mapBounds.LowerLeft = new Vector2Int(0, 0);
            mapBounds.LowerRight = new Vector2Int(xBounds, 0);
            mapBounds.UpperLeft = new Vector2Int(0, yBounds);
            mapBounds.UpperRight = new Vector2Int(xBounds, yBounds);
            mapBounds.Center = new Vector2Int(xBounds / 2, yBounds / 2);

            var riverCenterEntity = GameObject.Find("arena_water_center").GetComponent<EngineComponent>().Entity;

            riverBounds = GetPooledEntityBounds(riverCenterEntity);

            PoolInstances.BoxBoundsPool.MaximumPoolSize = 1000;
            PoolInstances.Vector2IntQueueItemPool.MaximumPoolSize = 5000;

            astar.DebugPathfinding = false;

            DebugEngine = Debug.isDebugBuild;

            collisionsSampler = CustomSampler.Create("Collisions sampler");
            collisionsSolveSampler = CustomSampler.Create("Collisions solver sampler");
            collisionsCheckSampler = CustomSampler.Create("Collisions check sampler");
            collisionsCallbackSampler = CustomSampler.Create("Collisions callback sampler");
            movementSampler = CustomSampler.Create("Movement sampler");
            scriptSampler = CustomSampler.Create("Script sampler");
            gridSampler = CustomSampler.Create("Grid building sampler");
            enginePathfindingSampler = CustomSampler.Create("PandoraEngine pathfinding");

            grid = new TightGrid(yBounds, xBounds, 19, 32);
        }

        public void Process(uint msLapsed)
        {
            var ticksNum = msLapsed / TickTime;

            TotalElapsed += msLapsed;

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
                AddEntity(leftRiverObject, 0, leftPosition, true, SafeGenerateTimestamp(leftRiverObject));

            var rightRiverObject = GameObject.Find("arena_water_right");

            var rightPosition = new Vector2Int(
                (Map.mapSizeX - 1) * UnitsPerCell,
                (13 * UnitsPerCell) + UnitsPerCell / 2
            );

            var rightEntity =
                AddEntity(rightRiverObject, 0, rightPosition, true, SafeGenerateTimestamp(rightRiverObject));

            var centerPosition = new Vector2Int(8 * UnitsPerCell, 13 * UnitsPerCell + (UnitsPerCell / 2));

            var centerRiverObject = GameObject.Find("arena_water_center");

            var centerEntity =
                AddEntity(centerRiverObject, 0, centerPosition, true, SafeGenerateTimestamp(centerRiverObject));

            centerEntity.IsStructure = true;
            centerEntity.IsMapObstacle = true;
            centerEntity.Layer = Constants.WATER_LAYER;

            var centerComponent = centerRiverObject.AddComponent<EngineComponent>();

            centerComponent.Entity = centerEntity;

            rightEntity.IsStructure = true;
            rightEntity.IsMapObstacle = true;
            rightEntity.Layer = Constants.WATER_LAYER;


            var rightComponent = rightRiverObject.AddComponent<EngineComponent>();

            rightComponent.Entity = rightEntity;

            leftEntity.IsStructure = true;
            leftEntity.IsMapObstacle = true;
            leftEntity.Layer = Constants.WATER_LAYER;

            var leftComponent = leftRiverObject.AddComponent<EngineComponent>();

            leftComponent.Entity = leftEntity;
        }

        public int GetSpeed(int engineUnitsPerSecond) =>
            Mathf.FloorToInt((engineUnitsPerSecond / 1000f) * TickTime);

        public IEnumerator<GridCell> FindPath(EngineEntity entity, Vector2Int target)
        {
            enginePathfindingSampler.Begin();

            var entityBounds = GetEntityBounds(entity);

            var unitsBounds =
                (from unit in Entities
                 where !unit.IsStructure && CanCollide(unit, entity)
                 select (bounds: GetPooledEntityBounds(unit), unit: unit)).ToList();

            var isFlying = entity.GameObject.layer == Constants.FLYING_LAYER;
            var team = entity.GameObject.GetComponent<TeamComponent>();

            var currentGridCell = PooledPhysicsToGridCell(entity.Position);
            var endGridCell = PooledPhysicsToGridCell(target);

            var path = astar.FindPathEnumerator(
                currentGridCell,
                endGridCell,
                position =>
                {
                    if (position == endGridCell) return false;

                    var physics = PooledGridCellToPhysics(position);

                    entityBounds.Translate(physics);

                    var isCollision = grid.Collide(
                        (a, b) =>
                        {
                            if (a.IsStructure || b.IsStructure)
                                return false;
                            else
                                return CanCollide(a, b);
                        },
                        entity,
                        entityBounds
                    );

                    PoolInstances.Vector2IntPool.ReturnObject(physics);

                    return isCollision || MapComponent.Instance.IsObstacle(
                        position,
                        entity.GameObject.layer == Constants.FLYING_LAYER,
                        entity.GameObject.GetComponent<TeamComponent>()
                    );
                },
                position =>
                {
                    var surroundingPositions = PoolInstances.GridCellListPool.GetObject();

                    for (var x = -1; x <= 1; x++)
                    {
                        for (var y = -1; y <= 1; y++)
                        {
                            var advance = PoolInstances.GridCellPool.GetObject();

                            advance.vector.x = position.vector.x + x;
                            advance.vector.y = position.vector.y + y;

                            surroundingPositions.Add(advance);
                        }
                    }

                    return surroundingPositions;
                },
                (a, b) => Vector2.Distance(a.vector, b.vector),
                false
            );

            PoolInstances.GridCellPool.ReturnObject(currentGridCell);
            PoolInstances.GridCellPool.ReturnObject(endGridCell);

            entity.IsEvading = false;

            foreach (var (bounds, _) in unitsBounds)
            {
                ReturnBounds(bounds);
            }

            enginePathfindingSampler.End();

            return path;
        }

        public EngineEntity AddEntity(GameObject gameObject, int engineUnitsPerSecond, GridCell position, bool isRigid, DateTime timestamp)
        {
            var physicsPosition = GridCellToPhysics(position) + (new Vector2Int(UnitsPerCell / 2, UnitsPerCell / 2));

            return AddEntity(gameObject, engineUnitsPerSecond, physicsPosition, isRigid, timestamp);
        }

        public EngineEntity AddEntity(GameObject gameObject, int engineUnitsPerSecond, Vector2Int position, bool isRigid, DateTime timestamp)
        {
            var speed = GetSpeed(engineUnitsPerSecond);

            Logger.Debug($"Assigning speed {speed} in {position}");

            var entity = new EngineEntity
            {
                Speed = speed,
                Position = position,
                GameObject = gameObject,
                Direction = new Vector2Int(0, 0),
                Engine = this,
                IsRigid = isRigid,
                Layer = gameObject.layer,
                Timestamp = timestamp
            };

            Entities.Add(entity);

            Entities.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

            return entity;
        }

        /// <summary>
        /// Generate a "safe", deterministic and unique-ish timestamp
        /// using the GameObject informations.
        /// 
        /// (This method is mainly used to generate a unique timestamp value for 
        /// static game objects. It must be deterministic since this timestamp 
        /// will be used to sort the game objects inside the engine and we need
        /// them in the same exact order across platforms)
        /// </summary>
        /// <param name="gameObject">GameObject used to generate the timestamp</param>
        /// <returns>A valid DateTime</returns>
        public static DateTime SafeGenerateTimestamp(GameObject gameObject)
        {
            var name = gameObject.name;
            var diff = Encoding.ASCII.GetBytes(name).Select(b => (int)b).Sum();
            var epoch = System.DateTime.MinValue;

            return epoch.AddSeconds(diff);
        }

        public void AddBehaviour(EngineBehaviour behaviour)
        {
            Behaviours.Add(behaviour);
        }

        public void PrintDebugInfo(EngineEntity entity)
        {
            var prefix = $"({entity.GameObject.name}) PrintDebugInfo:";
            var bounds = GetPooledEntityBounds(entity);

            Logger.Debug($"{prefix} Position {entity.Position}");
            Logger.Debug($"{prefix} Speed {entity.Speed}");
            Logger.Debug($"{prefix} Path {entity.Path}");
            Logger.Debug($"{prefix} Hitbox {bounds}");

            ReturnBounds(bounds);
        }

        bool CanCollide(EngineEntity first, EngineEntity second)
        {
            return
                (first.IsRigid || second.IsRigid) && // there must be one rigid object
                first != second && // entity can't collide with itself
                !(first.IsMapObstacle && second.IsMapObstacle) && // two map obstacles do not collide with each other
                CheckLayerCollision(first.Layer, second.Layer); // layer must be compatible for collisions
        }

        void BuildGrid(List<EngineEntity> entities)
        {
            gridSampler.Begin();

            grid.Clear();

            foreach (var entity in entities)
            {
                grid.Insert(entity);
            }

            gridSampler.End();
        }

        public void NextTick()
        {
            if (DebugEngine) movementSampler.Begin();
            // Move units
            foreach (var entity in Entities)
            {
                var unitsMoved = Mathf.FloorToInt(Mathf.Max(1f, entity.Speed));

                if (entity.Path == null) continue;

                for (var i = 0; i < unitsMoved; i++)
                {
                    var prevPosition = entity.Path.Current;

                    entity.Path.MoveNext();

                    var currentPosition = entity.Path.Current;

                    // if we exausted the current path
                    if (currentPosition == null || prevPosition == currentPosition)
                    {
                        entity.SetEmptyPath();

                        break;
                    }
                    else
                    {
                        entity.Direction = currentPosition - prevPosition;
                    }
                }

                if (entity.Path?.Current != null)
                {
                    entity.Position = entity.Path.Current;
                }
            }

            if (DebugEngine) movementSampler.End();

            // We clone the entities list while we iterate because the collision callbacks
            // might want to modify the entity list somehow (e.g. remove a projectile on collision)
            // TODO: A more efficient way to do this is to have a flag be true while we are checking collisions,
            // cache away all the removals/adds and execute them later
            var clonedEntities = new List<EngineEntity>(Entities);
            var collisionsNum = -1;
            var collisionSolvedCount = 0;

            if (DebugEngine) collisionsSampler.Begin();

            while (collisionsNum != 0)
            {
                BuildGrid(clonedEntities);

                collisionsNum = 0;
                collisionSolvedCount++;

                foreach (var collision in grid.Collisions(CanCollide))
                {

                    collisionsCheckSampler.Begin();
                    var first = collision.First;
                    var second = collision.Second;

                    var firstBox = collision.FirstBox;
                    var secondBox = collision.SecondBox;

                    collisionsNum++;

                    collisionsCallbackSampler.Begin();
                    if (first.CollisionCallback != null)
                    {
                        first.CollisionCallback.Collided(second, TotalElapsed);
                    }

                    if (second.CollisionCallback != null)
                    {
                        second.CollisionCallback.Collided(first, TotalElapsed);
                    }
                    collisionsCallbackSampler.End();

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

                    collisionsCheckSampler.End();

                    collisionsSolveSampler.Begin();

                    var collisionKey = (moved, unmoved);

                    var collisionCount = collisionsCount.ContainsKey(collisionKey) ? collisionsCount[collisionKey] : 0;

                    if (moved.IsRigid && unmoved.IsRigid && collisionCount < 3)
                    {
                        var movedFirstBox = GetPooledEntityBounds(first);
                        var movedSecondBox = GetPooledEntityBounds(second);

                        while (movedFirstBox.Collides(movedSecondBox)) // there probably is a math way to do this without a loop
                        {
                            moved.Position = moved.Position + direction; // move the entity away

                            ReturnBounds(movedFirstBox);
                            ReturnBounds(movedSecondBox);

                            movedFirstBox = GetPooledEntityBounds(first);
                            movedSecondBox = GetPooledEntityBounds(second);
                        }

                        moved.ResetTarget(); // Reset engine-side pathing
                        moved.CollisionSpeed++; // Give the moved entity some speed

                        if (unmoved.IsStructure)
                        { // Give the moved entity even more speed if pushed by a structure or obstacle (to avoid nasty loops)
                            moved.CollisionSpeed++;
                        }

                        if (!collisionsCount.ContainsKey(collisionKey))
                            collisionsCount[collisionKey] = 1;
                        else
                            collisionsCount[collisionKey]++;
                    }
                    else
                    {
                        // don't count the collision if objects don't move
                        collisionsNum--;
                    }

                    collisionsSolveSampler.End();
                }

                if (collisionSolvedCount > 30)
                {
                    Debug.LogError($"Cutting collision solving");

                    if (DebugEngine) collisionsSampler.End();

                    return;
                }
            }

            collisionsCount.Clear();

            if (DebugEngine) collisionsSampler.End();

            if (DebugEngine) scriptSampler.Begin();

            foreach (var entity in clonedEntities)
            {
                entity.CollisionSpeed = 0; // Once collisions are solved, remove collision speed

                var engineComponent = entity.GameObject.GetComponent<EngineComponent>();

                if (engineComponent == null) continue;

                foreach (var component in entity.GameObject.GetComponent<EngineComponent>().Components)
                {
                    if (DebugEngine) Profiler.BeginSample(component.ComponentName);

                    component.TickUpdate(TickTime);

                    if (DebugEngine) Profiler.EndSample();
                }
            }

            if (DebugEngine) scriptSampler.End();

            foreach (var behaviour in Behaviours)
            {
                behaviour.TickUpdate(TickTime);
            }

            for (var i = 0; i < DelayedJobs.Count; i++)
            {
                var (passed, job) = DelayedJobs[i];

                var updatedPassed = passed - TickTime;

                if (updatedPassed <= 0)
                {
                    DelayedJobs.RemoveAt(i);

                    job();
                }
                else
                {
                    DelayedJobs[i] = (updatedPassed, job);
                }
            }

            // Snapshot
            if (TotalElapsed % snapshotEvery == 0)
            {
                sendEngineSnapshot();
            }
        }

        void sendEngineSnapshot()
        {
            SerializableBehaviours.Clear();

            foreach (var behaviour in Behaviours) 
            {
                SerializableBehaviours.Add(new SerializableEngineBehaviour(behaviour.ComponentName));
            }

            var engineSnapshot = JsonUtility.ToJson(this);
            var team = TeamComponent.assignedTeam;

            var snapshotMessage = new EngineSnapshotMessage
            {
                Snapshot = engineSnapshot,
                Timestamp = DateTime.Now,
                ElapsedMs = TotalElapsed,
                Team = team
            };

            NetworkControllerSingleton.instance.EnqueueMessage(snapshotMessage);
        }

        public void DrawDebugGUI()
        {
            foreach (var entity in Entities)
            {
                var boxBounds = GetPooledEntityBounds(entity);

                var rect = Rect.zero;

                rect.xMin = Camera.main.WorldToScreenPoint(PhysicsToMapWorldUnflipped(boxBounds.UpperLeft)).x;
                rect.yMin = ScreenToGUISpace(Camera.main.WorldToScreenPoint(PhysicsToMapWorldUnflipped(boxBounds.UpperLeft))).y;
                rect.xMax = Camera.main.WorldToScreenPoint(PhysicsToMapWorldUnflipped(boxBounds.UpperRight)).x;
                rect.yMax = ScreenToGUISpace(Camera.main.WorldToScreenPoint(PhysicsToMapWorldUnflipped(boxBounds.LowerLeft))).y;

                GUI.Box(rect, GUIContent.none);

                ReturnBounds(boxBounds);
            }
        }

        public void RemoveEntity(EngineEntity entity)
        {
            Entities.Remove(entity);
        }

        public bool IsInHitboxRangeCells(EngineEntity entity1, EngineEntity entity2, int gridCellRange)
        {
            return IsInHitboxRange(entity1, entity2, gridCellRange * UnitsPerCell);
        }

        public int SquaredDistance(Vector2Int first, Vector2Int second) => ISquare(first.x - second.x) + ISquare(first.y - second.y);

        public int Distance(Vector2Int first, Vector2Int second) => ISqrt(SquaredDistance(first, second));

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
        public bool IsInCircularRange(EngineEntity sourceEntity, EngineEntity targetEntity, int radius, bool debug = false)
        {
            // Here we are using the simple Euclidean Distance

            var sourceEntityBound = GetPooledEntityBounds(sourceEntity);
            var targetEntityBound = GetPooledEntityBounds(targetEntity);

            var p1 = new Vector2Int(sourceEntityBound.Center.x, sourceEntityBound.Center.y);
            var p2 = new Vector2Int(targetEntityBound.Center.x, targetEntityBound.Center.y);

            var distance = ISqrt(SquaredDistance(p1, p2));

            if (Debug.isDebugBuild && debug)
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
        public bool IsInTriangularRange(EngineEntity sourceEntity, EngineEntity targetEntity, int width, int height, int unitsLeniency, bool debug = false)
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
            if (Debug.isDebugBuild && debug)
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
        public bool IsInConicRange(EngineEntity sourceEntity, EngineEntity targetEntity, int width, int height, int unitsLeniency, bool debug = false)
        {
            var isInTriangularRange = IsInTriangularRange(sourceEntity, targetEntity, width, height, unitsLeniency, debug);
            var isInCircularRange = IsInCircularRange(sourceEntity, targetEntity, height - unitsLeniency, debug);

            return isInTriangularRange && isInCircularRange;
        }

        // converts a world point to a physics engine point using linear interpolation 
        // TODO: this is only used on BoxCollider2D to let people use the collider tool to define boundaries
        // if this turns up to create problems we have to define int boundaries in EngineEntity
        public Vector2Int WorldToPhysics(Vector2 world)
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


        // converts a world point to a physics engine point using linear interpolation 
        // TODO: this is only used on BoxCollider2D to let people use the collider tool to define boundaries
        // if this turns up to create problems we have to define int boundaries in EngineEntity
        public Vector2Int PooledWorldToPhysics(Vector2 world)
        {
            var xWorldBounds = Map.cellWidth * Map.mapSizeX;
            var yWorldBounds = Map.cellHeight * Map.mapSizeY;

            var xPhysicsBounds = UnitsPerCell * Map.mapSizeX;
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            var vector = PoolInstances.Vector2IntPool.GetObject();

            vector.x = Mathf.RoundToInt((xPhysicsBounds * world.x) / xWorldBounds);
            vector.y = Mathf.RoundToInt((yPhysicsBounds * world.y) / yWorldBounds);

            return vector;
        }

        Vector2 ScreenToGUISpace(Vector2 screen)
        {
            return new Vector2(screen.x, Screen.height - screen.y);
        }

        Rect ScreenToGUIRect(Rect rect)
        {
            return new Rect(ScreenToGUISpace(rect.position), rect.size);
        }

        /// <summary>Returns the world position, already adjusted for the map and the team (flipped/non-flipped)</summary>
        public Vector2 PhysicsToMapWorld(Vector2Int physics) =>
            (TeamComponent.assignedTeam == TeamComponent.topTeam) ? PhysicsToMapWorldFlipped(physics) : PhysicsToMapWorldUnflipped(physics);


        /// <summary>Returns the world position, adjusted for the map</summary>
        Vector2 PhysicsToMapWorldUnflipped(Vector2Int physics)
        {
            return PhysicsToWorld(physics) + (Vector2)Map.transform.position;
        }


        /// <summary>Returns the world position, adjusted for the map and flipped (used for e.g. top team position rendering)</summary>
        Vector2 PhysicsToMapWorldFlipped(Vector2Int physics)
        {
            var yPhysicsBounds = UnitsPerCell * Map.mapSizeY;

            var flippedPhysics = physics;

            flippedPhysics.y = yPhysicsBounds - flippedPhysics.y;

            return PhysicsToMapWorldUnflipped(flippedPhysics);
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


        public Vector2Int PooledGridCellToPhysics(GridCell cell)
        {
            var physics = PoolInstances.Vector2IntPool.GetObject();

            physics.x = Mathf.RoundToInt(cell.vector.x * UnitsPerCell);
            physics.y = Mathf.RoundToInt(cell.vector.y * UnitsPerCell);

            return physics;
        }

        void SetPhysicsToGridCell(GridCell cell, Vector2Int physics)
        {
            cell.vector.x = physics.x / UnitsPerCell;
            cell.vector.y = physics.y / UnitsPerCell;
        }

        public GridCell PooledPhysicsToGridCell(Vector2Int physics)
        {
            var cell = PoolInstances.GridCellPool.GetObject();

            SetPhysicsToGridCell(cell, physics);

            return cell;
        }


        public GridCell PhysicsToGridCell(Vector2Int physics)
        {
            var cell = new GridCell(0, 0);

            SetPhysicsToGridCell(cell, physics);

            return cell;
        }

        public List<EngineEntity> FindInGridCell(GridCell gridCell, bool countStructures)
        { // TODO: Maybe use a quad tree for < O(n) search
            var physics = GridCellToPhysics(gridCell);

            List<EngineEntity> targetEntities = new List<EngineEntity> { };

            foreach (var entity in Entities)
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

        /// <summary>
        /// Finds the closest unit to the origin that satisfies the predicate
        /// </summary>
        public EngineEntity FindClosest(Vector2Int origin, Func<EngineEntity, bool> predicate)
        {
            EngineEntity closestEntity = null;
            int? closestDistance = null;

            foreach (var entity in Entities)
            {
                var distance = Distance(origin, entity.Position);

                if ((closestDistance == null || distance < closestDistance) && predicate(entity))
                {
                    closestDistance = distance;
                    closestEntity = entity;
                }
            }

            return closestEntity;
        }

        public List<EngineEntity> FindInHitboxRange(EngineEntity origin, int range, bool countStructures)
        {
            List<EngineEntity> targetEntities = new List<EngineEntity> { };

            foreach (var entity in Entities)
            {
                var isNotTargeted = (entity.IsStructure && !countStructures) || entity.IsMapObstacle || !entity.IsRigid;

                if (isNotTargeted) continue;

                if (IsInHitboxRange(origin, entity, range))
                {
                    targetEntities.Add(entity);
                }
            }

            return targetEntities;
        }

        public List<EngineEntity> FindInRadius(Vector2Int origin, int engineUnitsRadius, bool countStructures)
        {
            List<EngineEntity> targetEntities = new List<EngineEntity> { };

            foreach (var entity in Entities)
            {
                var isNotTargeted = (entity.IsStructure && !countStructures) || entity.IsMapObstacle;

                if (isNotTargeted) continue;

                var distance = Distance(origin, entity.Position);

                if (distance <= engineUnitsRadius)
                {
                    targetEntities.Add(entity);
                }
            }

            return targetEntities;
        }

        public void ReturnBounds(BoxBounds bounds)
        {
            PoolInstances.BoxBoundsPool.ReturnObject(bounds);
        }

        void SetEntityBounds(EngineEntity entity, BoxBounds bounds)
        {
            var worldBounds = entity.Bounds;

            var physicsExtents = PooledWorldToPhysics(worldBounds.size);

            var physicsUpperLeftBounds = entity.Position;

            physicsUpperLeftBounds.x -= Mathf.FloorToInt(physicsExtents.x / 2);
            physicsUpperLeftBounds.y += Mathf.FloorToInt(physicsExtents.y / 2);

            var physicsUpperRightBounds = entity.Position;

            physicsUpperRightBounds.x += Mathf.FloorToInt(physicsExtents.x / 2);
            physicsUpperRightBounds.y += Mathf.FloorToInt(physicsExtents.y / 2);

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

            PoolInstances.Vector2Pool.ReturnObject(physicsExtents);
        }

        public BoxBounds GetEntityBounds(EngineEntity entity)
        {
            var bounds = new BoxBounds();

            SetEntityBounds(entity, bounds);

            return bounds;
        }

        public BoxBounds GetPooledEntityBounds(EngineEntity entity)
        {
            var bounds = PoolInstances.BoxBoundsPool.GetObject();

            SetEntityBounds(entity, bounds);

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
                ((layer1 == Constants.RIVER_BOUNDS_LAYER || layer2 == Constants.RIVER_BOUNDS_LAYER) && (layer1 == Constants.SWIMMING_LAYER || layer2 == Constants.SWIMMING_LAYER)) ||
                (layer1 == Constants.PROJECTILES_LAYER || layer2 == Constants.PROJECTILES_LAYER) ||
                (layer1 == Constants.WATER_LAYER && (layer2 != Constants.SWIMMING_LAYER && layer2 != Constants.FLYING_LAYER)) ||
                (layer2 == Constants.WATER_LAYER && (layer1 != Constants.SWIMMING_LAYER && layer1 != Constants.FLYING_LAYER));
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

        /// <summary>
        /// Calculate the power of a Decimal
        /// </summary>
        /// <param name="n">The base</param>
        /// <param name="y">The exponent</param>
        /// <returns>A Decimal with the power result</returns>
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