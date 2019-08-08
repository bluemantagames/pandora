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
        public float aggroRange = 10;
        public PandoraEngine engine;
        public EngineEntity engineEntity;

        public float speed = 1f;
        public MapComponent map;
        float unitsPerSecond = 0f;
        float seconds = 0f;
        private SimplePriorityQueue<QueueItem> priorityQueue;

        // Start is called before the first frame update
        void Awake()
        {
            priorityQueue = new SimplePriorityQueue<QueueItem>();
            body = GetComponent<Rigidbody2D>();
            team = GetComponent<TeamComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
        }

        public MovementState Move()
        {
            Vector2 position = transform.position;
            var currentPosition = CurrentCellPosition();

            var enemy = map.GetNearestEnemy(gameObject, currentPosition, team.team, aggroRange);

            Debug.Log($"Enemy {enemy}");

            // first and foremost, if an enemy is in range: attack them
            if (enemy != null && targetEnemy == null)
            {
                targetEnemy = enemy;

                currentPath = null;

                return new MovementState(enemy, MovementStateEnum.MovingTowardsEnemy);
            }

            // remove targeted enemy if they are dead and recalculate pathing
            if (targetEnemy != null && targetEnemy.enemy.GetComponent<LifeComponent>().isDead) {
                Debug.Log("Target down");

                targetEnemy = null;

                currentPath = null;
            }

            // if you're attacking an enemy: keep attacking
            if (targetEnemy != null && combatBehaviour.IsInRange(currentPosition, targetEnemy.enemyCell)) {
                return new MovementState(enemy, MovementStateEnum.EnemyApproached);
            }

            // if no path has been calculated: calculate one and point the object to the first position in the queue
            if (currentPath == null)
            {
                currentPath = FindPath(map.GetTarget(gameObject, currentPosition, team.team, aggroRange));

                Debug.Log("Found path " + string.Join(",", currentPath));

                AdvancePosition(currentPosition);
            }

            // if current position is in the queue it means we need to advance target
            if (currentPath.Contains(currentPosition))
            {
                AdvancePosition(currentPosition);
            }

            seconds += Time.deltaTime;
            unitsPerSecond += Time.deltaTime * speed;

            if (seconds >= 1) {
                Debug.Log($"Units per second: {unitsPerSecond}");

                seconds = 0;
                unitsPerSecond = 0;
            }

            position += direction * (Time.fixedDeltaTime * speed);

            var worldPosition = engine.PhysicsToWorld(engineEntity.Position);
            var physicsDirection = engine.GridCellToPhysics(new GridCell(direction));

            engineEntity.Direction = physicsDirection;

            engine.Process(Mathf.RoundToInt(Time.deltaTime * 1000));
            //engine.NextTick();

            Debug.Log($"World position calculated from engine {worldPosition}");
            Debug.Log($"Direction calculated from engine {physicsDirection}");
            Debug.Log($"Position calculated from engine {engineEntity.Position}");
            Debug.Log($"Speed calculated from engine {engineEntity.Speed}");

            body.MovePosition(worldPosition);

            return new MovementState(null, MovementStateEnum.Moving);
        }

        /**
         * This sets the new grid cell target for the pathing and removes the current from the queue, effectively
         * "advancing" pathing forward
         */
        private void AdvancePosition(GridCell currentPosition)
        {
            if (currentPath.Count() > 1) // if path still has elements after we remove the current target
            {
                Debug.Log("Advancing from " + currentPosition);

                currentPath.Remove(currentPosition);

                currentTarget = currentPath.First();
                direction = (currentTarget.vector - currentPosition.vector).normalized;
            }
            else
            {
                direction = Vector2.zero;
                currentPath = null;
            }
        }


        /**
         * Very simple and probably shitty and not at all optimized A* implementation
         */
        List<GridCell> FindPath(GridCell end)
        {
            Profiler.BeginSample("FindPath");

            Debug.Log($"Searching path for {end}");

            priorityQueue.Clear();

            var currentPosition = map.WorldPositionToGridCell(transform.position);

            int pass = 0;

            var evaluatingPosition =
                new QueueItem(
                    new List<GridCell> { map.WorldPositionToGridCell(transform.position) },
                    new HashSet<GridCell>()
                );

            GridCell item;

            // get the last item in the queue
            while ((item = evaluatingPosition.points.Last()) != end)
            {
                var positionsCount = evaluatingPosition.points.Count();

                Debug.Log("Evaluating " + string.Join(",", evaluatingPosition));

                // check all surrounding positions
                for (var x = -1f; x <= 1f; x++)
                {
                    for (var y = -1f; y <= 1; y++)
                    {
                        var advance = new GridCell(item.vector.x + x, item.vector.y + y);

                        var isAdvanceRedundant = evaluatingPosition.pointsSet.Contains(advance);

                        if (advance != item && !map.IsObstacle(advance) && !isAdvanceRedundant) // except the current positions, obstacles or going back
                        {
                            var distanceToEnd = Vector2.Distance(advance.vector, end.vector); // use the distance between this point and the end as h(n)
                            var distanceFromStart = Vector2.Distance(currentPosition.vector, advance.vector); // use the distance between this point and the start as g(n)
                            var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                            var currentPositions = new List<GridCell>(evaluatingPosition.points) { advance };

                            Debug.Log("Enqueuing " + string.Join(",", currentPositions) + "with priority " + priority);

                            priorityQueue.Enqueue(
                                new QueueItem(currentPositions, new HashSet<GridCell>(currentPositions)),
                                priority
                            );
                        }
                    }
                }

                pass += 1;

                if (pass > 1000)
                {
                    Debug.Log("Short circuiting after 1000 passes");

                    return evaluatingPosition.points;
                }

                evaluatingPosition = priorityQueue.Dequeue();
            }

            Profiler.EndSample();

            return evaluatingPosition.points;
        }

        private GridCell CurrentCellPosition()
        {
            return map.WorldPositionToGridCell(transform.position);
        }
    }
}