using UnityEngine;
using System.Linq;
using Pandora.Combat;
using Pandora.Engine;
using Pandora.AI;
using System.Collections.Generic;
using Pandora.UI;

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
        TeamComponent teamComponent;

        void Start()
        {
            teamComponent = GetComponent<TeamComponent>();
        }

        public void InvokeCommand()
        {
            var entities =
                from harpy in GetComponent<GroupComponent>().Objects
                select harpy;

            Logger.Debug("harpy command invoked");

            target = findTarget();

            Logger.Debug($"harpy command invoked {target}");

            if (target == null)
            {
                Logger.DebugWarning("Could not find a target for harpies command");
            }
            else
            {
                Logger.Debug($"Harpies attacking {target}");

                foreach (var harpy in entities)
                {
                    harpy.GetComponent<BasicEntityController>().Target = target;
                }

                var spriteRenderer = target.enemy.GetComponent<SpriteRenderer>();

                if (spriteRenderer != null)
                {
                    originalColor = spriteRenderer.color;
                    spriteRenderer.color = SelectedColor;
                    coloredTimePassed = 0f;
                }
            }
        }

        void Update()
        {
            if (coloredTimePassed.HasValue)
            {
                coloredTimePassed += Time.deltaTime;

                if (coloredTimePassed >= BlinkTime)
                {
                    target.enemy.GetComponent<SpriteRenderer>().color = originalColor;

                    coloredTimePassed = null;
                }
            }
        }

        Enemy findTarget()
        {
            Enemy target = null;

            int? hp = null;

            var entities =
                from harpy in GetComponent<GroupComponent>().Objects
                select harpy;

            var harpyEntities = entities.Select(harpy => harpy.GetComponent<EngineComponent>());

            foreach (var targetEntity in MapComponent.Instance.engine.Entities)
            {
                var lifeComponent = targetEntity.GameObject.GetComponent<LifeComponent>();

                if (lifeComponent == null) continue;

                var isInRange = harpyEntities.Any(harpyCell => harpyCell.Engine.IsInHitboxRange(harpyCell.Entity, targetEntity, SearchRangeEngineUnits));

                if (!isInRange) continue;

                var targetTeamComponent = lifeComponent.gameObject.GetComponent<TeamComponent>();

                if ((hp == null || hp.Value < lifeComponent.lifeValue) && teamComponent.Team != targetTeamComponent.Team)
                {
                    hp = Mathf.FloorToInt(lifeComponent.lifeValue);

                    target = new Enemy(lifeComponent.gameObject);
                }
            }

            return target;
        }

        public List<EffectIndicator> FindTargets()
        {
            var target = findTarget();

            return (target == null) ?
                new List<EffectIndicator>() :
                new List<EffectIndicator>() {
                    new EntitiesIndicator(
                        new List<EngineEntity> { target.enemyEntity }
                    )
                };
        }
    }
}