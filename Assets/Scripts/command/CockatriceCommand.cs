using UnityEngine;
using System.Linq;
using Pandora.Combat;
using Pandora.Engine;
using Pandora.Movement;

namespace Pandora.Command
{
    /// <summary>
    /// On command, Cockatrice starts flying and deals huge amounts of damage to the nearest enemy.
    ///
    /// It keeps flying for X seconds
    /// </summary>
    public class CockatriceCommand : MonoBehaviour, CommandBehaviour
    {
        public int FlyingTimeMs = 2000, EngineUnitsRange = 1200;
        public bool TargetBuildings = false;

        TeamComponent team;
        EngineComponent engineComponent;

        void Start() {
            team = GetComponent<TeamComponent>();
            engineComponent = GetComponent<EngineComponent>();
        }

        public void InvokeCommand()
        {
            var cockatrice = engineComponent.Entity;

            var flyingMode = gameObject.AddComponent<CockatriceFlyingMode>();

            flyingMode.FlyingTimeMs = FlyingTimeMs;
            gameObject.layer = Constants.FLYING_LAYER;

            gameObject.GetComponent<EngineComponent>().RefreshComponents();

            int? lowestDistance = null;
            EngineEntity target = null;

            foreach (var entity in engineComponent.Entity.FindInHitboxRange(EngineUnitsRange, TargetBuildings))
            {
                if (entity.GameObject.GetComponent<TeamComponent>().team == team.team) continue;

                var distance = engineComponent.Engine.SquaredDistance(entity.Position, cockatrice.Position);

                if (lowestDistance == null || lowestDistance > distance)
                {
                    lowestDistance = distance;
                    target = entity;
                }
            }

            if (target != null) {
                var lifeComponent = target.GameObject.GetComponent<LifeComponent>();
                var damage = (lifeComponent.maxLife / 10) * 9;

                lifeComponent.AssignDamage(damage);
            }
        }
    }
}
