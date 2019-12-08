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
        float? coloredTimePassed = null;
        public float BlinkTime = 1f;
        Enemy target = null;

        public void InvokeCommand()
        {
            int? hp = null;

            var entities =
                from harpy in GetComponent<GroupComponent>().Objects
                select harpy;

            var positions = entities.Select(harpy => harpy.GetComponent<EngineComponent>());

            Logger.Debug("harpy command invoked");

            var teamComponent = GetComponent<TeamComponent>();

            foreach (var lifeComponent in MapComponent.Instance.gameObject.GetComponentsInChildren<LifeComponent>())
            {
                var targetEntity = lifeComponent.gameObject.GetComponent<EngineComponent>().Entity;
                var isInRange = positions.Any(harpyCell => harpyCell.Engine.IsInHitboxRange(harpyCell.Entity, targetEntity, SearchRangeEngineUnits));
                var targetTeamComponent = lifeComponent.gameObject.GetComponent<TeamComponent>();

                if (!isInRange) continue;

                if ((hp == null || hp.Value < lifeComponent.lifeValue) && teamComponent.team != targetTeamComponent.team) {
                    hp = Mathf.FloorToInt(lifeComponent.lifeValue);

                    target = new Enemy(lifeComponent.gameObject);
                }
            }

            Logger.Debug($"harpy command invoked {target}");

            if (target == null) {
                Logger.DebugWarning("Could not find a target for harpies command");
            } else {
                Logger.Debug($"Harpies attacking {target}");

                foreach (var harpy in entities) {
                    harpy.GetComponent<MovementComponent>().Target = target;
                }

                originalColor = target.enemy.GetComponent<SpriteRenderer>().color;
                target.enemy.GetComponent<SpriteRenderer>().color = SelectedColor;
                coloredTimePassed = 0f;
            }
        }

        void Awake()
        {
        }

        void Update()
        {
            if (coloredTimePassed.HasValue) {
                coloredTimePassed += Time.deltaTime;

                if (coloredTimePassed >= BlinkTime)  {
                    target.enemy.GetComponent<SpriteRenderer>().color = originalColor;
                    
                    coloredTimePassed = null;
                }

            }

        }

    }
}