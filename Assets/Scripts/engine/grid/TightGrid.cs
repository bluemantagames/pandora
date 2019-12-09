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
        HashSet<(EngineEntity, EngineEntity)> processedCollisions = new HashSet<(EngineEntity, EngineEntity)> { };


        public TightGrid(int height, int width, int rows, int columns)
        {
            this.height = height;
            this.width = width;
            this.rows = rows;
            this.columns = columns;

            grid = new Cell[rows + 1, columns + 1];

            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    grid[x, y] = new Cell(x, y, width, height);
                }
            }
        }

        IEnumerable<Cell> Cells()
        {
            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    yield return grid[x, y];
                }
            }
        }

        public LinkedList<Collision> Collisions(Func<EngineEntity, EngineEntity, bool> isCollision)
        {
            processedCollisions.Clear();

            var collisions = new LinkedList<Collision> { };

            foreach (var cell in Cells())
            {
                var cellCollisions = cell.Collisions(isCollision, processedCollisions, collisions);
            }

            return collisions;
        }

        public void Insert(EngineEntity item)
        {
            var box = item.Engine.GetPooledEntityBounds(item);

            var startX = Math.Min(box.LowerLeft.x / (width / rows), rows - 1);
            var endX = Math.Min(box.LowerRight.x / (width / rows), rows - 1);

            var startY = Math.Min(box.LowerLeft.y / (height / columns), columns - 1);
            var endY = Math.Min(box.UpperLeft.y / (height / columns), columns - 1);

            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    if (x <= rows - 1 && y <= grid.GetLength(1) - 1)
                    {
                        grid[x, y].Insert(item);
                    }
                }
            }

            item.Engine.ReturnBounds(box);
        }

        public void Clear()
        {
            foreach (var cell in Cells())
            {
                cell.Clear();
            }
        }

    }

}