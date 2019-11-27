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

        public List<Collision> Collisions(Func<EngineEntity, EngineEntity, bool> isCollision) {
            var checksum = 1L;
            var collisions = new List<Collision> {};

            foreach (var cell in Cells()) {
                var (cellCollisions, updatedChecksum) = cell.Collisions(isCollision, checksum);

                checksum = updatedChecksum;

                collisions.AddRange(cellCollisions);
            }

            return collisions;
        }

        public void Insert(EngineEntity item)
        {
            var box = item.Engine.GetPooledEntityBounds(item);

            foreach (var vertex in box.Vertices)
            {
                var cellX = Math.Min(vertex.x / (width / rows), rows - 1);
                var cellY = Math.Min(vertex.y / (height / columns), columns - 1);
                
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