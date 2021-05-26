using UnityEngine;
using Pandora.Engine;
using Pandora.Combat;
using Pandora.UI;
using System.Collections.Generic;

namespace Pandora.Command
{
    /// <summary>On double tap, the Bard will cure, give attack damage and speed</summary>
    public class BardCommand : MonoBehaviour, CommandBehaviour
    {
        public int Radius = 3000;
        public bool DebugLines = false;
        public GameObject[] EffectObjects;
        public GameObject VFXObject;
        List<EngineEntity> targets = new List<EngineEntity>(300);
        List<EffectIndicator> commandIndicators = new List<EffectIndicator>(300);
        TeamComponent sourceTeam;
        EngineEntity sourceEntity;

        List<EngineEntity> FindBuffTargets()
        {
            targets.Clear();

            var entities = MapComponent.Instance.engine.Entities;

            foreach (var targetEntity in entities)
            {
                if (targetEntity.GameObject.GetComponent<LifeComponent>() == null) continue;

                var teamComponent = targetEntity.GameObject.GetComponent<TeamComponent>();

                if (teamComponent == null) continue;

                var targetTeam = teamComponent.Team;

                if (sourceTeam.Team != targetTeam) continue;

                if (targetEntity.Engine.IsInCircularRange(sourceEntity, targetEntity, Radius, DebugLines))
                {
                    targets.Add(targetEntity);
                }
            }

            return targets;
        }

        void Start()
        {
            sourceTeam = GetComponent<TeamComponent>();
            sourceEntity = gameObject.GetComponent<EngineComponent>().Entity;
        }

        public List<EffectIndicator> FindTargets()
        {
            commandIndicators.Clear();

            commandIndicators.Add(
                new EntitiesIndicator(FindBuffTargets())
            );

            commandIndicators.Add(
                new FollowingCircleRangeIndicator(Radius, gameObject)
            );

            return commandIndicators;
        }

        public void InvokeCommand()
        {
            Logger.Debug("[Bard] Command invoked");

            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var sourceAnimator = GetComponent<Animator>();

            GetComponent<ArenaEntityBehaviour>().PlayAnimation("Command", 1000, null);

            var vfx = Instantiate(VFXObject, transform.position, VFXObject.transform.rotation, transform);

            vfx.GetComponent<ParticleSystem>().Play();

            var targets = FindBuffTargets();

            foreach (var targetEntity in targets)
            {
                var targetGameObject = targetEntity.GameObject;

                foreach (var effectObject in EffectObjects)
                {
                    var effect = effectObject.GetComponent<Effect>();

                    effect.Apply(gameObject, targetGameObject);
                }
            }
        }
    }
}