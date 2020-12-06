using UnityEngine;
using Pandora.Combat;
using Pandora.Engine;
using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command
{
    public class ClericCommand : MonoBehaviour, CommandBehaviour
    {
        public int EngineUnitsRange = 1200;

        TeamComponent team;
        EngineComponent engineComponent;
        GroupComponent groupComponent;

        void Start() {
            team = GetComponent<TeamComponent>();
            engineComponent = GetComponent<EngineComponent>();
            groupComponent = GetComponent<GroupComponent>();
        }

        public void InvokeCommand()
        {
            EngineEntity targetEntity = FindConverted();

            var isEveryoneAlive = groupComponent.Objects.TrueForAll(unit => !unit.GetComponent<LifeComponent>().IsDead);

            if (isEveryoneAlive && targetEntity != null) {
                targetEntity.GameObject.GetComponent<TeamComponent>().Convert(team.Team);

                foreach (var cleric in groupComponent.Objects) {
                    var lifeComponent = cleric.GetComponent<LifeComponent>();

                    lifeComponent.AssignDamage(lifeComponent.lifeValue, new UnitCommand(gameObject));
                }
            } else {
                Logger.DebugWarning("Somebody is dead or no valid targets, cannot convert");
            }
        }

        private bool IsTopSide(GridCell cell) => cell.vector.y >= MapComponent.Instance.bottomMapSizeY + 1;

        private EngineEntity FindConverted() {
            int? minLife = null;
            EngineEntity targetEntity = null;

            var isTopValid = groupComponent.Objects.Exists((cleric) => 
                IsTopSide(cleric.GetComponent<EngineComponent>().Entity.GetCurrentCell())
            );

            var isBottomValid = groupComponent.Objects.Exists((cleric) => 
                !IsTopSide(cleric.GetComponent<EngineComponent>().Entity.GetCurrentCell())
            );

            foreach (var entity in engineComponent.Entity.FindInHitboxRange(EngineUnitsRange, false))
            {
                if (entity.GameObject.GetComponent<TeamComponent>()?.Team == team.Team || !entity.IsRigid) continue;

                var lifeValue = entity.GameObject.GetComponent<LifeComponent>().lifeValue;

                var isSideValid = 
                    IsTopSide(entity.GetCurrentCell()) ? isTopValid : isBottomValid;

                if ((minLife == null || minLife > lifeValue) && isSideValid) {
                    minLife = lifeValue;
                    targetEntity = entity;
                }
            }

            return targetEntity;
        }

        public List<EffectIndicator> FindTargets()
        {
            var target = FindConverted();

            if (target != null)
                return new List<EffectIndicator> {
                    new EntitiesIndicator(
                        new List<EngineEntity> { FindConverted() }
                    )
                };
            else
                return new List<EffectIndicator> {};
        }
    }
}