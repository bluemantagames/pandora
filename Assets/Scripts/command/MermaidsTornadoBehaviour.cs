using Pandora.Engine;
using Pandora.Combat.Effects;
using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Command
{
    public class MermaidsTornadoBehaviour : MonoBehaviour, EngineBehaviour
    {
        public string ComponentName => "MermaidsTornadoBehaviour";

        public GameObject TornadoEffectObject;
        public int EngineUnitsRadius, EngineUnitsSpeed = 100, ChooseTargetTicksNum = 10, DurationMs = 2000;
        int ticksElapsed = 0;
        uint totalTimeElapsed = 0;
        List<EngineEntity> targets = new List<EngineEntity> { };
        bool isDisabled = false;
        float animationStartTime = 0.85f, endAnimationTime = 2.86f;
        ParticleSystem[] particles = null;
        bool particlesSetup = false;

        public void TickUpdate(uint timeLapsed)
        {
            if (isDisabled) return;

            var engineComponent = GetComponent<EngineComponent>();
            var teamComponent = GetComponent<TeamComponent>();

            ticksElapsed++;

            totalTimeElapsed += timeLapsed;

            var entity = engineComponent.Entity;
            var engine = engineComponent.Engine;

            transform.position = entity.GetWorldPosition();

            foreach (var target in engine.FindInRadius(entity.Position, EngineUnitsRadius, false))
            {
                if (
                    target == entity ||
                    !target.IsRigid ||
                    target.GameObject.GetComponent<TeamComponent>().Team == GetComponent<TeamComponent>().Team
                ) continue;

                var effect = TornadoEffectObject.GetComponent<MermaidsTornadoEffect>().Apply(gameObject, target.GameObject) as MermaidsTornadoEffect;

                effect.EngineUnitsRadius = EngineUnitsRadius;

                targets.Add(target);
            }

            if (ticksElapsed % ChooseTargetTicksNum == 0)
            {
                EngineEntity middleTower = null;

                foreach (var tower in MapComponent.Instance.gameObject.GetComponentsInChildren<TowerPositionComponent>())
                {
                    Logger.Debug($"Inspecting {tower.EngineTowerPosition.IsMiddle()} {tower.GetComponent<TowerTeamComponent>().EngineTeam} {teamComponent.Team}");

                    if (tower.EngineTowerPosition.IsMiddle() && tower.GetComponent<TowerTeamComponent>().EngineTeam == teamComponent.Team)
                    {
                        middleTower = tower.GetComponent<EngineComponent>().Entity;

                        break;
                    }
                }

                if (middleTower == null)
                {
                    Logger.DebugWarning("Could not find middle tower");

                    return;
                }

                var target = engine.FindClosest(middleTower.Position, foundEntity =>
                    !foundEntity.IsStructure &&
                    !foundEntity.IsMapObstacle &&
                    foundEntity.GameObject.GetComponent<TeamComponent>()?.Team != teamComponent.Team &&
                    foundEntity.GameObject.GetComponent<MermaidsTornadoEffect>() == null
                );

                if (target != null)
                {
                    entity.SetTarget(target);
                    entity.SetSpeed(EngineUnitsSpeed);
                }
            }

            if (totalTimeElapsed > DurationMs)
            {
                isDisabled = true;

                foreach (var target in targets)
                {
                    target.GameObject.GetComponent<MermaidsTornadoEffect>()?.Unapply(target.GameObject);
                }

                engine.RemoveEntity(entity);

                GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        ParticleSystem[] findParticles()
        {
            if (particles == null)
            {
                particles = GetComponentsInChildren<ParticleSystem>();
            }

            return particles;
        }

        void Update() {
            if (particlesSetup) {
                return;
            }

            float animationPercent = (float)totalTimeElapsed / DurationMs;

            foreach (var particle in findParticles()) {
                particle.Pause();

                particle.time = animationStartTime + (animationPercent * (endAnimationTime - animationStartTime));

                Debug.Log($"Particle time {particle.time}, animationPercent {animationPercent}");
            }
        }
    }

}