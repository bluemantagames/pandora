using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Pandora.Engine;
using Pandora.Network;
using Pandora.Command;
using System.Collections.Generic;
using Pandora.UI.HUD;
using Pandora.UI;
using UnityEngine.UI;

namespace Pandora
{
    public class CommandImageBehaviour : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
    {
        public string UnitId;
        public bool SmartCast = false;
        public int RefreshEveryFrames = 20;
        Guid? currentGuid = null;
        Image image;
        IndicatorsHandler indicatorsHandler;
        int lastRefresh = 0;

        public CommandViewportBehaviour parent;
        GameObject commandsThreshold;
        float? originalY = null, endDragTime = null;
        float endDragDebounceTime = 0.2f;

        bool dragging = false;

        void Start()
        {
            indicatorsHandler = MapComponent.Instance.GetComponent<IndicatorsHandler>();

            commandsThreshold = GameObject.FindWithTag("CommandThreshold");

            image = GetComponent<Image>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (SmartCast) return;

            dragging = true;

            if (!originalY.HasValue)
                originalY = transform.position.y;

            transform.position = new Vector2(transform.position.x, eventData.position.y);

            var transparency = (eventData.position.y / commandsThreshold.transform.position.y) * 0.8f;
            var color = image.color;

            color.a = 1f - transparency;

            image.color = color;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (SmartCast) return;

            endDragTime = Time.time;
            dragging = false;

            var shouldActivateCommand = (eventData.position.y / commandsThreshold.transform.position.y) >= 1;

            if (shouldActivateCommand)
                runCommand();
            else if (originalY.HasValue)
            {
                var position = transform.position;

                position.y = originalY.Value;

                transform.position = position;

                var color = image.color;

                color.a = 1;

                image.color = color;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var shouldDebounce =
                (endDragTime.HasValue && Time.time - endDragTime.Value < endDragDebounceTime) || dragging;

            if (shouldDebounce) return;

            parent.UnhighlightAll();

            if (SmartCast)
                runCommand();
            else
            {
                var cardHighlighter = GetComponent<PositionCardHighlighter>();

                if (cardHighlighter == null)
                {
                    Debug.Log("Highlighting");

                    gameObject.AddComponent<PositionCardHighlighter>();

                    refreshIndicators();
                }
                else
                {
                    Unhighlight();
                }
            }
        }

        public void Unhighlight()
        {
            clearIndicators();

            GetComponent<PositionCardHighlighter>()?.Unhighlight();
        }

        void refreshIndicators()
        {
            clearIndicators();

            var indicators = FindCommandListener().GetComponentInParent<CommandBehaviour>().FindTargets();

            currentGuid = indicatorsHandler.ProcessIndicators(indicators);
        }

        void clearIndicators()
        {
            if (currentGuid.HasValue)
            {
                indicatorsHandler.Clear(currentGuid.Value);

                currentGuid = null;
            }
        }

        void Update()
        {
            if (!currentGuid.HasValue) return;

            lastRefresh++;

            if (lastRefresh > RefreshEveryFrames)
            {
                lastRefresh = 0;

                refreshIndicators();
            }
        }

        CommandListener FindCommandListener()
        {
            var entities = new List<EngineEntity>(MapComponent.Instance.engine.Entities);

            foreach (var entity in entities)
            {
                var group = entity.GameObject.GetComponent<GroupComponent>();
                var unitId = entity.GameObject.GetComponent<UnitIdComponent>();
                var isOurEntity = false;

                if (group != null && group.OriginalId == UnitId)
                {
                    isOurEntity = true;
                }
                else if (unitId != null && unitId.Id == UnitId)
                {
                    isOurEntity = true;
                }

                if (isOurEntity)
                {
                    return entity.GameObject.GetComponentInChildren<CommandListener>();
                }
            }

            return null;
        }

        void runCommand()
        {
            clearIndicators();

            FindCommandListener().Command();
        }
    }

}