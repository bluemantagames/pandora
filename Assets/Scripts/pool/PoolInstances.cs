using UnityEngine;
using Pandora.Engine;

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


        public static ConcurrentObjectPool<Vector2Int> Vector2IntPool = new ConcurrentObjectPool<Vector2Int>(
            createFunction: () => new Vector2Int(),
            resetFunction: v => v.Set(0, 0),
            profilingEnabled: false
        );
    }
}
