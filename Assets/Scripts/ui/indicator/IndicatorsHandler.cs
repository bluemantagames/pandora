using UnityEngine;
using System;
using System.Collections.Generic;
using Pandora;
using Pandora.Engine;

namespace Pandora.UI
{
    public class IndicatorsHandler : MonoBehaviour, IndicatorsVisitor
    {
        Dictionary<Guid, List<GameObject>> circleIndicators = new Dictionary<Guid, List<GameObject>>(300);
        Dictionary<Guid, List<EngineEntity>> highlightedEntities = new Dictionary<Guid, List<EngineEntity>>(300);
        public GameObject CircleIndicator;
        public Color HighlightColor = Color.yellow;

        Guid? currentGuid = null;

        public Guid ProcessIndicators(List<EffectIndicator> indicators)
        {
            currentGuid = Guid.NewGuid();

            foreach (var indicator in indicators)
            {
                indicator.visit(this);
            }

            return currentGuid.Value;
        }

        public void visit(FollowingCircleRangeIndicator indicator)
        {
            var circle = Instantiate(CircleIndicator, indicator.Followed.transform, false);

            var position = circle.transform.position;

            position.z = -1;

            circle.transform.position = position;

            circle.transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(indicator.RadiusEngineUnits, indicator.RadiusEngineUnits));

            circle.transform.localScale = new Vector2(
                worldSpaceRadius.x / circle.transform.lossyScale.x,
                worldSpaceRadius.y / circle.transform.lossyScale.y
            );

            addCircleIndicator(circle);
        }


        public void visit(CircleRangeIndicator indicator)
        {
            var worldPosition = (Vector3)MapComponent.Instance.engine.PhysicsToWorld(indicator.PositionEngineUnits);

            worldPosition.z = -1;

            var circle = Instantiate(CircleIndicator, worldPosition, Quaternion.identity);

            circle.transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(indicator.RadiusEngineUnits, indicator.RadiusEngineUnits));

            circle.transform.localScale = new Vector2(
                worldSpaceRadius.x / circle.transform.lossyScale.x,
                worldSpaceRadius.y / circle.transform.lossyScale.y
            );

            addCircleIndicator(circle);
        }

        public void visit(EntitiesIndicator indicator)
        {
            foreach (var entity in indicator.Entities)
            {
                var entityHighlighter = entity.GameObject.GetComponent<EntityHighlighter>();

                if (entityHighlighter == null)
                {
                    Logger.DebugWarning($"Entity {entity} has no highlighter");

                    continue;
                }

                entityHighlighter.Highlight(new ColorHighlight(HighlightColor));

                addHighlightedEntity(entity);
            }
        }

        public void Clear(Guid guid)
        {
            if (highlightedEntities.ContainsKey(guid))
            {
                foreach (var entity in highlightedEntities[guid])
                {
                    var entityHighlighter = entity.GameObject.GetComponent<EntityHighlighter>();

                    entityHighlighter.Dehighlight(entityHighlighter.Current);
                }
            }

            if (circleIndicators.ContainsKey(guid))
            {
                foreach (var circle in circleIndicators[guid])
                {
                    Destroy(circle);
                }
            }

            highlightedEntities.Clear();
            circleIndicators.Clear();
        }

        public void Clear()
        {
            foreach (var guid in highlightedEntities.Keys) Clear(guid);
            foreach (var guid in circleIndicators.Keys) Clear(guid);
        }

        void addCircleIndicator(GameObject circle)
        {
            if (currentGuid.HasValue)
            {
                if (circleIndicators.ContainsKey(currentGuid.Value))
                    circleIndicators[currentGuid.Value].Add(circle);
                else
                    circleIndicators[currentGuid.Value] = new List<GameObject> { circle };
            }
        }

        void addHighlightedEntity(EngineEntity entity)
        {
            if (currentGuid.HasValue)
            {
                if (highlightedEntities.ContainsKey(currentGuid.Value))
                    highlightedEntities[currentGuid.Value].Add(entity);
                else
                    highlightedEntities[currentGuid.Value] = new List<EngineEntity> { entity };
            }
        }
    }
}