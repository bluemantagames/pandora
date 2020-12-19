using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Thx https://forum.unity.com/threads/passing-an-event-through-to-the-next-object-in-the-raycast.266614/

namespace Pandora.UI
{
    public class PassPointerEventsBehaviour : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IPointerUpHandler, IEndDragHandler
    {
        GameObject newTarget;

        public void OnPointerDown(PointerEventData eventData)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            if (raycastResults.Count >= 2)
            {
                newTarget = raycastResults[1].gameObject; //Array item 1 should be the one next underneath, handy to implement for-loop with check here if necessary.

                ExecuteEvents.Execute(newTarget, eventData, ExecuteEvents.pointerDownHandler);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            if (raycastResults.Count >= 2)
            {
                newTarget = raycastResults[1].gameObject; //Array item 1 should be the one next underneath, handy to implement for-loop with check here if necessary.
                ExecuteEvents.Execute(newTarget, eventData, ExecuteEvents.pointerUpHandler);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ExecuteEvents.Execute(newTarget, eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            ExecuteEvents.Execute(newTarget, eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ExecuteEvents.Execute(newTarget, eventData, ExecuteEvents.endDragHandler);
        }
    }
}