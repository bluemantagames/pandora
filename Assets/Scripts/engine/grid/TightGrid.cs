using System.Collections.Generic;
using Pandora.Pool;
using System;
using UnityEngine.Profiling;
using UnityEngine;

namespace Pandora.Engine.Grid
{
    public class TightGrid
    {
        int mapHeight, mapWidth, rows, columns;
        Cell[][] grid;
        CustomSampler collisionCheck;
        HashSet<(EngineEntity, EngineEntity)> processedCollisions = new HashSet<(EngineEntity, EngineEntity)> { };


        public TightGrid(int height, int width, int rows, int columns)
        {
            this.mapHeight = height;
            this.mapWidth = width;
            this.rows = rows;
            this.columns = columns;

            grid = new Cell[rows + 1][];

            for (var x = 0; x < rows; x++)
            {
                grid[x] = new Cell[columns + 1];

                for (var y = 0; y < columns; y++)
                {
                    grid[x][y] = new Cell(x, y, width, height);
                }
            }
        }

        IEnumerable<Cell> Cells()
        {
            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    yield return grid[x][y];
                }
            }
        }

        public bool Collide(Func<EngineEntity, EngineEntity, bool> isCollision,  EngineEntity entity, BoxBounds box)
        {
            var startX = Math.Min(box.LowerLeft.x / (mapWidth / rows), rows - 1);
            var endX = Math.Min(box.LowerRight.x / (mapWidth / rows), rows - 1);

            var startY = Math.Min(box.LowerLeft.y / (mapHeight / columns), columns - 1);
            var endY = Math.Min(box.UpperLeft.y / (mapHeight / columns), columns - 1);

            var processed = new HashSet<(EngineEntity, EngineEntity)> {};

            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    var collision = grid[x][y].Collide(isCollision, processed, entity, box);

                    if (collision) return true;
                }
            }

            return false;
        }

        public LinkedList<Collision> Collisions(Func<EngineEntity, EngineEntity, bool> isCollision)
        {
            processedCollisions.Clear();

            var collisions = new LinkedList<Collision> { };

            for (var x = 0; x < rows; x++)
            {
                for (var y = 0; y < columns; y++)
                {
                    grid[x][y].Collisions(isCollision, processedCollisions, collisions);
                }
            }

            return collisions;
        }

        public void Insert(EngineEntity item)
        {
            var box = item.Engine.GetPooledEntityBounds(item);

            var startX = Math.Min(box.LowerLeft.x / (mapWidth / rows), rows - 1);
            var endX = Math.Min(box.LowerRight.x / (mapWidth / rows), rows - 1);

            var startY = Math.Min(box.LowerLeft.y / (mapHeight / columns), columns - 1);
            var endY = Math.Min(box.UpperLeft.y / (mapHeight / columns), columns - 1);

            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    var isIndexInvalid = 
                        x < 0 ||
                        grid.Length - 1 < x ||
                        y < 0 ||
                        grid[x].Length - 1 < y;

                    if (isIndexInvalid) {
                        continue;
                    }

                    grid[x][y].Insert(item);
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