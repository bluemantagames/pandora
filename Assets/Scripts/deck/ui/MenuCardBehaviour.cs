using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Pandora.Network;
using Pandora.Pool;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Pandora.Deck.UI
{
    public class MenuCardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        GameObject originalParent;
        Vector2 originalPosition, originalPivot;

        public GameObject Canvas;
        public ScrollRect ParentScrollRect;
        public string CardName;
        public bool UiDisabled = false;

        DeckSpotParentBehaviour deckSpotParentBehaviour;
        DeckSpotBehaviour lastDeckSpot;
        Image imageComponent;
        Color imageColor;
        int menuCardSiblingIndex;
        GameObject placeholderCard;
        CancellationTokenSource draggingCancellationToken;
        bool isDragging = false;
        int draggingDelay = 80;
        bool disableScrollInteraction = false;

        public void Load()
        {
            originalParent = transform.parent.gameObject;
            originalPosition = transform.localPosition;
            originalPivot = GetComponent<RectTransform>().pivot;
            deckSpotParentBehaviour = GameObject.Find("Canvas").GetComponentInChildren<DeckSpotParentBehaviour>();
            menuCardSiblingIndex = gameObject.transform.GetSiblingIndex();
            ParentScrollRect = GetComponentInParent<ScrollRect>();
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

            disableScrollInteraction = false;
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

            disableScrollInteraction = true;

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
            newCardMenuCardBehaviour.ParentScrollRect = ParentScrollRect;
        }

        public void SetSpotWithPlaceholder(GameObject deckSpot)
        {
            CreatePlaceholder();
            SetSpot(deckSpot);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _ = StartDragTimer();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelDragTimer();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelDragTimer();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (ShouldDragBubble())
            {
                CancelDragTimer();

                if (ParentScrollRect == null) return;

                ExecuteEvents.Execute(ParentScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }
            else
            {
                if (placeholderCard == null) CreatePlaceholder();

                gameObject.transform.SetParent(Canvas.transform);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ShouldDragBubble())
            {
                CancelDragTimer();

                if (ParentScrollRect == null) return;

                ExecuteEvents.Execute(ParentScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
            }
            else
            {
                transform.position = Input.mousePosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (ShouldDragBubble())
            {
                CancelDragTimer();

                if (ParentScrollRect == null) return;

                ExecuteEvents.Execute(ParentScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
            }
            else
            {
                isDragging = false;

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

        private async UniTaskVoid StartDragTimer()
        {
            draggingCancellationToken = new CancellationTokenSource();
            await UniTask.Delay(draggingDelay, cancellationToken: draggingCancellationToken.Token);
            isDragging = true;
        }

        private void CancelDragTimer()
        {
            if (draggingCancellationToken == null) return;

            draggingCancellationToken.Cancel();
        }

        private bool ShouldDragBubble()
        {
            return !disableScrollInteraction && (!isDragging || UiDisabled == true);
        }
    }

}