using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pandora.UI.Modal;
using Pandora;
using Pandora.Deck;

namespace Pandora.UI.Menu.Deck
{
    public class SingleCardDetails : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        private MenuModalBehaviour modalContainer;
        private CardBehaviour cardBehaviour;
        private bool isDragging = false;

        void Awake()
        {
            var mainCanvas = GameObject.Find("Main").gameObject;

            modalContainer = mainCanvas.GetComponentInChildren<MenuModalBehaviour>();

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

            var modalComponent = InstantiateModal();
            modalContainer.AppendComponent(modalComponent);
            modalContainer.Show();
        }

        private GameObject InstantiateModal()
        {
            var cardType = cardBehaviour.CardType;
            GameObject detailedCardResource = null;

            switch (cardType)
            {
                case CardType.Controller:
                    detailedCardResource = Resources.Load("DetailedCards/ControllerDetailedCard") as GameObject;
                    break;

                case CardType.Damage:
                    detailedCardResource = Resources.Load("DetailedCards/DamageDetailedCard") as GameObject;
                    break;

                case CardType.Spell:
                    detailedCardResource = Resources.Load("DetailedCards/SpellDetailedCard") as GameObject;
                    break;

                case CardType.Support:
                    detailedCardResource = Resources.Load("DetailedCards/SupportDetailedCard") as GameObject;
                    break;

                case CardType.Tank:
                    detailedCardResource = Resources.Load("DetailedCards/TankDetailedCard") as GameObject;
                    break;
            }

            var instantiatedModal = Instantiate(detailedCardResource);

            var cardModalBehaviour = instantiatedModal.GetComponent<CardModalBehaviour>();
            cardModalBehaviour.CurrentCardBehaviour = cardBehaviour;
            cardModalBehaviour.LoadInfo();

            return instantiatedModal;
        }
    }
}