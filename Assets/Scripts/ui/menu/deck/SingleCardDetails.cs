using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pandora.UI.Modal;
using Pandora;

namespace Pandora.UI.Menu.Deck
{
    public class SingleCardDetails : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        private MenuModalBehaviour modalContainer;
        private CardModalBehaviour cardModalBehaviour;
        private CardBehaviour cardBehaviour;
        private bool isDragging = false;

        void Awake()
        {
            var mainCanvas = GameObject.Find("Main").gameObject;

            modalContainer = mainCanvas.GetComponentInChildren<MenuModalBehaviour>();
            cardModalBehaviour = mainCanvas.GetComponentInChildren<CardModalBehaviour>();

            cardBehaviour = GetComponent<CardBehaviour>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging) return;

            cardModalBehaviour.CurrentCardBehaviour = cardBehaviour;
            cardModalBehaviour.LoadInfo();

            var cardModalObject = cardModalBehaviour.gameObject;
            modalContainer.AppendComponent(cardModalObject);
            modalContainer.Show();
        }
    }
}