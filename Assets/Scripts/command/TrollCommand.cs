﻿using UnityEngine;
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
        public GameObject[] EffectObjects;

        public void InvokeCommand()
        {
            Debug.Log("[Troll] Command invoked");

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

                if (sourceEntity.Engine.IsInConicRange(sourceEntity, targetEntity, Width, Height, 20))
                {
                    // Assign damage
                    targetLifeComponent.AssignDamage(Damage);

                    // Assign effects
                    foreach (var effectObject in EffectObjects)
                    {
                        var effect = effectObject.GetComponent<Effect>();

                        effect.Apply(gameObject, targetGameObject);
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