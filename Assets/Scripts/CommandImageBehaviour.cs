using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Pandora.Engine;
using Pandora.Network;
using Pandora.Command;
using System.Collections.Generic;
using Pandora.UI.HUD;
using Pandora.UI;

namespace Pandora
{
    public class CommandImageBehaviour : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string UnitId;
        public bool SmartCast = false;
        public int RefreshEveryFrames = 20;
        Guid? currentGuid = null;
        IndicatorsHandler indicatorsHandler;
        int lastRefresh = 0;

        void Start()
        {
            indicatorsHandler = MapComponent.Instance.GetComponent<IndicatorsHandler>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = new Vector2(transform.position.x, eventData.position.y);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (SmartCast)
                RunCommand();
            else
            {
                var cardHighlighter = GetComponent<PositionCardHighlighter>();

                if (cardHighlighter == null)
                {
                    Debug.Log("Highlighting");

                    gameObject.AddComponent<PositionCardHighlighter>();

                    RefreshIndicators();
                }
                else
                {
                    ClearIndicators();

                    cardHighlighter.Unhighlight();
                }
            }
        }

        void RefreshIndicators()
        {
            ClearIndicators();

            var indicators = FindCommandListener().GetComponentInParent<CommandBehaviour>().FindTargets();

            currentGuid = indicatorsHandler.ProcessIndicators(indicators);
        }

        void ClearIndicators()
        {
            if (currentGuid.HasValue) {
                indicatorsHandler.Clear(currentGuid.Value);

                currentGuid = null;
            }
        }

        void Update() {
            if (!currentGuid.HasValue) return;

            lastRefresh++;

            if (lastRefresh > RefreshEveryFrames) {
                lastRefresh = 0;

                RefreshIndicators();
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

        void RunCommand()
        {
            FindCommandListener().Command();
        }
    }

}