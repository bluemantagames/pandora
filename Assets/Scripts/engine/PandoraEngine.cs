using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Engine
{
    public class PandoraEngine
    {
        int tick = 10; // milliseconds, minimum tick
        int unitsPerCell = 100; // physics engine units per grid cell
        List<EngineEntity> entities;
        public MapComponent Map;

        // Start with a reasonable capacity already
        Dictionary<Vector2Int, List<EngineEntity>> positions =
            new Dictionary<Vector2Int, List<EngineEntity>>(1000);

        void NextTick()
        {
            foreach (var entity in entities)
            {
                var unitsMoved = Mathf.FloorToInt(Mathf.Max(1f, entity.Speed * (tick / 1000f)));

                entity.Position += entity.Direction * unitsMoved;

                IndexPositions(entity);
            }

            positions.Clear();
        }

        // converts a world point to a physics engine point using linear interpolation 
        // TODO: this is done only to let people use the collider tool to define boundaries
        // if this turns up to create problems we have to define int boundaries in EngineEntity
        Vector2Int WorldToPhysics(Vector2 world)
        {
            var xWorldBounds = Map.cellWidth * Map.mapSizeX;
            var yWorldBounds = Map.cellHeight * Map.mapSizeY;

            var xPhysicsBounds = unitsPerCell * Map.mapSizeX;
            var yPhysicsBounds = unitsPerCell * Map.mapSizeY;

            return new Vector2Int(
                Mathf.RoundToInt((xPhysicsBounds * world.x) / xWorldBounds),
                Mathf.RoundToInt((yPhysicsBounds * world.y) / yWorldBounds)
            );
        }

        void IndexPositions(EngineEntity entity)
        {
            var worldBounds = entity.GameObject.GetComponent<BoxCollider2D>().bounds;

            var worldUpperLeftBounds = worldBounds.center;

            worldUpperLeftBounds.x -= worldBounds.extents.x / 2;
            worldUpperLeftBounds.y += worldBounds.extents.y / 2;

            var worldLowerRightBounds = worldBounds.center;

            worldLowerRightBounds.x += worldBounds.extents.x / 2;
            worldLowerRightBounds.y -= worldBounds.extents.y / 2;

            var physicsUpperLeftBounds = WorldToPhysics(worldUpperLeftBounds);
            var physicsLowerRightBounds = WorldToPhysics(worldLowerRightBounds);

            for (var x = physicsUpperLeftBounds.x; x <= physicsLowerRightBounds.x; x++)
            {
                for (var y = physicsUpperLeftBounds.y; y >= physicsUpperLeftBounds.y; y--)
                {
                    var position = new Vector2Int(x, y);

                    if (positions.ContainsKey(position))
                    {
                        positions[position].Add(entity);
                    }
                    else
                    {
                        positions.Add(position, new List<EngineEntity> { entity });
                    }
                }
            }

        }
    }

}