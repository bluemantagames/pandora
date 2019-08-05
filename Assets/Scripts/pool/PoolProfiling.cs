using System.Threading;

namespace CRclone.Pool
{
    public class PoolProfiling
    {
        private long objectHitCount = 0;
        private long objectMissCount = 0;
        private long returnedToPoolCount = 0;
        private long instancesCreatedCount = 0;
        private long objectResetCount = 0;
        private long objectDisposeCount = 0;
        private long overflowCount = 0;

        /// <summary>
        /// Whether profiling is enabled or not.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The number of successful accesses to the pool
        /// (the pool had free objects to provide).
        /// </summary>
        public long ObjectHitCount => objectHitCount;

        /// <summary>
        /// The number of unsuccessful accesses to the pool
        /// (the pool had to create a new object to satisfy the request)
        /// </summary>
        public long ObjectMissCount => objectMissCount;

        /// <summary>
        /// The number of objects successfully returned to the pool.
        /// </summary>
        public long ReturnedToPoolCount => returnedToPoolCount;

        /// <summary>
        /// The number of instances created by the pool since its creation.
        /// </summary>
        public long InstancesCreatedCount => instancesCreatedCount;

        /// <summary>
        /// The number of objects cleared before reusing.
        /// </summary>
        public long ObjectResetCount => objectResetCount;

        /// <summary>
        /// The number of objects disposed.
        /// </summary>
        public long ObjectDisposeCount => objectDisposeCount;

        /// <summary>
        /// The number of pool overflows
        /// (an object was returned but there was no room for it)
        /// </summary>
        public long OverflowCount => overflowCount;

        /// <summary>
        /// Increments the pool object hit count.
        /// </summary>
        internal void IncrementObjectHitCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref objectHitCount);
            }
        }

        /// <summary>
        /// Increments the pool object miss count.
        /// </summary>
        internal void IncrementObjectMissCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref objectMissCount);
            }
        }

        /// <summary>
        /// Increments the returned objects count.
        /// </summary>
        internal void IncrementReturnedToPoolCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref returnedToPoolCount);
            }
        }

        /// <summary>
        /// Increments the created instances count.
        /// </summary>
        internal void IncrementInstancesCreatedCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref instancesCreatedCount);
            }
        }

        /// <summary>
        /// Increments the reset objects count.
        /// </summary>
        internal void IncrementObjectResetCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref objectResetCount);
            }
        }

        /// <summary>
        /// Increments the disposed objects count.
        /// </summary>
        internal void IncrementObjectDisposeCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref objectDisposeCount);
            }
        }

        /// <summary>
        /// Increments the pool overflows count.
        /// </summary>
        internal void IncrementOverflowCount()
        {
            if (Enabled)
            {
                Interlocked.Increment(ref overflowCount);
            }
        }

        /// <summary>
        /// Resets all collected metrics.
        /// </summary>
        internal void Reset()
        {
            objectHitCount = 0;
            objectMissCount = 0;
            returnedToPoolCount = 0;
            instancesCreatedCount = 0;
            objectResetCount = 0;
            objectDisposeCount = 0;
            overflowCount = 0;
        }
    }
}
