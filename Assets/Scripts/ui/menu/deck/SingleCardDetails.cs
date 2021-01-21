using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pandora.UI.Modal;

public class SingleCardDetails : MonoBehaviour, IPointerClickHandler
{
    public MenuModalBehaviour modalContainer;
    public GameObject cardDetailsModal;

    public void OnPointerClick(PointerEventData eventData)
    {
        modalContainer.AppendComponent(cardDetailsModal);
        modalContainer.Show();
    }
}
