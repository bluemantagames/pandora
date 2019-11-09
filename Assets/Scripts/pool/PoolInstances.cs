using UnityEngine;
using Pandora.Engine;
using System.Collections.Generic;
using System;

namespace Pandora.Pool
{
    /// <summary>
    /// A shared collection of pool instances.
    /// </summary>
    public static class PoolInstances
    {
        public static ConcurrentObjectPool<Vector2> Vector2Pool = new ConcurrentObjectPool<Vector2>(
            createFunction: () => new Vector2(),
            resetFunction: v => v.Set(0, 0),
            profilingEnabled: false
        );

        public static ConcurrentObjectPool<BoxBounds> BoxBoundsPool = new ConcurrentObjectPool<BoxBounds>(
            createFunction: () => new BoxBounds(),
            resetFunction: b => b.Clear(),
            profilingEnabled: false
        );

        public static ConcurrentObjectPool<GridCell> GridCellPool = new ConcurrentObjectPool<GridCell>(
            createFunction: () => new GridCell(new Vector2(0, 0)),
            resetFunction: v => v.vector.Set(0, 0),
            profilingEnabled: false
        );

        public static ConcurrentObjectPool<Vector2Int> Vector2IntPool = new ConcurrentObjectPool<Vector2Int>(
            createFunction: () => new Vector2Int(),
            resetFunction: v => v.Set(0, 0),
            profilingEnabled: false
        );

        public static ConcurrentObjectPool<List<GridCell>> GridCellListPool = new ConcurrentObjectPool<List<GridCell>>(
            createFunction: () => new List<GridCell>(100),
            resetFunction: v => v.Clear(),
            profilingEnabled: false
        );


        public static ConcurrentObjectPool<HashSet<GridCell>> GridCellHashSetPool = new ConcurrentObjectPool<HashSet<GridCell>>(
            createFunction: () => new HashSet<GridCell>(),
            resetFunction: v => v.Clear(),
            profilingEnabled: false
        );


        public static ConcurrentObjectPool<HashSet<Vector2>> VectorHashSetPool = new ConcurrentObjectPool<HashSet<Vector2>>(
            createFunction: () => new HashSet<Vector2>(),
            resetFunction: v => v.Clear(),
            profilingEnabled: false
        );

        public static ConcurrentObjectPool<QueueItem> QueueItemPool = new ConcurrentObjectPool<QueueItem>(
            createFunction: () => new QueueItem(),
            resetFunction: v => {
                v.points = new List<Vector2> {};
                v.pointsSet = new HashSet<Vector2> {};
            },
            profilingEnabled: false
        );
        public static ConcurrentObjectPool<Decimal> DecimalPool = new ConcurrentObjectPool<Decimal>(
            createFunction: () => new Decimal(),
            resetFunction: d => d = 0,
            profilingEnabled: false
        );
    }
}
