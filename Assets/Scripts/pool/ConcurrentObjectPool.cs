using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Pandora.Pool
{
    /// <summary>
    /// Object Pool constants.
    /// </summary>
    public static class ConcurrentObjectPool
    {
        /// <summary>
        /// Default maximum size of a pool.
        /// </summary>
        public const int DefaultMaximumPoolSize = 100;
    }

    /// <summary>
    /// A generic concurrent object pool
    /// </summary>
    public class ConcurrentObjectPool<T>
    {
        #region Variables and Properties
        private int poolResizeCASFlag = 0;
        private int maximumPoolSize = 10;

        public int MaximumPoolSize
        {
            get => maximumPoolSize;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                maximumPoolSize = value;

                ResizePool();
            }
        }

        /// <summary>
        /// The function used to create new objects.
        /// </summary>
        public Func<T> CreateFunction { get; private set; }

        /// <summary>
        /// The action used to reset objects.
        /// </summary>
        public Action<T> ResetFunction { get; private set; }

        /// <summary>
        /// The action used to dispose objects.
        /// </summary>
        public Action<T> DisposeFunction { get; private set; }

        /// <summary>
        /// Objects bag
        /// </summary>
        private ConcurrentBag<T> PooledObjects { get; set; }
        
        /// <summary>
        /// Number of objects available in the pool
        /// </summary>
        public int PooledObjectsCount { get => PooledObjects.Count; }

        /// <summary>
        /// Profiling information
        /// </summary>
        public PoolProfiling Profiling { get; private set; }
        #endregion

        /// <summary>
        /// Initializes a new pool with the specified methods.
        /// </summary>
        /// <param name="createFunction">The method to be used to create new objects</param>
        /// <param name="resetFunction">The method to be used to clear objects</param>
        /// <param name="disposeFunction">This method will be called to dispose objects</param>
        /// <param name="profilingEnabled">Whether to enable profiling or not</param>
        public ConcurrentObjectPool(
            Func<T> createFunction = null,
            Action<T> resetFunction = null,
            Action<T> disposeFunction = null,
            bool profilingEnabled = false)
        {
            PooledObjects = new ConcurrentBag<T>();
            Profiling = new PoolProfiling { Enabled = profilingEnabled };
            // If no create function is provided, try to use the default constructor
            CreateFunction = createFunction ?? (() => Activator.CreateInstance<T>());
            ResetFunction = resetFunction;
            DisposeFunction = disposeFunction;

            // Resize the Pool to default size
            MaximumPoolSize = ConcurrentObjectPool.DefaultMaximumPoolSize;
        }

        /// <summary>
        /// Pool destructor
        /// </summary>
        ~ConcurrentObjectPool()
        {
            Clear();
        }

        /// <summary>
        /// Clears the pool and dispose every object inside it.
        /// </summary>
        public void Clear()
        {
            while (PooledObjects.TryTake(out var objToDestroy))
            {
                DisposeObject(objToDestroy);
            }
        }

        /// <summary>
        /// Resets all profiling metrics.
        /// </summary>
        public void ResetProfiling()
        {
            Profiling.Reset();
        }

        private T CreateObject()
        {
            Profiling.IncrementInstancesCreatedCount();
            return CreateFunction();
        }

        private void ResetObject(T obj)
        {
            Profiling.IncrementObjectResetCount();
            ResetFunction?.Invoke(obj);
        }

        private void DisposeObject(T obj)
        {
            Profiling.IncrementObjectDisposeCount();
            DisposeFunction?.Invoke(obj);
        }

        /// <summary>
        /// Gets an object from the pool. If there aren't any object
        /// available, a new one is instantiated.
        /// </summary>
        public T GetObject()
        {
            if (PooledObjects.TryTake(out var objToReturn))
            {
                Profiling.IncrementObjectHitCount();
                return objToReturn;
            }
            else
            {
                Profiling.IncrementObjectMissCount();
                return CreateObject();
            }
        }

        /// <summary>
        /// Returns the given object to the pool.
        /// </summary>
        public void ReturnObject(T obj)
        {
            if (PooledObjectsCount < MaximumPoolSize)
            {
                ResetObject(obj);
                PooledObjects.Add(obj);
                Profiling.IncrementReturnedToPoolCount();
            }
            else
            {
                // Pool exceeded its maximum size. Just dispose the object.
                DisposeObject(obj);
                Profiling.IncrementOverflowCount();
            }
        }

        /// <summary>
        /// Resizes the pool adding or removing objects accordingly.
        /// This method guarantees data integrity when called from multiple threads,
        /// but doesn't lock sequentially.
        /// </summary>
        private void ResizePool()
        {
            if (Interlocked.CompareExchange(ref poolResizeCASFlag, 1, 0) == 0)
            {
                while (PooledObjectsCount < MaximumPoolSize)
                {
                    PooledObjects.Add(CreateObject());
                }

                while (PooledObjectsCount > MaximumPoolSize &&
                       PooledObjects.TryTake(out var objToDestroy))
                {
                    DisposeObject(objToDestroy);
                }

                poolResizeCASFlag = 0;
            }
        }
    }
}
