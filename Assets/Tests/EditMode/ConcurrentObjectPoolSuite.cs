using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Pool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ConcurrentObjectPoolSuite
    {
        /// <summary>
        /// A class that just wraps an integer.
        /// </summary>
        internal class WrappedInt
        {
            public int Value { get; set; }

            public WrappedInt(int value = 0)
            {
                Value = value;
            }

            public void SetToZero()
            {
                Value = 0;
            }

            public void Increment()
            {
                Value++;
            }
        }

        /// <summary>
        /// Basic test, covers all common use cases.
        /// </summary>
        [Test]
        public void TestBasic()
        {
            var pool = new ConcurrentObjectPool<WrappedInt>(
                createFunction: () => new WrappedInt(Random.Range(0, 1000)),
                resetFunction: i => i.SetToZero(),
                profilingEnabled: true
            );

            // Set pool maximum size to 100 objects
            const int ObjectsCount = 100;
            pool.MaximumPoolSize = ObjectsCount;

            // We now have 100 objects inside pool
            Assert.AreEqual(ObjectsCount, pool.PooledObjectsCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.InstancesCreatedCount);

            var objectsList = new List<WrappedInt>();

            for (int i = 0; i < ObjectsCount; ++i)
            {
                var obj = pool.GetObject();
                obj.Increment();
                objectsList.Add(obj);
            }

            Assert.AreEqual(0, pool.PooledObjectsCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.InstancesCreatedCount);
            Assert.AreEqual(0, pool.Profiling.ObjectDisposeCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ObjectHitCount);
            Assert.AreEqual(0, pool.Profiling.ObjectMissCount);
            Assert.AreEqual(0, pool.Profiling.ObjectResetCount);
            Assert.AreEqual(0, pool.Profiling.OverflowCount);
            Assert.AreEqual(0, pool.Profiling.ReturnedToPoolCount);

            // Return all objects
            objectsList.ForEach(pool.ReturnObject);

            Assert.AreEqual(ObjectsCount, pool.PooledObjectsCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ReturnedToPoolCount);
            Assert.AreEqual(0, pool.Profiling.ObjectDisposeCount);

            // Clear pool
            pool.Clear();

            Assert.AreEqual(0, pool.PooledObjectsCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ObjectDisposeCount);
        }

        /// <summary>
        /// Tests concurrent access to the pool
        /// </summary>
        [Test]
        public void TestConcurrentAccess()
        {
            var pool = new ConcurrentObjectPool<WrappedInt>(
                createFunction: () => new WrappedInt(),
                resetFunction: i => i.SetToZero(),
                profilingEnabled: true
            );

            const int ObjectsCount = 1000;

            // Resize pool
            pool.MaximumPoolSize = ObjectsCount;

            Parallel.For(0, ObjectsCount, (i, loopState) =>
            {
                var obj = pool.GetObject();
                obj.Value = i;
                pool.ReturnObject(obj);
            });

            Assert.AreEqual(ObjectsCount, pool.PooledObjectsCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.InstancesCreatedCount);
            Assert.AreEqual(0, pool.Profiling.ObjectDisposeCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ObjectHitCount);
            Assert.AreEqual(0, pool.Profiling.ObjectMissCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ObjectResetCount);
            Assert.AreEqual(0, pool.Profiling.OverflowCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ReturnedToPoolCount);
        }

        /// <summary>
        /// Tests concurrent resizing of the pool.
        /// Resize just ensure data integrity, but doesn't
        /// sequentially lock.
        /// </summary>
        [Test]
        public void TestConcurrentResizing()
        {
            var pool = new ConcurrentObjectPool<WrappedInt>(
                createFunction: () => new WrappedInt(),
                resetFunction: i => i.SetToZero(),
                profilingEnabled: true
            );
            
            const int MaxSize = 1000;

            Parallel.For(0, 1000, (i, loopState) =>
            {
                // Resize pool
                pool.MaximumPoolSize = MaxSize;
            });

            Assert.AreEqual(MaxSize, pool.PooledObjectsCount);
            Assert.AreEqual(MaxSize, pool.MaximumPoolSize);
            Assert.AreEqual(MaxSize, pool.Profiling.InstancesCreatedCount);
        }

        /// <summary>
        /// Tests pool overflow
        /// </summary>
        [Test]
        public void TestOverflow()
        {
            var pool = new ConcurrentObjectPool<WrappedInt>(
                createFunction: () => new WrappedInt(),
                resetFunction: i => i.SetToZero(),
                profilingEnabled: true
            );

            const int ObjectsCount = 100;
            const int OverflowSize = 15;

            // Set pool size
            pool.MaximumPoolSize = ObjectsCount;

            var objectsList = new List<WrappedInt>();

            for (int i = 0; i < ObjectsCount + OverflowSize; ++i)
            {
                var obj = pool.GetObject();
                obj.Increment();
                objectsList.Add(obj);
            }

            Assert.AreEqual(0, pool.PooledObjectsCount);
            Assert.AreEqual(0, pool.Profiling.OverflowCount);
            Assert.AreEqual(0, pool.Profiling.ObjectDisposeCount);
            Assert.AreEqual(ObjectsCount, pool.Profiling.ObjectHitCount);
            Assert.AreEqual(OverflowSize, pool.Profiling.ObjectMissCount);
            Assert.AreEqual(ObjectsCount + OverflowSize, pool.Profiling.InstancesCreatedCount);

            objectsList.ForEach(pool.ReturnObject);

            Assert.AreEqual(OverflowSize, pool.Profiling.OverflowCount);
            Assert.AreEqual(ObjectsCount, pool.PooledObjectsCount);
        }
    }
}
