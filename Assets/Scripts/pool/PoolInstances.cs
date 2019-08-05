using UnityEngine;

namespace CRclone.Pool
{
    /// <summary>
    /// A shared collection of pool instances.
    /// </summary>
    public static class PoolInstances
    {
        public static ConcurrentObjectPool<Vector2> Vector2Pool = new ConcurrentObjectPool<Vector2>(
            createFunction: () => new Vector2(),
            resetFunction: v => v.Set(0, 0),
            profilingEnabled: true
        );
    }
}
