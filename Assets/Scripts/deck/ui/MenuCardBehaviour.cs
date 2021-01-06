using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Pandora.Network;
using Pandora.Pool;

namespace Pandora.Deck.UI
{
    public class MenuCardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        GameObject originalParent;
        Vector2 originalPosition, originalPivot;

        public GameObject Canvas;
        public string CardName;
        public bool UiDisabled = false;

        DeckSpotParentBehaviour deckSpotParentBehaviour;
        DeckSpotBehaviour lastDeckSpot;
        Image imageComponent;
        Color imageColor;
        int menuCardSiblingIndex;
        GameObject placeholderCard;

        public void Load()
        {
            originalParent = transform.parent.gameObject;
            originalPosition = transform.localPosition;
            originalPivot = GetComponent<RectTransform>().pivot;
            deckSpotParentBehaviour = GameObject.Find("Canvas").GetComponentInChildren<DeckSpotParentBehaviour>();
            menuCardSiblingIndex = gameObject.transform.GetSiblingIndex();
        }

        public void Reset()
        {
            if (lastDeckSpot != null)
            {
                lastDeckSpot.Card = null;
                lastDeckSpot.CardObject = null;
            }

            if (placeholderCard != null)
            {
                // Here we are setting the placeholder as the
                // last sibling because the removal will not
                // happen instantly and this will cause 
                // problems with the card positioning.
                placeholderCard.transform.SetAsLastSibling();

                Destroy(placeholderCard);
            }

            transform.SetParent(originalParent.transform);
            transform.SetSiblingIndex(menuCardSiblingIndex);
            GetComponent<RectTransform>().pivot = originalPivot;
        }

        public void SetSpot(GameObject deckSpot)
        {
            var deckSpotBehaviour = lastDeckSpot = deckSpot.GetComponent<DeckSpotBehaviour>();

            if (deckSpotBehaviour.CardObject != null)
            {
                if (deckSpotBehaviour.CardObject != gameObject)
                    deckSpotBehaviour.CardObject.GetComponent<MenuCardBehaviour>()?.Reset();
            }

            deckSpotBehaviour.Card = new Card(CardName);
            deckSpotBehaviour.CardObject = gameObject;

            gameObject.transform.SetParent(deckSpot.transform);

            Vector2 newPosition = PoolInstances.Vector2Pool.GetObject();
            newPosition.x = 0;
            newPosition.y = 0;

            transform.localPosition = newPosition;

            var deckSpotRect = deckSpotBehaviour.GetComponent<RectTransform>();
            var rectTransform = GetComponent<RectTransform>();

            rectTransform.sizeDelta = deckSpotRect.sizeDelta;
            rectTransform.pivot = deckSpotRect.pivot;

            PoolInstances.Vector2Pool.ReturnObject(newPosition);
        }

        private void CreatePlaceholder()
        {
            var newCard = Instantiate(gameObject);
            var newCardMenuCardBehaviour = newCard.GetComponent<MenuCardBehaviour>();
            var newCardCardBehaviour = newCard.GetComponent<CardBehaviour>();

            // A placeholder doesn't need the
            // card behaviour component
            Destroy(newCardCardBehaviour);

            newCardMenuCardBehaviour.UiDisabled = true;
            newCard.transform.SetParent(gameObject.transform.parent, false);
            placeholderCard = newCard;
            newCard.transform.SetSiblingIndex(menuCardSiblingIndex);
        }

        public void SetSpotWithPlaceholder(GameObject deckSpot)
        {
            CreatePlaceholder();
            SetSpot(deckSpot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (UiDisabled == true) return;
            if (placeholderCard == null) CreatePlaceholder();

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

            _ = deckSpotParentBehaviour.SaveDeck();
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