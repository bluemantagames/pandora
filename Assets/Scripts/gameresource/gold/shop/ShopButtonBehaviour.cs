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

        public void OnPointerClick(PointerEventData eventData)
        {
            var modal = Instantiate(Modal);

            modal.GetComponentInChildren<ModalBehaviour>().AddContent(ModalContent);

            Logger.Debug("Shop button clicked");
        }
    }

}