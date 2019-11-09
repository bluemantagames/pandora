﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Priority_Queue;
using Pandora;
using Pandora.Combat;
using UnityEngine.Profiling;
using Pandora.Engine;
using Pandora.Pool;
using System.Linq;

namespace Pandora.Movement
{
    public class MovementComponent : MonoBehaviour, CollisionCallback, MovementBehaviour
    {
        Rigidbody2D body;
        GridCell currentTarget;
        Vector2 worldCurrentTarget;
        Vector2 direction;
        List<GridCell> currentPath;
        TeamComponent team;
        Enemy targetEnemy;
        CombatBehaviour combatBehaviour;
        uint? collisionTotalElapsed = null;
        /// <summary>Enables log on the A* implementation</summary>
        public bool DebugPathfinding;
        bool isTargetForced = false;
        bool evadeUnits = false;
        Vector2Int? lastCollisionPosition;
        public MovementStateEnum LastState { get; set; }
        Enemy lastEnemyTargeted;

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


        /// <summary>This is the value set in the unity editor, backing the interface one</summary>
        public int MovementSpeed = 400;

        public int Speed {
            get => MovementSpeed;
            set => MovementSpeed = value;
        }

        public MapComponent map { get; set; }

        // Start is called before the first frame update
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            team = GetComponent<TeamComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
        }

        /// <summary>
        /// Reset the current path thus forcing
        /// recalculation of the pathing
        /// </summary>
        public void ResetPath() {
            currentPath = null;
        }

        public MovementState Move()
        {
            var currentPosition = CurrentCellPosition();

            lastEnemyTargeted = map.GetEnemy(gameObject, currentPosition, team);

            var isTargetDead = targetEnemy?.enemy.GetComponent<LifeComponent>().IsDead ?? true;

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
                    targetEnemy = lastEnemyTargeted;
                }
            }

            // Otherwise, pick a target
            if (lastEnemyTargeted.enemy != targetEnemy?.enemy && (!combatBehaviour.isAttacking || isTargetDead) && !isTargetForced)
            {
                targetEnemy = lastEnemyTargeted;

                currentPath = null;

                return new MovementState(lastEnemyTargeted, MovementStateEnum.TargetAcquired);
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
                Debug.LogWarning($"Empty path for {targetEnemy.enemyCell} from {currentPosition}");

                return;
            }

            currentTarget = currentPath.First();

            engineEntity.SetTarget(currentTarget);

            direction = (currentTarget.vector - currentPosition.vector).normalized;
        }

        List<GridCell> VectorsToGridCells(IEnumerable<Vector2> vectors) =>
            (from vector in vectors
            select new GridCell(vector)).ToList();

        /**
         * Simple A* implementation. We try to use as many pools
         * as humanly possible in order to not allocate too much (it costs a lot of time)
         * 
         * If evadeUnits is true, it also counts units-occupied gridcells as obstacles
         */
        List<GridCell> FindPath(GridCell end)
        {
            var priorityQueue = new SimplePriorityQueue<QueueItem>();
            var endVector = end.vector;

            priorityQueue.Clear();

            var currentPosition = engineEntity.GetCurrentCell().vector;

            int pass = 0;

            var evaluatingPosition =
                new QueueItem(
                    new List<Vector2> { currentPosition },
                    new HashSet<Vector2>()
                );

            Vector2 item;

            var pathFound = false;

            if (map.IsObstacle(end, IsFlying, team))
            {
                Debug.LogWarning($"Cannot find path towards an obstacle ({end})");

                return VectorsToGridCells(evaluatingPosition.points);
            }

            if (currentPosition == endVector)
            {
                return VectorsToGridCells(evaluatingPosition.points);
            }

            // get the last item in the queue
            while ((item = evaluatingPosition.points.Last()) != endVector && !pathFound)
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
                        var advance = PoolInstances.Vector2Pool.GetObject();

                        advance.x = item.x + x;
                        advance.y = item.y + y;

                        var advanceGridCell = PoolInstances.GridCellPool.GetObject();

                        advanceGridCell.vector.x = advance.x;
                        advanceGridCell.vector.x = advance.x;

                        var isAdvanceRedundant = evaluatingPosition.pointsSet.Contains(advance);

                        var containsUnit = false;

                        if (evadeUnits && advance != endVector) // check the advance for units unless it's the end position
                        {
                            var units = engine.FindInGridCell(advanceGridCell, false);

                            containsUnit = units.Exists(entity => entity.GameObject.GetComponent<TeamComponent>()?.team == team.team);
                        }

                        if (advance != item && !map.IsObstacle(advanceGridCell, IsFlying, team) && !isAdvanceRedundant && !containsUnit) // except the current positions, obstacles or going back
                        {
                            var distanceToEnd = Vector2.Distance(advance, end.vector); // use the distance between this point and the end as h(n)
                            var distanceFromStart = evaluatingPosition.points.Count + 1; // use the distance between this point and the start as g(n)
                            var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                            var currentPositions = new List<Vector2>(evaluatingPosition.points) { advance };
                            var queueItem = PoolInstances.QueueItemPool.GetObject();

                            queueItem.points = currentPositions;
                            queueItem.pointsSet = PoolInstances.VectorHashSetPool.GetObject();

                            foreach (var position in currentPositions)
                            {
                                queueItem.pointsSet.Add(position);
                            }

                            if (advance == endVector)
                            { // Stop the loop if we found the path
                                evaluatingPosition = queueItem;
                                pathFound = true;

                                break;
                            }

                            PoolInstances.VectorHashSetPool.ReturnObject(evaluatingPosition.pointsSet);
                            PoolInstances.QueueItemPool.ReturnObject(evaluatingPosition);

                            priorityQueue.Enqueue(
                                queueItem,
                                priority
                            );
                        }
                        else
                        {
                            PoolInstances.Vector2Pool.ReturnObject(advance);
                            PoolInstances.GridCellPool.ReturnObject(advanceGridCell);
                        }
                    }
                }

                pass += 1;

                if (pass > 5000)
                {
                    Debug.LogWarning($"Short circuiting after 5000 passes started from {currentPosition} to {end} - {team.team} {gameObject.name} {evadeUnits}");
                    Debug.LogWarning("Best paths found are");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");

                    if (DebugPathfinding)
                    {
                        Debug.Log("DebugPathfinding: Pausing the editor");

                        Debug.Break();
                    }

                    if (evadeUnits)
                    {
                        evadeUnits = false;

                        return FindPath(end);
                    }
                    else
                    {
                        return VectorsToGridCells(evaluatingPosition.points);
                    }
                }

                if (!pathFound) evaluatingPosition = priorityQueue.Dequeue();
            }

            evadeUnits = false;

            if (DebugPathfinding)
            {
                Debug.Log($"DebugPathfinding: Done, positions {string.Join(", ", evaluatingPosition.points)}");
            }

            return VectorsToGridCells(evaluatingPosition.points);
        }

        private void CalculatePath()
        {
            engineEntity.SetSpeed(Speed);

            var currentPosition = CurrentCellPosition();
            var target = targetEnemy.enemyCell;

            if (DebugPathfinding)
            {
                Debug.Log($"DebugPathfinding: Current target is {target} ({targetEnemy})");
            }

            var wereUnitEvaded = evadeUnits;

            currentPath = FindPath(target).Skip(1).ToList();

            if (wereUnitEvaded)
            {
                Debug.Log($"Evaded units with {string.Join(",", currentPath)}");
            }
        }

        private GridCell CurrentCellPosition()
        {
            return engineEntity.GetCurrentCell();
        }

        public void Collided(EngineEntity entity, uint totalElapsed)
        {
            var shouldNotCheckEvading =
                gameObject.layer == Constants.FLYING_LAYER ||
                !entity.IsRigid ||
                evadeUnits ||
                lastEnemyTargeted.enemyCell.vector.y == map.bottomMapSizeY ||
                GetComponent<CombatBehaviour>().isAttacking;

            if (shouldNotCheckEvading) return;

            if (!collisionTotalElapsed.HasValue)
            {
                collisionTotalElapsed = totalElapsed;
            }

            lastCollisionPosition = engineEntity.Position;

            if (lastCollisionPosition == engineEntity.Position && totalElapsed - (collisionTotalElapsed ?? 0) >= 20)
            {
                Debug.Log("Finally evading");

                evadeUnits = true;
                currentPath = null;

                lastCollisionPosition = null;
                collisionTotalElapsed = null;
            }

        }
    }
}