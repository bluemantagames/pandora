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
    public class MovementComponent : MonoBehaviour
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
        bool isTargetForced = false;
        public MovementStateEnum LastState;

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

            var enemy = map.GetEnemyInRange(gameObject, currentPosition, team.team);

            // first and foremost, if an enemy is in range: attack them
            if (enemy != null && targetEnemy == null)
            {
                targetEnemy = enemy;

                currentPath = null;

                return new MovementState(enemy, MovementStateEnum.TargetAcquired);
            }

            // remove targeted enemy if they are dead and recalculate pathing
            if (targetEnemy != null && targetEnemy.enemy.GetComponent<LifeComponent>().isDead)
            {
                Debug.Log("Target down");

                targetEnemy = null;

                currentPath = null;
            }

            // if you're attacking an enemy: keep attacking
            if (targetEnemy != null && combatBehaviour.IsInAttackRange(targetEnemy))
            {
                engineEntity.SetEmptyPath();

                return new MovementState(enemy, MovementStateEnum.EnemyApproached);
            }

            // if you were attacking an enemy, but they are now out of attack range, forget them
            if (targetEnemy != null && !combatBehaviour.IsInAttackRange(targetEnemy) && LastState == MovementStateEnum.EnemyApproached)
            {
                currentPath = null;

                if (!isTargetForced) {
                    targetEnemy = null;
                }
            }

            // if no path has been calculated: calculate one and point the object to the first position in the queue
            if (currentPath == null || currentPath.Contains(currentPosition))
            {
                AdvancePosition(currentPosition);

                Debug.Log($"Found path ({gameObject.name}): {string.Join(",", currentPath)}, am in {currentPosition}");
            }

            var worldPosition =
                (TeamComponent.assignedTeam == TeamComponent.bottomTeam) ?
                    engineEntity.GetWorldPosition() :
                    engineEntity.GetFlippedWorldPosition();

            transform.position = worldPosition;

            return new MovementState(null, MovementStateEnum.Moving);
        }

        /**
         * This sets the new grid cell target for the pathing and removes the current from the queue, effectively
         * "advancing" pathing forward
         */
        private void AdvancePosition(GridCell currentPosition)
        {
            CalculatePath();

            if (currentPath.Count < 1) {
                Debug.LogWarning($"Empty path for {targetEnemy?.enemyCell ?? map.GetTarget(gameObject, currentPosition, team.team)}");
            }

            currentTarget = currentPath.First();

            engineEntity.SetTarget(currentTarget);

            direction = (currentTarget.vector - currentPosition.vector).normalized;
        }


        /**
         * Very simple and probably shitty and not at all optimized A* implementation
         */
        List<GridCell> FindPath(GridCell end)
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

            if (currentPosition == end)
            {
                return evaluatingPosition.points;
            }

            // get the last item in the queue
            while ((item = evaluatingPosition.points.Last()) != end && !pathFound)
            {

                if (DebugPathfinding)
                {
                    Debug.Log($"Positions {string.Join(", ", evaluatingPosition.points)} - searching for {end}");
                }

                // check all surrounding positions
                for (var x = -1f; x <= 1f; x++)
                {
                    for (var y = -1f; y <= 1f; y++)
                    {
                        var advance = new GridCell(item.vector.x + x, item.vector.y + y);

                        var isAdvanceRedundant = evaluatingPosition.pointsSet.Contains(advance);

                        if (advance == end) { // Stop the loop if we found the path
                            Debug.Log("Found path!");
                        }

                        if (advance != item && !map.IsObstacle(advance) && !isAdvanceRedundant) // except the current positions, obstacles or going back
                        {
                            var distanceToEnd = Vector2.Distance(advance.vector, end.vector); // use the distance between this point and the end as h(n)
                            var distanceFromStart = evaluatingPosition.points.Count + 1; // use the distance between this point and the start as g(n)
                            var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                            var currentPositions = new List<GridCell>(evaluatingPosition.points) { advance };
                            var queueItem = new QueueItem(currentPositions, new HashSet<GridCell>(currentPositions));

                            if (advance == end) { // Stop the loop if we found the path
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
                        Debug.Log("Pausing the editor");

                        Debug.Break();
                    }

                    return evaluatingPosition.points;
                }

                if (!pathFound) evaluatingPosition = priorityQueue.Dequeue();
            }

            if (DebugPathfinding)
            {
                Debug.Log($"Done, positions {string.Join(", ", evaluatingPosition.points)}");
            }

            return evaluatingPosition.points;
        }

        private void CalculatePath()
        {
            var currentPosition = CurrentCellPosition();

            currentPath = FindPath(targetEnemy?.enemyCell ?? map.GetTarget(gameObject, currentPosition, team.team)).Skip(1).ToList();
        }

        private GridCell CurrentCellPosition()
        {
            return engineEntity.GetCurrentCell();
        }
    }
}