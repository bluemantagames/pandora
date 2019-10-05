using UnityEngine;
using Pandora.Engine;

namespace Pandora.Command
{
    /// <summary>On double tap, the Troll will damage all the units in a "triangle" area</summary>
    public class TrollCommand : MonoBehaviour, CommandBehaviour
    {
        public int Width = 4000;
        public int Height = 3000;
        public int Damage = 50;

        public void InvokeCommand()
        {
            Debug.Log("[Troll] Command invoked");

            var source = GetComponent<EngineComponent>().Entity;
            var sourceTeam = GetComponent<TeamComponent>().team;
            var sourceAnimator = GetComponent<Animator>();

            // Execute the attack animation
            sourceAnimator.Play("GiantAttack");

            foreach (var targetLifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var target = targetLifeComponent.gameObject.GetComponent<EngineComponent>().Entity;
                var targetTeam = targetLifeComponent.gameObject.GetComponent<TeamComponent>().team;

                if (sourceTeam == targetTeam) continue;

                if (source.Engine.IsInTriangularRange(source, target, Width, Height))
                {
                    Debug.Log("[Troll] Unit damaged");
                    targetLifeComponent.AssignDamage(Damage);
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