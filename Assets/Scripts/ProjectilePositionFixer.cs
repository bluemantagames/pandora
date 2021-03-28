using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;

namespace Pandora
{
    public class ProjectilePositionFixer : MonoBehaviour
    {
        public int ProjectileAdjustmentX = 0;
        public int ProjectileAdjustmentY = 0;

        int projectileDirectionThreshold = 1;

        public Vector2Int CalculateProjectilePosition(EngineEntity unitEntity, PandoraEngine engine, Vector2Int direction)
        {
            var basePosition = unitEntity.Position;
            var computedPosition = new Vector2Int(basePosition.x + ProjectileAdjustmentX, basePosition.y + ProjectileAdjustmentY);
            var rotatedPosition = engine.RotateFigureByDirection(new List<Vector2Int>() { computedPosition }, basePosition, direction)[0];

            return rotatedPosition;
        }

        public Vector2Int CalculateDirection(EngineEntity unitEntity, EngineEntity enemyEntity)
        {
            var rawDirection = enemyEntity.GetCurrentCell().vector - unitEntity.GetCurrentCell().vector;

            var direction = new Vector2Int(
                rawDirection.x > projectileDirectionThreshold ? 1 : rawDirection.x < -projectileDirectionThreshold ? -1 : 0,
                rawDirection.y > projectileDirectionThreshold ? 1 : rawDirection.y < -projectileDirectionThreshold ? -1 : 0
            );

            Logger.Debug($"Calculated direction for projectiles ({direction.x}, {direction.y}) using the gridcell ({rawDirection.x}, {rawDirection.y})");


            return direction;
        }
    }
}