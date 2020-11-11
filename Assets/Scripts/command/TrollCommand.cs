using UnityEngine;
using Pandora.Engine;
using Pandora.Combat;
using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command
{
    /// <summary>On double tap, the Troll will damage all the units in a "triangle" area</summary>
    public class TrollCommand : MonoBehaviour, CommandBehaviour
    {
        public int Width = 4000;
        public int Height = 3000;
        public int Damage = 50;
        public int UnitsLeniency = 20;
        public bool DebugLines = false;
        public GameObject[] EffectObjects;
        public GameObject TriangleIndicator;

        EngineEntity sourceEntity;

        void Start() {
            sourceEntity = GetComponent<EngineComponent>().Entity;
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

            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var sourceTeam = GetComponent<TeamComponent>().Team;
            var sourceAnimator = GetComponent<Animator>();

            // Execute the attack animation
            sourceAnimator.Play("GiantAttack");

            foreach (var targetLifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var targetGameObject = targetLifeComponent.gameObject;
                var targetEntity = targetGameObject.GetComponent<EngineComponent>().Entity;
                var targetTeam = targetGameObject.GetComponent<TeamComponent>().Team;

                if (sourceTeam == targetTeam) continue;

                if (sourceEntity.Engine.IsInConicRange(sourceEntity, targetEntity, Width, Height, UnitsLeniency, DebugLines))
                {
                    // Assign damage
                    targetLifeComponent.AssignDamage(Damage, new UnitCommand(gameObject));

                    // Assign effects to non-structure units
                    if (!targetEntity.IsStructure && targetEntity.GameObject.layer != Constants.SWIMMING_LAYER) {
                        foreach (var effectObject in EffectObjects)
                        {
                            var effect = effectObject.GetComponent<Effect>();

                            effect.Apply(gameObject, targetGameObject);
                        }
                    }
                }
            }
        }

    }
}