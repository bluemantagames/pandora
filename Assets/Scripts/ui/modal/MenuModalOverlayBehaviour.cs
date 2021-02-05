using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pandora.UI.Modal
{
    public class MenuModalOverlayBehaviour : MonoBehaviour, IPointerClickHandler
    {
        private MenuModalBehaviour modalContainer;

        void Awake()
        {
            modalContainer = GetComponentInParent<MenuModalBehaviour>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            modalContainer.Hide();
        }
    }
}