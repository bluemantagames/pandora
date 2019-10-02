using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Priority_Queue;
using Pandora;
using Pandora.Combat;
using UnityEngine.Profiling;
using Pandora.Engine;

namespace Pandora.Movement
{
    public class MovementComponent : MonoBehaviour, CollisionCallback
    {
        Rigidbody2D body;
        GridCell currentTarget;
        Vector2 worldCurrentTarget;
        Vector2 direction;
        List<GridCell> currentPath;
        TeamComponent team;
        Enemy targetEnemy;
        CombatBehaviour combatBehaviour;
        /// <summary>Enables log on the A* implementation</summary>
        public bool DebugPathfinding;
        bool isTargetForced = false, evadeUnits = false;
        Vector2Int? lastCollisionPosition;
        public MovementStateEnum LastState;

        public bool IsFlying
        {
            get
            {
                return gameObject.layer == Constants.FLYING_LAYER;
            }
        }

        public Enemy Target
        {
            private get
            {
                return targetEnemy;
            }

            set
            {
                targetEnemy = value;
                isTargetForced = true;
                currentPath = null;
            }
        }

        PandoraEngine engine
        {
            get
            {
                return GetComponent<EngineComponent>().Engine;
            }
        }

        EngineEntity engineEntity
        {
            get
            {
                return GetComponent<EngineComponent>().Entity;
            }
        }

        public float speed = 1f;
        public MapComponent map;

        // Start is called before the first frame update
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            team = GetComponent<TeamComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
        }

        public MovementState Move()
        {
            var currentPosition = CurrentCellPosition();

            var enemy = map.GetEnemy(gameObject, currentPosition, team);

            var isTargetDead = targetEnemy?.enemy.GetComponent<LifeComponent>().isDead ?? true;

            transform.position =
                (TeamComponent.assignedTeam == TeamComponent.bottomTeam) ?
                    engineEntity.GetWorldPosition() :
                    engineEntity.GetFlippedWorldPosition();


            // if you're attacking an enemy: keep attacking
            if (targetEnemy != null && combatBehaviour.IsInAttackRange(targetEnemy) && !isTargetDead)
            {
                engineEntity.SetEmptyPath();

                return new MovementState(targetEnemy, MovementStateEnum.EnemyApproached);
            }

            // if you were attacking an enemy, but they are now out of attack range, forget them
            if (targetEnemy != null && !combatBehaviour.IsInAttackRange(targetEnemy) && LastState == MovementStateEnum.EnemyApproached)
            {
                currentPath = null;

                if (!isTargetForced)
                {
                    targetEnemy = null;
                }
            }

            // Otherwise, pick a target
            if (enemy.enemy != targetEnemy?.enemy && (!combatBehaviour.isAttacking || isTargetDead) && !isTargetForced)
            {
                targetEnemy = enemy;

                currentPath = null;

                return new MovementState(enemy, MovementStateEnum.TargetAcquired);
            }

            // if no path has been calculated: calculate one and point the object to the first position in the queue
            if (currentPath == null || currentPath.Contains(currentPosition))
            {
                AdvancePosition(currentPosition);

                Debug.Log($"Found path ({gameObject.name}): {string.Join(",", currentPath)}, am in {currentPosition}");
            }

            return new MovementState(null, MovementStateEnum.Moving);
        }

        /**
         * This sets the new grid cell target for the pathing and removes the current from the queue, effectively
         * "advancing" pathing forward
         */
        private void AdvancePosition(GridCell currentPosition)
        {
            CalculatePath();

            if (currentPath.Count < 1)
            {
                Debug.LogWarning($"Empty path for {targetEnemy.enemyCell}");
            }

            currentTarget = currentPath.First();

            engineEntity.SetTarget(currentTarget);

            direction = (currentTarget.vector - currentPosition.vector).normalized;
        }


        /**
         * Very simple and probably shitty and not at all optimized A* implementation.
         * 
         * If evadeUnits is true, it also counts units-occupied gridcells as obstacles
         */
        List<GridCell> FindPath(GridCell end, bool evadeUnits)
        {
            var priorityQueue = new SimplePriorityQueue<QueueItem>();
            //Debug.Log($"Searching path for {end}");

            priorityQueue.Clear();

            var currentPosition = engineEntity.GetCurrentCell();

            int pass = 0;

            var evaluatingPosition =
                new QueueItem(
                    new List<GridCell> { currentPosition },
                    new HashSet<GridCell>()
                );

            GridCell item;

            var pathFound = false;

            if (map.IsObstacle(end, IsFlying, team))
            {
                Debug.LogWarning("Cannot find path towards an obstacle");

                return evaluatingPosition.points;
            }

            if (currentPosition == end)
            {
                return evaluatingPosition.points;
            }

            // get the last item in the queue
            while ((item = evaluatingPosition.points.Last()) != end && !pathFound)
            {

                if (DebugPathfinding)
                {
                    Debug.Log($"DebugPathfinding: Positions {string.Join(", ", evaluatingPosition.points)} - searching for {end}");
                }

                // check all surrounding positions
                for (var x = -1f; x <= 1f; x++)
                {
                    for (var y = -1f; y <= 1f; y++)
                    {
                        var advance = new GridCell(item.vector.x + x, item.vector.y + y);

                        var isAdvanceRedundant = evaluatingPosition.pointsSet.Contains(advance);

                        var containsUnit = (evadeUnits) ? engine.FindInGridCell(advance).Count > 0 : false;

                        if (advance != item && !map.IsObstacle(advance, IsFlying, team) && !isAdvanceRedundant && !containsUnit) // except the current positions, obstacles or going back
                        {
                            var distanceToEnd = Vector2.Distance(advance.vector, end.vector); // use the distance between this point and the end as h(n)
                            var distanceFromStart = evaluatingPosition.points.Count + 1; // use the distance between this point and the start as g(n)
                            var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                            var currentPositions = new List<GridCell>(evaluatingPosition.points) { advance };
                            var queueItem = new QueueItem(currentPositions, new HashSet<GridCell>(currentPositions));

                            if (advance == end)
                            { // Stop the loop if we found the path
                                evaluatingPosition = queueItem;
                                pathFound = true;

                                break;
                            }

                            priorityQueue.Enqueue(
                                new QueueItem(currentPositions, new HashSet<GridCell>(currentPositions)),
                                priority
                            );
                        }
                    }
                }

                pass += 1;

                if (pass > 5000)
                {
                    Debug.Log($"Short circuiting after 5000 passes started from {currentPosition} to {end}");
                    Debug.Log("Best paths found are");
                    Debug.Log($"{priorityQueue.Dequeue()}");
                    Debug.Log($"{priorityQueue.Dequeue()}");
                    Debug.Log($"{priorityQueue.Dequeue()}");

                    if (DebugPathfinding)
                    {
                        Debug.Log("DebugPathfinding: Pausing the editor");

                        Debug.Break();
                    }

                    return evaluatingPosition.points;
                }

                if (!pathFound) evaluatingPosition = priorityQueue.Dequeue();
            }

            if (DebugPathfinding)
            {
                Debug.Log($"DebugPathfinding: Done, positions {string.Join(", ", evaluatingPosition.points)}");
            }

            return evaluatingPosition.points;
        }

        private void CalculatePath()
        {
            engineEntity.SetSpeed(speed);

            var currentPosition = CurrentCellPosition();
            var target = targetEnemy.enemyCell;

            if (DebugPathfinding)
            {
                Debug.Log($"DebugPathfinding: Current target is {target} ({targetEnemy})");
            }

            currentPath = FindPath(target, evadeUnits).Skip(1).ToList();

            evadeUnits = false; // try not evading next time
        }

        private GridCell CurrentCellPosition()
        {
            return engineEntity.GetCurrentCell();
        }

        public void Collided(EngineEntity entity)
        {
            // Disabling this for flying units - harpies do this way too much making the game lag
            if (gameObject.layer == Constants.FLYING_LAYER) return;

            Debug.Log("Collided, checking if Evading");

            if (GetComponent<CombatBehaviour>().isAttacking) return;

            if (lastCollisionPosition == null)
            {
                lastCollisionPosition = engineEntity.Position;

                return;
            }

            if (lastCollisionPosition == engineEntity.Position)
            {
                Debug.Log("Evading");

                evadeUnits = true;
            }
            else
            {
                lastCollisionPosition = null;
            }
        }
    }
}