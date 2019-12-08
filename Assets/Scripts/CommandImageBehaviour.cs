using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Pandora.Engine;
using Pandora.Network;
using Pandora.Command;

namespace Pandora
{
    public class CommandImageBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public string UnitId;

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (var entity in MapComponent.Instance.engine.Entities)
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