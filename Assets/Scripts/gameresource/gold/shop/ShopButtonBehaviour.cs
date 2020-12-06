using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pandora;
using Pandora.UI.Modal;

namespace Pandora.Resource.Gold.Shop {
    public class ShopButtonBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public GameObject Modal;
        public GameObject ModalContent;
        public GameObject ShopContainer;
        public Sprite OpenShopSprite;
        Image image;

        Sprite closedShopSprite;
        bool isOpen = false;

        void Start() {
            image = GetComponent<Image>();

            closedShopSprite = image.sprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isOpen = !isOpen;

            if (isOpen) {
                ShopContainer.SetActive(true);

                image.sprite = OpenShopSprite;
            } else {
                ShopContainer.SetActive(false);

                image.sprite = closedShopSprite;
            }
        }
    }

}