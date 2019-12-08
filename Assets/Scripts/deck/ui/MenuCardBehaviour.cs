using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Pandora.Deck.UI
{
    public class MenuCardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        GameObject originalParent;
        Vector2 originalPosition, originalPivot;

        public GameObject Canvas;
        public string CardName;

        DeckSpotBehaviour lastDeckSpot;

        public void Load()
        {
            originalParent = transform.parent.gameObject;
            originalPosition = transform.localPosition;
            originalPivot = GetComponent<RectTransform>().pivot;
        }

        public void Reset()
        {
            Logger.Debug($"Resetting {gameObject}");

            gameObject.transform.SetParent(originalParent.transform);

            transform.localPosition = originalPosition;

            lastDeckSpot.Card = null;
            lastDeckSpot.CardObject = null;

            GetComponent<RectTransform>().pivot = originalPivot;
        }

        public void SetSpot(GameObject deckSpot)
        {
            var deckSpotBehaviour = lastDeckSpot = deckSpot.GetComponent<DeckSpotBehaviour>();

            if (deckSpotBehaviour.CardObject != null)
            {
                deckSpotBehaviour.CardObject.GetComponent<MenuCardBehaviour>()?.Reset();
            }

            deckSpotBehaviour.Card = new Card(CardName);
            deckSpotBehaviour.CardObject = gameObject;

            gameObject.transform.SetParent(deckSpot.transform.parent);

            transform.localPosition = deckSpot.transform.localPosition;

            GetComponent<RectTransform>().pivot = deckSpot.GetComponent<RectTransform>().pivot;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (transform.parent.gameObject == originalParent)
            {
                gameObject.transform.SetParent(Canvas.transform);
            }

            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var pointerData = new PointerEventData(null);

            pointerData.position = Input.mousePosition;

            var foundElements = new List<RaycastResult> { };

            Canvas.GetComponent<GraphicRaycaster>().Raycast(pointerData, foundElements);

            var deckSpot = foundElements.Find(result => result.gameObject.GetComponent<DeckSpotBehaviour>() != null).gameObject;

            if (deckSpot != null)
            {
                SetSpot(deckSpot);
            }
            else
            {
                Reset();
            }
        }
    }

}