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
using Pandora.Pool;

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
        bool isEvading;
        CombatBehaviour combatBehaviour;
        uint? collisionTotalElapsed = null;
        /// <summary>Enables log on the A* implementation</summary>
        public bool DebugPathfinding;
        bool isTargetForced = false;
        Vector2Int? lastCollisionPosition;
        public MovementStateEnum LastState { get; set; }
        Enemy lastEnemyTargeted;

        Astar<Vector2Int> astar = new Astar<Vector2Int>(
            PoolInstances.Vector2IntHashSetPool,
            PoolInstances.Vector2IntQueueItemPool,
            PoolInstances.Vector2IntPool,
            PoolInstances.Vector2IntListPool
        );

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

        public int Speed
        {
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
        public void ResetPath()
        {
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

            // if you were attacking an enemy, but they are now out of attack range: forget them
            if (targetEnemy != null && !combatBehaviour.IsInAttackRange(targetEnemy) && LastState == MovementStateEnum.EnemyApproached)
            {
                currentPath = null;

                if (!isTargetForced)
                {
                    targetEnemy = lastEnemyTargeted;
                }
            }

            // Otherwise: pick a target
            if (lastEnemyTargeted.enemy != targetEnemy?.enemy && (!combatBehaviour.isAttacking || isTargetDead) && !isTargetForced)
            {
                targetEnemy = lastEnemyTargeted;

                currentPath = null;

                return new MovementState(lastEnemyTargeted, MovementStateEnum.TargetAcquired);
            }

            // if no path has been calculated: calculate one and point the object to the first position in the queue
            if (currentPath == null || currentPath.Contains(currentPosition) || engineEntity.Path == null)
            {
                AdvancePosition(currentPosition);
            }

            return new MovementState(null, MovementStateEnum.Moving);
        }

        /**
         * This sets the new grid cell target for the pathing and removes the current from the queue, effectively
         * "advancing" pathing forward
         */
        private void AdvancePosition(GridCell currentPosition)
        {
            engineEntity.IsEvading = isEvading;

            isEvading = false;

            if (!engineEntity.IsEvading)
            {
                CalculatePath();
            } else {
                currentPath = new List<GridCell> {}; // Leave pathfinding to the engine when evading
            }

            if (currentPath.Count < 1 && !engineEntity.IsEvading)
            {
                Debug.LogWarning($"Empty path for {targetEnemy.enemyCell} from {currentPosition}, trying to evade");

                isEvading = true;

                AdvancePosition(currentPosition);

                return;
            }

            if (engineEntity.IsEvading)
            {
                currentTarget = targetEnemy.enemyCell;
            }
            else
            {
                currentTarget = currentPath.First();
            }

            engineEntity.SetTarget(currentTarget);

            direction =  ((Vector2) currentTarget.vector - currentPosition.vector).normalized;
        }

        List<GridCell> VectorsToGridCells(IEnumerable<Vector2Int> vectors) =>
            (from vector in vectors
             select new GridCell(vector)).ToList();

        List<GridCell> FindPath(GridCell end)
        {
            return VectorsToGridCells(
                astar.FindPath(
                    engineEntity.GetCurrentCell().vector,
                    end.vector,
                    position =>
                    {
                        var gridCell = PoolInstances.GridCellPool.GetObject();

                        gridCell.vector = position;

                        var isObstacle = map.IsObstacle(gridCell, IsFlying, team);

                        PoolInstances.GridCellPool.ReturnObject(gridCell);

                        return isObstacle;
                    },
                    position =>
                    {
                        var surroundingPositions = PoolInstances.Vector2IntListPool.GetObject();

                        for (var x = -1; x <= 1; x++)
                        {
                            for (var y = -1; y <= 1; y++)
                            {
                                var advance = PoolInstances.Vector2IntPool.GetObject();

                                advance.x = position.x + x;
                                advance.y = position.y + y;

                                surroundingPositions.Add(advance);
                            }
                        }

                        return surroundingPositions;
                    },
                    (a, b) => Vector2.Distance(a, b)
                )
            );
        }

        private void CalculatePath()
        {
            Profiler.BeginSample("MovementComponent pathfinding");
            engineEntity.SetSpeed(Speed);

            var currentPosition = CurrentCellPosition();
            var target = targetEnemy.enemyCell;

            if (DebugPathfinding)
            {
                Debug.Log($"DebugPathfinding: Current target is {target} ({targetEnemy})");
            }

            currentPath = FindPath(target).Skip(1).ToList();
            Profiler.EndSample();
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
                engineEntity.IsEvading ||
                lastEnemyTargeted?.enemyCell.vector.y == map.bottomMapSizeY ||
                GetComponent<CombatBehaviour>().isAttacking;

            if (shouldNotCheckEvading) return;

            if (!collisionTotalElapsed.HasValue)
            {
                collisionTotalElapsed = totalElapsed;
            }

            if (lastCollisionPosition == engineEntity.Position && totalElapsed - (collisionTotalElapsed ?? 0) >= 50)
            {
                Debug.Log("Finally evading");

                // we don't set engineEntity.IsEvading directly because
                // the collision system might use it
                isEvading = true;

                engineEntity.EvadedUnit = entity;

                currentPath = null;

                lastCollisionPosition = null;
                collisionTotalElapsed = null;
            }
            else
            {
                lastCollisionPosition = engineEntity.Position;
            }

        }
    }
}