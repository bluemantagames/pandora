using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Pandora.Engine;
using Pandora.Network;
using Pandora.Command;
using System.Collections.Generic;

namespace Pandora
{
    public class CommandImageBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public string UnitId;

        public void OnPointerClick(PointerEventData eventData)
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
                }
            }
        }
    }

}