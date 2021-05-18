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

        void Awake()
        {
            engineComponent = GetComponent<EngineComponent>();
            areaCombatBehaviour = GetComponent<AreaCombatBehaviour>();
        }

        public Dictionary<GameObject, int> CalculateAreaDamages(Enemy target)
        {
            var engine = engineComponent.Engine;
            var entity = engineComponent.Entity;
            var damages = new Dictionary<GameObject, int>();
            var angle = engineComponent.Engine.GetAngleFromVectors(entity.Position, target.enemyEntity.Position);
            var snappedAngle = engineComponent.Engine.SnapAngleToMultiple(angle, 45);
            var direction = engineComponent.Engine.SnappedAngleToDirection(snappedAngle);

            foreach (var nearTarget in engine.FindInTriangularRange(entity, direction, 500, areaCombatBehaviour.AttackRangeEngineUnits, 0, true))
            {
                if (
                    nearTarget == entity ||
                    nearTarget.GameObject.GetComponent<TeamComponent>().Team == GetComponent<TeamComponent>().Team
                ) continue;

                damages.Add(nearTarget.GameObject, areaCombatBehaviour.Damage);
            }

            return damages;
        }
    }
}