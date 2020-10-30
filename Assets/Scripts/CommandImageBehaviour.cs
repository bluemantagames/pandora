using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Pandora.Engine;
using Pandora.Network;
using Pandora.Command;
using System.Collections.Generic;
using Pandora.UI.HUD;

namespace Pandora
{
    public class CommandImageBehaviour : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public string UnitId;
        public bool SmartCast = false;

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
            else {
                var cardHighlighter = GetComponent<PositionCardHighlighter>();

                if (cardHighlighter == null) {
                    Debug.Log("Highlighting");

                    gameObject.AddComponent<PositionCardHighlighter>();
                } else {
                    cardHighlighter.Unhighlight();
                }
            }
        }

        void RunCommand()
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
                    entity.GameObject.GetComponentInChildren<CommandListener>().Command();

                    break;
                }
            }
        }
    }

}