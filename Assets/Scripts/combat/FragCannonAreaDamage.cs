using System;
using System.Collections.Generic;
using UnityEngine;
using Pandora;
using Pandora.Engine;

namespace Pandora.Combat
{
    public class FragCannonAreaDamage : MonoBehaviour, AreaDamage
    {
        EngineComponent engineComponent;
        AreaCombatBehaviour areaCombatBehaviour;
        public int SideDamage = 20;
        public int CentralDamage = 50;
        public int ConicSize = 500;

        void Awake()
        {
            engineComponent = GetComponent<EngineComponent>();
            areaCombatBehaviour = GetComponent<AreaCombatBehaviour>();
        }

        public Dictionary<GameObject, int> CalculateAreaDamages(Enemy target)
        {
            var engine = engineComponent.Engine;
            var entity = engineComponent.Entity;
            var attackRange = areaCombatBehaviour.AttackRangeEngineUnits;
            var damages = new Dictionary<GameObject, int>();

            var angle = engineComponent.Engine.GetAngleFromVectors(entity.Position, target.enemyEntity.Position);
            var snappedAngle = engineComponent.Engine.SnapAngleToMultiple(angle, 45);
            var direction = engineComponent.Engine.SnappedAngleToDirection(snappedAngle);

            var fullTriangleTargets = engine.FindInTriangularRange(entity, direction, ConicSize, attackRange, 0, true);
            var centralTriangleTargets = engine.FindInTriangularRange(entity, direction, ConicSize / 3, attackRange, 0, true);

            foreach (var nearTarget in fullTriangleTargets)
            {
                if (
                    nearTarget == entity ||
                    nearTarget.GameObject.GetComponent<TeamComponent>().Team == GetComponent<TeamComponent>().Team
                ) continue;

                // FIXME: This should be optimized
                var isCentral = centralTriangleTargets.Contains(nearTarget);

                var damage = isCentral ? CentralDamage : SideDamage;

                Logger.Debug($"[AREADAMAGE] Giving area damage of {damage}");

                damages.Add(nearTarget.GameObject, damage);
            }

            return damages;
        }
    }
}