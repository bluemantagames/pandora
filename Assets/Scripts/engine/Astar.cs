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

        Stack<System.Diagnostics.Stopwatch> stopWatches = new Stack<System.Diagnostics.Stopwatch> { };

        void StartStopwatch(bool ignoreDebug = false)
        {
            if (DebugPathfinding || ignoreDebug)
            {
                var stopWatch = new System.Diagnostics.Stopwatch();

                stopWatch.Start();

                stopWatches.Push(stopWatch);
            }
        }

        void LogStopwatch(string log, bool ignoreDebug = false)
        {
            if (DebugPathfinding || ignoreDebug)
            {
                var stopWatch = stopWatches.Pop();

                stopWatch.Stop();

                Debug.Log($"{log} took {stopWatch.Elapsed}");
            }
        }

        /**
        * Simple A* implementation. We try to use as many pools
        * as humanly possible in order to not allocate too much (it costs a lot of time)
        *
        * When greedy is true, g(x) is removed from the score, making this algorithm effectively
        * a greedy best-first search, which seems to perform better when evading other units.
        */
        public LinkedList<T> FindLinkedListPath(T currentPosition, T end, Func<T, bool> isObstacle, Func<T, List<T>> getSurroundingNodes, Func<T, T, float> distance, bool greedy = false)
        {
            var priorityQueue = new SimplePriorityQueue<QueueItem<T>>();

            priorityQueue.Clear();

            var cameFrom = new Dictionary<QueueItem<T>, QueueItem<T>>(100000);
            var queueItems = new Dictionary<T, QueueItem<T>>(100000);
            var gScore = new Dictionary<T, float>(100000);
            var fScore = new Dictionary<T, float>(100000);

            int pass = 0, advancesNum = 0;

            var map = MapComponent.Instance;

            var evaluatingPosition =
                new QueueItem<T>(
                    currentPosition
                );

            T item;

            var pathFound = false;

            if (isObstacle(end))
            {
                Debug.LogWarning($"Cannot find path towards an obstacle ({end})");

                return new LinkedList<T> { };
            }

            if (currentPosition.Equals(end))
            {
                return new LinkedList<T> { };
            }

            gScore[currentPosition] = 0;
            fScore[currentPosition] = 0;

            StartStopwatch(true);

            // get the last item in the queue
            while (!(item = evaluatingPosition.Item).Equals(end) && !pathFound)
            {
                StartStopwatch();

                if (item.Equals(end))
                {
                    pathFound = true;

                    break;
                }

                StartStopwatch();
                var advances = getSurroundingNodes(item);
                LogStopwatch("Get surrounding nodes");

                if (DebugPathfinding)
                {
                    Debug.Log($"Checking {item}");
                }

                foreach (var advance in advances)
                {
                    advancesNum++;

                    StartStopwatch();
                    if (isObstacle(advance) || advance.Equals(item))
                    {
                        nodePool.ReturnObject(advance);

                        LogStopwatch("Obstacle");

                        continue;
                    }
                    LogStopwatch("Obstacle");

                    QueueItem<T> queueItem = null;

                    if (queueItems.ContainsKey(advance))
                    {
                        queueItem = queueItems[advance];
                    }
                    else
                    {
                        queueItem = nodeQueueItemPool.GetObject();

                        queueItem.Item = advance;

                        queueItems[advance] = queueItem;
                    }

                    var advanceGScore = gScore[item] + 1;

                    if (!gScore.ContainsKey(advance) || gScore[advance] > advanceGScore)
                    {
                        cameFrom[queueItem] = evaluatingPosition;
                        gScore[advance] = advanceGScore;
                        fScore[advance] = (greedy ? 0 : advanceGScore) + distance(advance, end);

                        priorityQueue.Enqueue(queueItem, fScore[advance]);
                    }
                }

                nodeContainerPool?.ReturnObject(advances);

                LogStopwatch("Pass");

                pass += 1;

                if (pass > 100000)
                {
                    Debug.LogWarning($"Short circuiting after 100000 passes started from {currentPosition} to {end} ({Time.frameCount}, checked {advancesNum} advances)");
                    Debug.LogWarning("Best paths found are");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");
                    Debug.LogWarning($"{priorityQueue.Dequeue()}");

                    if (DebugPathfinding)
                    {
                        Debug.Log("DebugPathfinding: Pausing the editor");

                        Debug.Break();
                    }

                    return new LinkedList<T> { };
                }

                evaluatingPosition = priorityQueue.Dequeue();
            }

            LogStopwatch($"Total pathfinding using {pass} iterations", true);

            var path = new LinkedList<T> { };

            while (cameFrom.ContainsKey(evaluatingPosition))
            {
                path.AddFirst(new LinkedListNode<T>(cameFrom[evaluatingPosition].Item));

                evaluatingPosition = cameFrom[evaluatingPosition];
            }

            foreach (var queueItem in cameFrom.Keys)
            {
                nodeQueueItemPool.ReturnObject(queueItem);
            }

            return path;
        }

        /**
        * Simple A* implementation. We try to use as many pools
        * as humanly possible in order to not allocate too much (it costs a lot of time)
        *
        * When greedy is true, g(x) is removed from the score, making this algorithm effectively
        * a greedy best-first search, which seems to perform better when evading other units.
        */
        public List<T> FindPath(T currentPosition, T end, Func<T, bool> isObstacle, Func<T, List<T>> getSurroundingNodes, Func<T, T, float> distance, bool greedy = false)
        {
            return FindLinkedListPath(currentPosition, end, isObstacle, getSurroundingNodes, distance, greedy).ToList();
        }


        /**
        * Simple A* implementation. We try to use as many pools
        * as humanly possible in order to not allocate too much (it costs a lot of time)
        *
        * When greedy is true, g(x) is removed from the score, making this algorithm effectively
        * a greedy best-first search, which seems to perform better when evading other units.
        */
        public IEnumerator<T> FindPathEnumerator(T currentPosition, T end, Func<T, bool> isObstacle, Func<T, List<T>> getSurroundingNodes, Func<T, T, float> distance, bool greedy = false)
        {
            return FindLinkedListPath(currentPosition, end, isObstacle, getSurroundingNodes, distance, greedy).GetEnumerator();
        }
    }
}