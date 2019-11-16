using System.Collections.Generic;
using System;
using Priority_Queue;
using Pandora;
using UnityEngine;
using System.Linq;
using Pandora.Pool;

namespace Pandora.Engine
{
    public class Astar<T> where T : IEquatable<T>
    {
        public bool DebugPathfinding = false;

        ConcurrentObjectPool<HashSet<T>> nodeHashsetPool;
        ConcurrentObjectPool<QueueItem<T>> nodeQueueItemPool;
        ConcurrentObjectPool<T> nodePool;
        ConcurrentObjectPool<List<T>> nodeContainerPool;

        public Astar(ConcurrentObjectPool<HashSet<T>> nodeHashsetPool, ConcurrentObjectPool<QueueItem<T>> nodeQueueItemPool, ConcurrentObjectPool<T> nodePool, ConcurrentObjectPool<List<T>> nodeContainerPool)
        {
            this.nodeHashsetPool = nodeHashsetPool;
            this.nodeQueueItemPool = nodeQueueItemPool;
            this.nodePool = nodePool;
            this.nodeContainerPool = nodeContainerPool;
        }

        /**
        * Simple A* implementation. We try to use as many pools
        * as humanly possible in order to not allocate too much (it costs a lot of time)
        */
        public List<T> FindPath(T currentPosition, T end, Func<T, bool> isObstacle, Func<T, List<T>> getSurroundingNodes, Func<T, T, float> distance)
        {
            var priorityQueue = new SimplePriorityQueue<QueueItem<T>>();

            priorityQueue.Clear();

            int pass = 0, advancesNum = 0;

            var map = MapComponent.Instance;

            var evaluatingPosition =
                new QueueItem<T>(
                    new List<T> { currentPosition },
                    new HashSet<T>()
                );

            T item;

            var pathFound = false;

            if (isObstacle(end))
            {
                Debug.LogWarning($"Cannot find path towards an obstacle ({end})");

                return evaluatingPosition.points;
            }

            if (currentPosition.Equals(end))
            {
                return evaluatingPosition.points;
            }

            // get the last item in the queue
            while (!(item = evaluatingPosition.points.Last()).Equals(end) && !pathFound)
            {

                if (DebugPathfinding)
                {
                    Debug.Log($"DebugPathfinding: Positions {string.Join(", ", evaluatingPosition.points)} - searching for {end}");
                }

                var advances = getSurroundingNodes(item);

                foreach (var advance in advances)
                {
                    advancesNum++;

                    var isAdvanceRedundant = evaluatingPosition.pointsSet.Contains(advance);

                    System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();

                    st.Start();

                    if (!advance.Equals(item) && !isObstacle(advance) && !isAdvanceRedundant) // except the current positions, obstacles or going back
                    {
                        st.Stop();

                        if (DebugPathfinding)
                        {
                            Debug.Log($"MyMethod took {st.Elapsed} ms to complete");
                            Debug.Break();
                        }

                        var distanceToEnd = distance(advance, end); // use the distance between this node and the end as h(n)
                        var distanceFromStart = evaluatingPosition.points.Count + 1; // use the distance between this node and the start as g(n)
                        var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                        var currentPositions = new List<T>(evaluatingPosition.points) { advance };
                        var queueItem = nodeQueueItemPool.GetObject();

                        queueItem.points = currentPositions;
                        queueItem.pointsSet = nodeHashsetPool.GetObject();

                        foreach (var position in currentPositions)
                        {
                            queueItem.pointsSet.Add(position);
                        }

                        if (advance.Equals(end))
                        { // Stop the loop if we found the path
                            evaluatingPosition = queueItem;
                            pathFound = true;

                            break;
                        }

                        nodeHashsetPool.ReturnObject(evaluatingPosition.pointsSet);
                        nodeQueueItemPool.ReturnObject(evaluatingPosition);

                        priorityQueue.Enqueue(
                            queueItem,
                            priority
                        );
                    }
                    else
                    {
                        nodePool?.ReturnObject(advance);
                    }
                }

                nodeContainerPool?.ReturnObject(advances);

                pass += 1;

                if (pass > 5000)
                {
                    Debug.LogWarning($"Short circuiting after 5000 passes started from {currentPosition} to {end} ({Time.frameCount}, checked {advancesNum} advances)");
                    Debug.LogWarning("Best paths found are");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");

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
    }
}