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
        Dictionary<Guid, List<GameObject>> followingIndicators = new Dictionary<Guid, List<GameObject>>(300);
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

            addToDictionary(circleIndicators, circle);
        }

        public void visit(CircleRangeIndicator indicator)
        {
            var worldPosition = (Vector3)MapComponent.Instance.engine.PhysicsToWorld(indicator.PositionEngineUnits);

            worldPosition.z = -1;

            var circle = Instantiate(CircleIndicator, worldPosition, Quaternion.identity);

            circle.GetComponent<CircleIndicatorBehaviour>().Initialize(indicator.RadiusEngineUnits);

            addToDictionary(circleIndicators, circle);
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

                addToDictionary(highlightedEntities, entity);
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

            addToDictionary(rectangleIndicators, rectangle);
        }


        public void visit(FollowingGameObjectIndicator indicator)
        {
            var instance = Instantiate(indicator.Following, Vector3.zero, Quaternion.identity, indicator.Followed.transform);

            addToDictionary(followingIndicators, instance);
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

            if (followingIndicators.ContainsKey(guid))
            {
                foreach (var indicator in followingIndicators[guid])
                {
                    Destroy(indicator);
                }

                followingIndicators.Remove(guid);
            }
        }

        public void Clear()
        {
            foreach (var guid in highlightedEntities.Keys) Clear(guid);
            foreach (var guid in circleIndicators.Keys) Clear(guid);
            foreach (var guid in rectangleIndicators.Keys) Clear(guid);
            foreach (var guid in followingIndicators.Keys) Clear(guid);
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

        void addToDictionary<A>(Dictionary<Guid, List<A>> dictionary, A instance) {
            if (currentGuid.HasValue) {
                if (dictionary.ContainsKey(currentGuid.Value))
                    dictionary[currentGuid.Value].Add(instance);
                else
                    dictionary[currentGuid.Value] = new List<A> { instance };
            }
        }
    }
}