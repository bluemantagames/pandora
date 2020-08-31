using UnityEngine;
using System.Collections.Generic;
using Pandora.Deck;

namespace Pandora.Deck.UI {
    public class DeckSpotParentBehaviour: MonoBehaviour {
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

        public void LoadSavedDeck() {
            var serializedDeckWrapper = PlayerPrefs.GetString("DeckWrapper");

            Logger.Debug($"Loaded {serializedDeckWrapper}");

            if (serializedDeckWrapper == null) return;

            var deckWrapper = ScriptableObject.CreateInstance<DeckWrapper>();

            JsonUtility.FromJsonOverwrite(serializedDeckWrapper, deckWrapper);

            var menuCardsParent = transform.parent.GetComponentInChildren<MenuCardsParentBehaviour>();

            var spots = new Queue<DeckSpotBehaviour>(GetComponentsInChildren<DeckSpotBehaviour>());

            foreach (var cardName in deckWrapper.Cards) {
                var spot = spots.Dequeue();

                var card = menuCardsParent.FindCard(cardName);

                card.SetSpot(spot.gameObject);
            }
        }
    }
}