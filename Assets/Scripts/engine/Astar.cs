using System.Collections.Generic;
using System;
using Priority_Queue;
using Pandora;
using UnityEngine;
using System.Linq;
using Pandora.Pool;
using UnityEngine.Profiling;

namespace Pandora.Engine
{
    public class Astar<T> where T : IEquatable<T>
    {
        public bool DebugPathfinding = false;

        ConcurrentObjectPool<HashSet<T>> nodeHashsetPool;
        ConcurrentObjectPool<QueueItem<T>> nodeQueueItemPool;
        ConcurrentObjectPool<T> nodePool;
        ConcurrentObjectPool<List<T>> nodeContainerPool;
        Dictionary<T, bool> obstacleCache = new Dictionary<T, bool>(1000000);

        StablePriorityQueue<QueueItem<T>> priorityQueue = new StablePriorityQueue<QueueItem<T>>(1000000);

        Dictionary<QueueItem<T>, QueueItem<T>> cameFrom = new Dictionary<QueueItem<T>, QueueItem<T>>(100000);
        Dictionary<T, QueueItem<T>> queueItems = new Dictionary<T, QueueItem<T>>(100000);
        Dictionary<T, float> gScore = new Dictionary<T, float>(100000);
        Dictionary<T, float> fScore = new Dictionary<T, float>(100000);
        List<QueueItem<T>> dequeueCandidates = new List<QueueItem<T>>(5000);

        CustomSampler surroundingSampler, obstacleSampler, passSampler;

        private static Astar<Vector2Int> _vector2Instance = null;

        static public Astar<Vector2Int> Vector2Instance
        {
            get
            {
                if (_vector2Instance == null)
                {
                    _vector2Instance = new Astar<Vector2Int>(
                        PoolInstances.Vector2IntHashSetPool,
                        PoolInstances.Vector2IntQueueItemPool,
                        PoolInstances.Vector2IntPool,
                        PoolInstances.Vector2IntListPool
                    );
                }

                return _vector2Instance;
            }
        }

        public Astar(ConcurrentObjectPool<HashSet<T>> nodeHashsetPool, ConcurrentObjectPool<QueueItem<T>> nodeQueueItemPool, ConcurrentObjectPool<T> nodePool, ConcurrentObjectPool<List<T>> nodeContainerPool)
        {
            this.nodeHashsetPool = nodeHashsetPool;
            this.nodeQueueItemPool = nodeQueueItemPool;
            this.nodePool = nodePool;
            this.nodeContainerPool = nodeContainerPool;

            surroundingSampler = CustomSampler.Create("Get surrounding nodes");
            obstacleSampler = CustomSampler.Create("Check obstacles");
            passSampler = CustomSampler.Create("Pathfinding pass");
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

                Logger.Debug($"{log} took {stopWatch.Elapsed}");
            }
        }

        LinkedList<T> BuildPath(QueueItem<T> evaluatingPosition)
        {
            var path = new LinkedList<T> { };

            path.AddFirst(evaluatingPosition.Item);

            while (cameFrom.ContainsKey(evaluatingPosition))
            {
                path.AddFirst(new LinkedListNode<T>(cameFrom[evaluatingPosition].Item));

                evaluatingPosition = cameFrom[evaluatingPosition];
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
        public LinkedList<T> FindLinkedListPath(T currentPosition, T end, Func<T, bool> isObstacle, Func<T, List<T>> getNeighbours, Func<T, T, float> distance, bool greedy = false)
        {
            priorityQueue.Clear();
            cameFrom.Clear();
            queueItems.Clear();
            gScore.Clear();
            fScore.Clear();
            dequeueCandidates.Clear();

            int pass = 0, neighboursNum = 0;

            var map = MapComponent.Instance;

            var evaluatingPosition =
                new QueueItem<T>(
                    currentPosition
                );

            T item;

            var pathFound = false;

            if (isObstacle(end))
            {
                Logger.DebugWarning($"Cannot find path towards an obstacle ({end})");

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
                passSampler.Begin();

                if (item.Equals(end))
                {
                    pathFound = true;

                    break;
                }

                surroundingSampler.Begin();
                var neighbours = getNeighbours(item);
                surroundingSampler.End();

                if (DebugPathfinding)
                {
                    Logger.Debug($"Checking {item}");
                }

                foreach (var neighbour in neighbours)
                {
                    neighboursNum++;

                    obstacleSampler.Begin();

                    if (!obstacleCache.ContainsKey(neighbour)) {
                        obstacleCache[neighbour] = isObstacle(neighbour);
                    }

                    if (obstacleCache[neighbour] || neighbour.Equals(item))
                    {
                        nodePool.ReturnObject(neighbour);

                        obstacleSampler.End();

                        continue;
                    }
                    obstacleSampler.End();

                    QueueItem<T> queueItem = null;

                    if (queueItems.ContainsKey(neighbour))
                    {
                        queueItem = queueItems[neighbour];
                    }
                    else
                    {
                        queueItem = nodeQueueItemPool.GetObject();

                        queueItem.Item = neighbour;

                        queueItems[neighbour] = queueItem;
                    }

                    var advanceGScore = gScore[item] + 1;

                    if (!gScore.ContainsKey(neighbour) || gScore[neighbour] > advanceGScore)
                    {
                        cameFrom[queueItem] = evaluatingPosition;
                        gScore[neighbour] = advanceGScore;
                        fScore[neighbour] = (greedy ? 0 : advanceGScore) + distance(neighbour, end);

                        // We don't update when in greedy mode, since the fScore doesn't change
                        if (priorityQueue.Contains(queueItem) && !greedy)
                        {
                            priorityQueue.UpdatePriority(queueItem, fScore[neighbour]);
                        }
                        else
                        {
                            priorityQueue.Enqueue(queueItem, fScore[neighbour]);
                        }
                    }
                }

                nodeContainerPool?.ReturnObject(neighbours);

                passSampler.End();

                pass += 1;

                if (priorityQueue.Count > 0)
                {
                    evaluatingPosition = priorityQueue.Dequeue();
                }
                else
                {
                    LogStopwatch($"Total pathfinding cut because no path found using {pass} iterations", true);

                    return BuildPath(evaluatingPosition);
                }

                if (pass > 2000)
                {
                    Logger.DebugWarning($"Short circuiting after {pass} passes started from {currentPosition} to {end} ({Time.frameCount}, checked {neighboursNum} nodes)");

                    if (DebugPathfinding)
                    {
                        Logger.Debug("DebugPathfinding: Pausing the editor");

                        Debug.Break();
                    }

                    LogStopwatch($"Total pathfinding cut using {pass} iterations", true);

                    return BuildPath(evaluatingPosition);
                }
            }

            var path = BuildPath(evaluatingPosition);

            foreach (var queueItem in cameFrom.Keys)
            {
                priorityQueue.ResetNode(queueItem);

                nodeQueueItemPool.ReturnObject(queueItem);
            }

            LogStopwatch($"Total pathfinding using {pass} iterations, path is long {path.Count}", true);

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