using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Deck.UI
{
    public class MenuCardsParentBehaviour : MonoBehaviour
    {
        void Awake()
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<CardBehaviour>().IsUI = true;
            }

            GameObject.Find("Canvas").GetComponentInChildren<DeckSpotParentBehaviour>().LoadSavedDeck();
        }

        public MenuCardBehaviour FindCard(string cardName) =>
            GetComponentsInChildren<MenuCardBehaviour>()
                .ToList()
                .Find(c => c.CardName == cardName);
    }

}