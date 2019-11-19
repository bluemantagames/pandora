using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Deck.UI
{
    public class MenuCardsParentBehaviour : MonoBehaviour
    {
        public List<Card> Deck
        {
            get
            {
                var deck = new List<Card> { };

                foreach (Transform child in transform)
                {
                    var card = child.GetComponent<DeckSpotBehaviour>()?.Card;

                    if (card != null)
                    {
                        deck.Add(card);
                    }
                }

                return deck;
            }
        }

        void Start()
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<CardBehaviour>().IsUI = true;
            }
        }
    }

}