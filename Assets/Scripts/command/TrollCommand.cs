using UnityEngine;
using Pandora.Engine;

namespace Pandora.Command
{
    /// <summary>On double tap, the Troll will damage all the units in a "triangle" area</summary>
    public class TrollCommand : MonoBehaviour, CommandBehaviour
    {
        public int width = 4000;
        public int height = 3000;
        public int damage = 50;

        public void InvokeCommand()
        {
            Debug.Log("[Troll] Command invoked");

            var source = GetComponent<EngineComponent>().Entity;
            var sourceTeam = GetComponent<TeamComponent>().team;

            foreach (var targetLifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var target = targetLifeComponent.gameObject.GetComponent<EngineComponent>().Entity;
                var targetTeam = targetLifeComponent.gameObject.GetComponent<TeamComponent>().team;

                if (sourceTeam == targetTeam) continue;

                if (source.Engine.IsInTriangularRange(source, target, width, height))
                {
                    Debug.Log("[Troll] Unit damaged");
                    targetLifeComponent.AssignDamage(damage);
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