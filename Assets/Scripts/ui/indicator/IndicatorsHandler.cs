using UnityEngine;
using System.Collections.Generic;
using Pandora;
using Pandora.Engine;

namespace Pandora.UI {
    public class IndicatorsHandler: MonoBehaviour, IndicatorsVisitor {
        List<EngineEntity> highlightedEntities = new List<EngineEntity>(300);
        List<GameObject> circleIndicators = new List<GameObject>(300);
        public GameObject CircleIndicator;

        public void ProcessIndicators(List<EffectIndicator> indicators) {
            Clear();

            foreach (var indicator in indicators) {
                indicator.visit(this);
            }
        }

        public void visit(FollowingCircleRangeIndicator indicator) {
            var circle = Instantiate(CircleIndicator, indicator.Followed.transform, false);

            circle.transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(indicator.RadiusEngineUnits, indicator.RadiusEngineUnits));

            circle.transform.localScale = new Vector2(
                worldSpaceRadius.x / circle.transform.lossyScale.x,
                worldSpaceRadius.y / circle.transform.lossyScale.y
            );

            circleIndicators.Add(circle);
        }


        public void visit(CircleRangeIndicator indicator) {
            var worldPosition = MapComponent.Instance.engine.PhysicsToWorld(indicator.PositionEngineUnits);

            var circle = Instantiate(CircleIndicator, worldPosition, Quaternion.identity);

            circle.transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(indicator.RadiusEngineUnits, indicator.RadiusEngineUnits));

            circle.transform.localScale = new Vector2(
                worldSpaceRadius.x / circle.transform.lossyScale.x,
                worldSpaceRadius.y / circle.transform.lossyScale.y
            );

            circleIndicators.Add(circle);
        }

        public void visit(EntitiesIndicator indicator) {
            foreach (var entity in indicator.Entities) {
                var entityHighlighter = entity.GameObject.GetComponent<EntityHighlighter>();

                if (entityHighlighter == null) {
                    Logger.DebugWarning($"Entity {entity} has no highlighter");

                    continue;
                }

                entityHighlighter.Highlight(new ColorHighlight(Color.yellow));

                highlightedEntities.Add(entity);
            }
        }

        void Clear() {
            foreach (var entity in highlightedEntities) {
                var entityHighlighter = entity.GameObject.GetComponent<EntityHighlighter>();

                entityHighlighter.Dehighlight(entityHighlighter.Current);
            }

            foreach (var circle in circleIndicators) {
                Destroy(circle);
            }

            highlightedEntities.Clear();
            circleIndicators.Clear();
        }
    }
}