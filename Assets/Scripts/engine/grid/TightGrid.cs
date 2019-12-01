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
        HashSet<(EngineEntity, EngineEntity)> processedCollisions = new HashSet<(EngineEntity, EngineEntity)> {};


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
            processedCollisions.Clear();

            var collisions = new List<Collision> {};

            foreach (var cell in Cells()) {
                var cellCollisions = cell.Collisions(isCollision, processedCollisions);

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

            var centerCellX = Math.Min(box.Center.x / (width / rows), rows - 1);
            var centerCellY = Math.Min(box.Center.y / (height / columns), columns - 1);
            
            grid[centerCellX, centerCellY].Insert(item);

            item.Engine.ReturnBounds(box);
        }

        public void Clear() {
            foreach (var cell in Cells()) {
                cell.Clear();
            }
        }

    }

}