using UnityEngine;
using System.Collections.Generic;
using Pandora;
using Pandora.Engine;

namespace Pandora.UI {
    public class IndicatorsHandler: MonoBehaviour {
        List<EngineEntity> highlightedEntities = new List<EngineEntity>(300);
        List<GameObject> circleIndicators = new List<GameObject>(300);
        public GameObject CircleIndicator;

        public void ProcessIndicators(List<EffectIndicator> indicators) {
            Clear();
        }

        void ProcessFollowingCircle(FollowingCircleRangeIndicator indicator) {
            var circle = Instantiate(CircleIndicator, indicator.Followed.transform, false);

            circle.transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(indicator.RadiusEngineUnits, indicator.RadiusEngineUnits));

            circle.transform.localScale = new Vector2(
                worldSpaceRadius.x / circle.transform.lossyScale.x,
                worldSpaceRadius.y / circle.transform.lossyScale.y
            );
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