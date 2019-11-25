using System.Collections.Generic;
using System;
using Pandora.Pool;

namespace Pandora.Engine.Grid {
    public class Cell {
        public List<EngineEntity> Items;

        public Cell(int x, int y, int w, int h) {
            Items = new List<EngineEntity> {};
        }

        public void Insert(EngineEntity item) {
            Items.Add(item);
        }

        public void Clear() {
            Items.Clear();
        }
        
        public IEnumerable<Collision> Collisions(Func<EngineEntity, EngineEntity, bool> isCollision) {
            foreach (var a in Items) {
                foreach (var b in Items) {
                    var aBox = a.Engine.GetPooledEntityBounds(a);
                    var bBox = b.Engine.GetPooledEntityBounds(b);

                    if (aBox.Collides(bBox) && isCollision(a, b)) {
                        yield return new Collision(a, b);
                    }

                    a.Engine.ReturnBounds(aBox);
                    b.Engine.ReturnBounds(bBox);
                }
            }
        }
    }
}