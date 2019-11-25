using System.Collections.Generic;
using Pandora.Pool;
using System;
using UnityEngine.Profiling;
using UnityEngine;

namespace Pandora.Engine.Grid
{
    public class TightGrid
    {
        int height, width, rows, columns;
        Cell[,] grid;
        CustomSampler collisionCheck;


        public TightGrid(int height, int width, int rows, int columns)
        {
            this.height = height;
            this.width = width;
            this.rows = rows;
            this.columns = columns;

            grid = new Cell[rows, columns];

            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    grid[x, y] = new Cell(x, y, width, height);
                }
            }
        }

        IEnumerable<Cell> Cells() {
            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    yield return grid[x, y];
                }
            }
        }

        public IEnumerable<Collision> Collisions(Func<EngineEntity, EngineEntity, bool> isCollision) {
            foreach (var cell in Cells()) {
                foreach (var collision in cell.Collisions(isCollision)) {
                    yield return collision;
                }
            }
        }

        public void Insert(EngineEntity item)
        {
            var box = item.Engine.GetPooledEntityBounds(item);

            foreach (var vertex in box.Vertices)
            {
                var cellX = Math.Max(vertex.x / (width / rows), rows - 1);
                var cellY = Math.Max(vertex.y / (height / columns), columns - 1);

                Debug.Log($"Inserting into {cellX} and {cellY}");
                
                grid[cellX, cellY].Insert(item);
            }

            item.Engine.ReturnBounds(box);
        }

        public void Clear() {
            foreach (var cell in Cells()) {
                cell.Clear();
            }
        }

    }

}