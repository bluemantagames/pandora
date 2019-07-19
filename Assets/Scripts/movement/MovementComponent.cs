using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Priority_Queue;
using CRclone;
using CRclone.Combat;

namespace CRclone.Movement
{
    public class MovementComponent : MonoBehaviour
    {
        Rigidbody2D body;
        Vector2 currentTarget;
        Vector2 worldCurrentTarget;
        Vector2 direction;
        List<Vector2> currentPath;
        TeamComponent team;
        Enemy targetEnemy;
        CombatBehaviour combatBehaviour;

        public float speed = 1f;
        public MapListener map;

        // Start is called before the first frame update
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            team = GetComponent<TeamComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
        }

        public MovementState Move()
        {
            Vector2 position = transform.position;
            var currentPosition = CurrentCellPosition();

            var enemy = map.GetNearestEnemy(gameObject, currentPosition, team.team);

            Debug.Log("Enemy " + enemy);

            // first and foremost, if an enemy is in range: attack them
            if (enemy != null && targetEnemy == null)
            {
                targetEnemy = enemy;

                currentPath = null;

                return new MovementState(enemy, MovementStateEnum.MovingTowardsEnemy);
            }

            // remove targeted enemy if they are dead
            if (targetEnemy != null && targetEnemy.enemy.GetComponent<LifeComponent>().isDead) {
                targetEnemy = null;
            }

            // if you're attacking an enemy: keep attacking
            if (targetEnemy != null && combatBehaviour.IsInRange(currentPosition, targetEnemy.enemyCell)) {
                return new MovementState(enemy, MovementStateEnum.EnemyApproached);
            }

            // if no path has been calculated: calculate one and point the object to the first position in the queue
            if (currentPath == null)
            {
                currentPath = FindPath(map.GetTarget(gameObject, currentPosition, team.team));

                Debug.Log("Found path " + string.Join(",", currentPath));

                AdvancePosition(currentPosition);
            }

            // if current position is in the queue it means we need to advance target
            if (currentPath.Contains(currentPosition))
            {
                AdvancePosition(currentPosition);
            }

            position += direction * (Time.deltaTime * speed);

            body.MovePosition(position);

            return new MovementState(null, MovementStateEnum.Moving);
        }

        /**
         * This sets the new grid cell target for the pathing and removes the current from the queue, effectively
         * "advancing" pathing forward
         */
        private void AdvancePosition(Vector2 currentPosition)
        {
            if (currentPath.Count() > 1) // if path still has elements after we remove the current target
            {
                Debug.Log("Advancing from " + currentPosition);

                currentPath.Remove(currentPosition);

                currentTarget = currentPath.First();
                worldCurrentTarget = map.GridCellToWorldPosition(currentTarget);
                direction = (currentTarget - currentPosition).normalized;
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
        List<Vector2> FindPath(Vector2 end)
        {
            var priorityQueue = new SimplePriorityQueue<QueueItem>();
            var currentPosition = map.WorldPositionToGridCell(transform.position);

            int pass = 0;

            var evaluatingPosition =
                new QueueItem(
                    new List<Vector2> { map.WorldPositionToGridCell(transform.position) },
                    new HashSet<Vector2>()
                );

            Vector2 item;

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
                        var advance = new Vector2(item.x + x, item.y + y);

                        var isAdvanceRedundant = evaluatingPosition.pointsSet.Contains(advance);

                        if (advance != item && !map.IsObstacle(advance) && !isAdvanceRedundant) // except the current positions, obstacles or going back
                        {
                            var distanceToEnd = Vector2.Distance(advance, end); // use the distance between this point and the end as h(n)
                            var distanceFromStart = Vector2.Distance(currentPosition, advance); // use the distance between this point and the start as g(n)
                            var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                            var currentPositions = new List<Vector2>(evaluatingPosition.points) { advance };

                            Debug.Log("Enqueuing " + string.Join(",", currentPositions) + "with priority " + priority);

                            priorityQueue.Enqueue(
                                new QueueItem(currentPositions, new HashSet<Vector2>(currentPositions)),
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

            return evaluatingPosition.points;
        }

        private Vector2 CurrentCellPosition()
        {
            return map.WorldPositionToGridCell(transform.position);
        }
    }
}