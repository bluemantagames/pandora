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
        public int SpecularPivotAdjustmentX = 0;
        public int SpecularPivotAdjustmentY = 0;
        public int ProjectileAdjustmentX = 0;
        public int ProjectileAdjustmentY = 0;

        int projectileDirectionThreshold = 1;

        public Vector2Int CalculateProjectilePosition(
            int pivotAdjX,
            int pivotAdjY,
            int adjX,
            int adjY,
            Vector2Int basePosition,
            PandoraEngine engine,
            Vector2Int direction
        )
        {
            var computedBasePosition = PoolInstances.Vector2IntPool.GetObject();
            computedBasePosition.x = basePosition.x + pivotAdjX;
            computedBasePosition.y = basePosition.y + pivotAdjY;

            var computedPosition = PoolInstances.Vector2IntPool.GetObject();
            computedPosition.x = computedBasePosition.x + adjX;
            computedPosition.y = computedBasePosition.y + adjY;

            var figure = PoolInstances.Vector2IntListPool.GetObject();
            figure.Add(computedPosition);

            var rotatedPosition = engine.RotateFigureByDirection(figure, computedBasePosition, direction)[0];

            PoolInstances.Vector2IntPool.ReturnObject(computedBasePosition);
            PoolInstances.Vector2IntPool.ReturnObject(computedPosition);
            PoolInstances.Vector2IntListPool.ReturnObject(figure);

            return rotatedPosition;
        }

        public Vector2Int CalculateProjectilePosition(Vector2Int basePosition, PandoraEngine engine, Vector2Int direction)
        {
            return CalculateProjectilePosition(
                PivotAdjustmentX,
                PivotAdjustmentY,
                ProjectileAdjustmentX,
                ProjectileAdjustmentY,
                basePosition,
                engine,
                direction
            );
        }

        public Vector2Int CalculateTowerProjectilePosition(Vector2Int basePosition, PandoraEngine engine, Vector2Int direction)
        {
            var isSpecular = TeamComponent.assignedTeam == 2;
            var computedPivotAdjustmentX = PivotAdjustmentX;
            var computedPivotAdjustmentY = isSpecular ? -PivotAdjustmentY : PivotAdjustmentY;

            if (isSpecular)
            {
                computedPivotAdjustmentX += SpecularPivotAdjustmentX;
                computedPivotAdjustmentY -= SpecularPivotAdjustmentY;
            }

            return CalculateProjectilePosition(
                computedPivotAdjustmentX,
                computedPivotAdjustmentY,
                ProjectileAdjustmentX,
                ProjectileAdjustmentY,
                basePosition,
                engine,
                direction
            );
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