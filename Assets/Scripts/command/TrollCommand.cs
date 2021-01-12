using UnityEngine;
using Pandora.Engine;
using Pandora.Combat;
using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command
{
    /// <summary>On double tap, the Troll will damage all the units in a "triangle" area</summary>
    public class TrollCommand : MonoBehaviour, CommandBehaviour, EngineBehaviour
    {
        public int Width = 4000;
        public int Height = 3000;
        public int Damage = 50;
        public int UnitsLeniency = 20;
        public bool DebugLines = false;
        public GameObject[] EffectObjects;
        public GameObject TriangleIndicator;
        List<EngineEntity> affectedEntities = new List<EngineEntity>(100);

        bool commandTrigger = false, commandExecuted = false;
        float timePassed = 0f, animationTotalTime = 0.5f; // seconds

        EngineEntity sourceEntity;

        public string ComponentName => "TrollCommand";

        Animator sourceAnimator;

        void Start()
        {
            sourceEntity = GetComponent<EngineComponent>().Entity;
            sourceAnimator = GetComponent<Animator>();

            sourceAnimator.speed = 0;
        }

        public List<EffectIndicator> FindTargets()
        {
            var engine = sourceEntity.Engine;

            var (v1, v2, v3) = engine.CalculateRotatedTriangleVertices(
                sourceEntity.Position, Width, Height, UnitsLeniency, sourceEntity.Direction
            );

            var triangle = Instantiate(TriangleIndicator, Vector3.zero, Quaternion.identity, sourceEntity.GameObject.transform);

            triangle.GetComponent<TriangleRenderer>().Initialize(
                engine.PhysicsToMapWorld(v1),
                engine.PhysicsToMapWorld(v2),
                engine.PhysicsToMapWorld(v3)
            );

            return new List<EffectIndicator> {
                new GameObjectIndicator(triangle)
            };
        }

        public void InvokeCommand()
        {
            Logger.Debug("[Troll] Command invoked");

            var sourceTeam = GetComponent<TeamComponent>().Team;

            commandTrigger = true;

            sourceEntity.IsMovementPaused = true;

            GetComponent<UnitBehaviour>().PauseBehaviour();

            foreach (var entity in sourceEntity.Engine.Entities)
            {
                var targetGameObject = entity.GameObject;
                var targetLifeComponent = targetGameObject.GetComponent<LifeComponent>();

                if (targetLifeComponent == null) continue;

                var targetTeam = targetGameObject.GetComponent<TeamComponent>().Team;

                if (sourceTeam == targetTeam) continue;

                if (sourceEntity.Engine.IsInConicRange(sourceEntity, entity, Width, Height, UnitsLeniency, DebugLines))
                {
                    affectedEntities.Add(entity);
                }
            }
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (commandTrigger)
            {
                timePassed += (float)timeLapsed / 1000f;

                var percent = timePassed / animationTotalTime;

                // Execute the attack animation
                sourceAnimator.Play("Command", 0, percent);

                if (percent >= 0.5f && !commandExecuted)
                {
                    foreach (var entity in affectedEntities)
                    {
                        // Assign damage
                        entity.GameObject.GetComponent<LifeComponent>().AssignDamage(Damage, new UnitCommand(gameObject));

                        // Assign effects to non-structure units
                        if (!entity.IsStructure && entity.GameObject.layer != Constants.SWIMMING_LAYER)
                        {
                            foreach (var effectObject in EffectObjects)
                            {
                                var effect = effectObject.GetComponent<Effect>();

                                effect.Apply(gameObject, entity.GameObject);
                            }
                        }
                    }

                    commandExecuted = true;
                }

                if (percent >= 1f)
                {
                    commandTrigger = false;
                    sourceEntity.IsMovementPaused = false;

                    GetComponent<UnitBehaviour>().UnpauseBehaviour();
                }
            }
        }
    }
}