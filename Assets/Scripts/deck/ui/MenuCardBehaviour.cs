using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Pandora.Deck.UI
{
    public class MenuCardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        GameObject originalParent;
        Vector2 originalPosition, originalPivot;

        public GameObject Canvas;
        public string CardName;
        public bool UiDisabled = false;

        DeckSpotBehaviour lastDeckSpot;
        Image imageComponent;
        Color imageColor;

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

            if (lastDeckSpot != null)
            {
                lastDeckSpot.Card = null;
                lastDeckSpot.CardObject = null;
            }

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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (UiDisabled == true) return;
            gameObject.transform.SetParent(Canvas.transform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (UiDisabled == true) return;

            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (UiDisabled == true) return;

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

        void Awake()
        {
            imageComponent = gameObject.GetComponent<Image>();
            imageColor = imageComponent.color;
        }

        public void Update()
        {
            // Make it opaque if disabled
            var opaqueColor = new Color(imageColor.r, imageColor.g, imageColor.b, 0.2f);
            imageComponent.color = (UiDisabled == true) ? opaqueColor : imageColor;
        }
    }

}