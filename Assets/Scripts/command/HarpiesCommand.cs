using UnityEngine;
using System.Linq;
using Pandora.Combat;
using Pandora.Engine;
using Pandora.Movement;

namespace Pandora.Command
{
    /// <summary>On double tap, harpies rally on the nearest enemy with the highest amount of hitpoints</summary>
    public class HarpiesCommand : MonoBehaviour, CommandBehaviour
    {
        public Color SelectedColor = Color.yellow;
        Color originalColor;

        /// <summary>The max radius in which to search for a target</summary>
        public int SearchRangeEngineUnits = 1200;

        SpriteRenderer rangerRenderer;
        MeleeCombatBehaviour combatBehaviour;

        public void InvokeCommand()
        {
            Enemy target = null;
            int? hp = null;

            var entities =
                from harpy in transform.parent.GetComponent<GroupComponent>().Objects
                select harpy;

            var positions = entities.Select(harpy => harpy.GetComponent<EngineComponent>());

            Debug.Log("harpy command invoked");

            var teamComponent = transform.parent.GetComponent<TeamComponent>();

            foreach (var lifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var targetEntity = lifeComponent.gameObject.GetComponent<EngineComponent>().Entity;
                var isInRange = positions.Any(harpyCell => harpyCell.Engine.IsInRange(harpyCell.Entity, targetEntity, SearchRangeEngineUnits));
                var targetTeamComponent = lifeComponent.gameObject.GetComponent<TeamComponent>();

                if (!isInRange) continue;

                if ((hp == null || hp.Value < lifeComponent.lifeValue) && teamComponent.team != targetTeamComponent.team) {
                    hp = Mathf.FloorToInt(lifeComponent.lifeValue);

                    target = new Enemy(lifeComponent.gameObject);
                }
            }

            Debug.Log($"harpy command invoked {target}");

            if (target == null) {
                Debug.LogWarning("Could not find a target for harpies command");
            } else {
                Debug.Log($"Harpies attacking {target}");

                foreach (var harpy in entities) {
                    harpy.GetComponent<MovementComponent>().Target = target;
                }
            }
        }

        void Awake()
        {
            combatBehaviour = GetComponentInParent<MeleeCombatBehaviour>();
            rangerRenderer = GetComponentInParent<SpriteRenderer>();
            originalColor = rangerRenderer.color;
        }

        void Update()
        {
            if (rangerRenderer.color != SelectedColor && combatBehaviour.NextAttackMultiplier.HasValue)
            {
                rangerRenderer.color = SelectedColor;
            }

            if (rangerRenderer.color == SelectedColor && !combatBehaviour.NextAttackMultiplier.HasValue)
            {
                rangerRenderer.color = originalColor;
            }
        }

    }
}