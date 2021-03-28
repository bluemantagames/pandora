using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;
using Pandora.Pool;

namespace Pandora
{
    public class ProjectilePositionFixer : MonoBehaviour
    {
        public int PivotAdjustmentX = 0;
        public int PivotAdjustmentY = 0;
        public int ProjectileAdjustmentX = 0;
        public int ProjectileAdjustmentY = 0;

        int projectileDirectionThreshold = 1;

        public Vector2Int CalculateProjectilePosition(Vector2Int basePosition, PandoraEngine engine, Vector2Int direction)
        {
            var computedBasePosition = PoolInstances.Vector2IntPool.GetObject();
            computedBasePosition.x = basePosition.x + PivotAdjustmentX;
            computedBasePosition.y = basePosition.y + PivotAdjustmentY;

            var computedPosition = PoolInstances.Vector2IntPool.GetObject();
            computedPosition.x = computedBasePosition.x + ProjectileAdjustmentX;
            computedPosition.y = computedBasePosition.y + ProjectileAdjustmentY;

            var rotatedPosition = engine.RotateFigureByDirection(new List<Vector2Int>() { computedPosition }, computedBasePosition, direction)[0];

            PoolInstances.Vector2IntPool.ReturnObject(computedBasePosition);
            PoolInstances.Vector2IntPool.ReturnObject(computedPosition);

            return rotatedPosition;
        }

        public Vector2Int CalculateDirection(EngineEntity unitEntity, EngineEntity enemyEntity)
        {
            var rawDirection = enemyEntity.GetCurrentCell().vector - unitEntity.GetCurrentCell().vector;

            var direction = PoolInstances.Vector2IntPool.GetObject();
            direction.x = rawDirection.x > projectileDirectionThreshold ? 1 : rawDirection.x < -projectileDirectionThreshold ? -1 : 0;
            direction.y = rawDirection.y > projectileDirectionThreshold ? 1 : rawDirection.y < -projectileDirectionThreshold ? -1 : 0;

            Logger.Debug($"Calculated direction for projectiles ({direction.x}, {direction.y}) using the gridcell ({rawDirection.x}, {rawDirection.y})");

            return direction;
        }
    }
}