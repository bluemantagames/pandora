using UnityEngine;
using Pandora.Engine;
using Pandora.Combat;

namespace Pandora.Command
{
    /// <summary>On double tap, the Troll will damage all the units in a "triangle" area</summary>
    public class TrollCommand : MonoBehaviour, CommandBehaviour
    {
        public int Width = 4000;
        public int Height = 3000;
        public int Damage = 50;
        public bool DebugLines = false;
        public GameObject[] EffectObjects;

        public void InvokeCommand()
        {
            Logger.Debug("[Troll] Command invoked");

            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var sourceTeam = GetComponent<TeamComponent>().team;
            var sourceAnimator = GetComponent<Animator>();

            // Execute the attack animation
            sourceAnimator.Play("GiantAttack");

            foreach (var targetLifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var targetGameObject = targetLifeComponent.gameObject;
                var targetEntity = targetGameObject.GetComponent<EngineComponent>().Entity;
                var targetTeam = targetGameObject.GetComponent<TeamComponent>().team;

                if (sourceTeam == targetTeam) continue;

                if (sourceEntity.Engine.IsInConicRange(sourceEntity, targetEntity, Width, Height, 20, DebugLines))
                {
                    // Assign damage
                    targetLifeComponent.AssignDamage(Damage);

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

        void Awake()
        {
        }

        void Update()
        {
        }

    }
}