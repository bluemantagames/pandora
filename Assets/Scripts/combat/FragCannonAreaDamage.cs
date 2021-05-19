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
            var centralTriangleTargets = engine.FindInTriangularRange(fullTriangleTargets, entity, direction, ConicSize / 3, attackRange, 0, true);

            foreach (var nearTarget in fullTriangleTargets)
            {
                if (!IsEntityDamageable(nearTarget)) continue;

                damages.Add(nearTarget.GameObject, SideDamage);
            }

            foreach (var nearTarget in centralTriangleTargets)
            {
                if (!IsEntityDamageable(nearTarget)) continue;

                damages[nearTarget.GameObject] = CentralDamage;
            }

            return damages;
        }

        bool IsEntityDamageable(EngineEntity target)
        {
            var entity = engineComponent.Entity;
            var targetTeam = target.GameObject.GetComponent<TeamComponent>().Team;
            var sourceTeam = GetComponent<TeamComponent>().Team;

            return target != entity && targetTeam != sourceTeam;
        }
    }
}