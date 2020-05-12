using UnityEngine;
using Pandora.Combat;
using Pandora.Engine;

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
                if (entity.GameObject.GetComponent<TeamComponent>()?.team == team.team || !entity.IsRigid) continue;

                var lifeValue = entity.GameObject.GetComponent<LifeComponent>().lifeValue;

                var isSideValid = 
                    IsTopSide(entity.GetCurrentCell()) ? isTopValid : isBottomValid;

                if ((minLife == null || minLife > lifeValue) && isSideValid) {
                    minLife = lifeValue;
                    targetEntity = entity;
                }
            }

            var isEveryoneAlive = groupComponent.Objects.TrueForAll(unit => !unit.GetComponent<LifeComponent>().IsDead);

            if (isEveryoneAlive && targetEntity != null) {
                targetEntity.GameObject.GetComponent<TeamComponent>().Convert(team.team);

                foreach (var cleric in groupComponent.Objects) {
                    var lifeComponent = cleric.GetComponent<LifeComponent>();

                    lifeComponent.AssignDamage(lifeComponent.lifeValue);
                }
            } else {
                Logger.DebugWarning("Somebody is dead or no valid targets, cannot convert");
            }
        }

        private bool IsTopSide(GridCell cell) => cell.vector.y >= MapComponent.Instance.bottomMapSizeY + 1;
    }
}