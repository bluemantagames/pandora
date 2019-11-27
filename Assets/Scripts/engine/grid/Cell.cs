using System.Collections.Generic;
using System;
using Pandora.Pool;
using UnityEngine.Profiling;

namespace Pandora.Engine.Grid
{
    public class Cell
    {
        public List<EngineEntity> Items;
        CustomSampler collisionCheck;
        public List<Collision> registeredCollisions;

        public Cell(int x, int y, int w, int h)
        {
            Items = new List<EngineEntity> { };

            collisionCheck = CustomSampler.Create($"Check collision {x}, {y}");
        }

        public void Insert(EngineEntity item)
        {
            Items.Add(item);
        }

        public void Clear()
        {
            if (registeredCollisions != null)
            {
                foreach (var collision in registeredCollisions)
                {
                    collision.First.Engine.ReturnBounds(collision.FirstBox);
                    collision.Second.Engine.ReturnBounds(collision.SecondBox);

                    PoolInstances.CollisionPool.ReturnObject(collision);
                }
            }

            PoolInstances.CollisionListPool.ReturnObject(registeredCollisions);

            Items.Clear();
        }

        public LinkedList<Collision> Collisions(Func<EngineEntity, EngineEntity, bool> isCollision, HashSet<(EngineEntity, EngineEntity)> processed)
        {
            collisionCheck.Begin();

            var collisions = new LinkedList<Collision>();

            foreach (var a in Items)
            {
                foreach (var b in Items)
                {
                    if (a == b) continue;

                    var first = (a.Timestamp > b.Timestamp) ? a : b;
                    var second = (first == a) ? b : a;
                    var pair = (first, second);

                    if (processed.Contains(pair))
                        continue;
                    else
                        processed.Add(pair);

                    var aBox = a.Engine.GetPooledEntityBounds(a);
                    var bBox = b.Engine.GetPooledEntityBounds(b);

                    var collision = PoolInstances.CollisionPool.GetObject();

                    collision.First = a;
                    collision.Second = b;

                    collision.FirstBox = aBox;
                    collision.SecondBox = bBox;

                    if (aBox.Collides(bBox) && isCollision(a, b))
                    {
                        collisions.AddFirst(new LinkedListNode<Collision>(collision));
                    }
                    else
                    {
                        a.Engine.ReturnBounds(aBox);
                        b.Engine.ReturnBounds(bBox);

                        PoolInstances.CollisionPool.ReturnObject(collision);
                    }

                }
            }

            collisionCheck.End();

            return collisions;
        }
    }
}