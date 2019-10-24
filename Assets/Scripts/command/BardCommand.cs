using UnityEngine;
using Pandora.Engine;
using Pandora.Combat;

namespace Pandora.Command
{
    /// <summary>On double tap, the Troll will damage all the units in a "triangle" area</summary>
    public class BardCommand : MonoBehaviour, CommandBehaviour
    {
        public int Radius = 3000;
        public bool DebugLines = false;
        public GameObject[] EffectObjects;

        public void InvokeCommand()
        {
            Debug.Log("[Bard] Command invoked");

            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var sourceTeam = GetComponent<TeamComponent>().team;
            var sourceAnimator = GetComponent<Animator>();

            // Execute the attack animation
            sourceAnimator.Play("BardAttack");

            foreach (var targetLifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var targetGameObject = targetLifeComponent.gameObject;
                var targetEntity = targetGameObject.GetComponent<EngineComponent>().Entity;
                var targetTeam = targetGameObject.GetComponent<TeamComponent>().team;

                if (sourceTeam == targetTeam) continue;

                if (sourceEntity.Engine.IsInCircularRange(sourceEntity, targetEntity, Radius, DebugLines))
                {
                    // ...
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