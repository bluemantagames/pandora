using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pandora.Deck;
using Pandora.Deck.UI;

public class MulliganTakeBehaviour : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) {
		LocalDeck.Instance.MulliganTaken();
	} 
}
