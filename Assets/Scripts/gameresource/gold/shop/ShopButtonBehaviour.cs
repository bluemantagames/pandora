using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pandora;

namespace Pandora.Resource.Gold.Shop {
    public class ShopButtonBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Logger.Debug("Shop button clicked");
        }
    }

}