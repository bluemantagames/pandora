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
        Dictionary<Guid, List<GameObject>> rectangleIndicators = new Dictionary<Guid, List<GameObject>>(300);
        Dictionary<Guid, List<EngineEntity>> highlightedEntities = new Dictionary<Guid, List<EngineEntity>>(300);
        public GameObject CircleIndicator, RectangleIndicator;
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

            circle.GetComponent<CircleIndicatorBehaviour>().Initialize(indicator.RadiusEngineUnits);

            addCircleIndicator(circle);
        }

        public void visit(CircleRangeIndicator indicator)
        {
            var worldPosition = (Vector3)MapComponent.Instance.engine.PhysicsToWorld(indicator.PositionEngineUnits);

            worldPosition.z = -1;

            var circle = Instantiate(CircleIndicator, worldPosition, Quaternion.identity);

            circle.GetComponent<CircleIndicatorBehaviour>().Initialize(indicator.RadiusEngineUnits);

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

        public void visit(LaneIndicator indicator)
        {
            var mapComponent = MapComponent.Instance;
            var engine = mapComponent.engine;

            var engineXPosition = (indicator.Lane.GridXPosition() * engine.UnitsPerCell) + engine.UnitsPerCell / 2;
            var engineYPosition = (mapComponent.RiverY * engine.UnitsPerCell) + engine.UnitsPerCell / 2;

            var position = engine.PhysicsToMapWorld(new Vector2Int(engineXPosition, engineYPosition));

            var rectangle = Instantiate(RectangleIndicator, position, Quaternion.identity);

            rectangle.GetComponent<RectangleIndicatorBehaviour>().Initialize(
                mapComponent.engine.UnitsPerCell,
                mapComponent.engine.UnitsPerCell * 15
            );

            addRectangleIndicator(rectangle);
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

                highlightedEntities.Remove(guid);
            }

            if (circleIndicators.ContainsKey(guid))
            {
                foreach (var circle in circleIndicators[guid])
                {
                    Destroy(circle);
                }

                circleIndicators.Remove(guid);
            }

            if (rectangleIndicators.ContainsKey(guid))
            {
                foreach (var rectangle in rectangleIndicators[guid])
                {
                    Destroy(rectangle);
                }

                rectangleIndicators.Remove(guid);
            }
        }

        public void Clear()
        {
            foreach (var guid in highlightedEntities.Keys) Clear(guid);
            foreach (var guid in circleIndicators.Keys) Clear(guid);
        }


        void addRectangleIndicator(GameObject circle)
        {
            if (currentGuid.HasValue)
            {
                if (rectangleIndicators.ContainsKey(currentGuid.Value))
                    rectangleIndicators[currentGuid.Value].Add(circle);
                else
                    rectangleIndicators[currentGuid.Value] = new List<GameObject> { circle };
            }
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