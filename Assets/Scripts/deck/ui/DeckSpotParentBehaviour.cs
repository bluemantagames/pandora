using UnityEngine;
using System.Collections.Generic;
using Pandora.Deck;

namespace Pandora.Deck.UI
{
    public class DeckSpotParentBehaviour : MonoBehaviour
    {
        private ModelSingleton modelSingleton = ModelSingleton.instance;

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

        public void LoadSavedDeck()
        {
            var deckSlot = modelSingleton.DeckSlots[0];

            if (deckSlot == null) return;

            var deck = JsonUtility.FromJson<List<string>>(deckSlot.deck);

            if (deck == null) return;

            var menuCardsParent = transform.parent.GetComponentInChildren<MenuCardsParentBehaviour>();

            var spots = new Queue<DeckSpotBehaviour>(GetComponentsInChildren<DeckSpotBehaviour>());

            foreach (var cardName in deck)
            {
                var spot = spots.Dequeue();

                var card = menuCardsParent.FindCard(cardName);

                card.SetSpot(spot.gameObject);
            }
        }
    }
}