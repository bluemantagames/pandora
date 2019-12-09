using System.Collections.Generic;
using System;
using Pandora.Pool;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using System.Threading;

namespace Pandora.Engine.Grid
{
    public class Cell
    {
        public LinkedList<EngineEntity> Items;
        CustomSampler collisionCheck, hitboxCheck, itemAddCheck;

        public Cell(int x, int y, int w, int h)
        {
            Items = new LinkedList<EngineEntity> { };

            collisionCheck = CustomSampler.Create($"Check collision {x}, {y}");
            hitboxCheck = CustomSampler.Create($"Check hitbox collision {x}, {y}");
            itemAddCheck = CustomSampler.Create($"Item added to {x}, {y}");
        }

        public void Insert(EngineEntity item)
        {
            itemAddCheck.Begin();
            Items.AddFirst(item);
            itemAddCheck.End();
        }

        public void Clear()
        {
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

                    hitboxCheck.Begin();

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

                    hitboxCheck.End();
                }
            }

            collisionCheck.End();

            return collisions;
        }
    }
}