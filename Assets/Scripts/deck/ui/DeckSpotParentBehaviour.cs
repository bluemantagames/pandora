using UnityEngine;
using System.Collections.Generic;
using Pandora.Deck;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Net;
using Pandora.Network;

namespace Pandora.Deck.UI
{
    public class DeckSpotParentBehaviour : MonoBehaviour
    {
        private ModelSingleton modelSingleton = ModelSingleton.instance;
        ApiControllerSingleton apiControllerSingleton = ApiControllerSingleton.instance;

        public List<Card> Deck
        {
            get
            {
                var deck = new List<Card> { };
                var spots = gameObject.GetComponentsInChildren<DeckSpotBehaviour>();

                foreach (var deckSpotBehaviour in spots)
                {
                    deck.Add(deckSpotBehaviour.Card);
                }

                return deck;
            }
        }

        public async UniTaskVoid SaveDeck()
        {
            var activeDeckSlot = modelSingleton.User.activeDeckSlot;

            if (activeDeckSlot == null) return;

            var deck = Deck.Select(d => d != null ? d.Name : null).ToList();
            var token = modelSingleton.Token;

            Debug.Log($"Saving deck slot {activeDeckSlot} with deck");
            Debug.Log(deck);

            var response = await apiControllerSingleton.DeckSlotUpdate((long)activeDeckSlot, deck, token);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.Log($"Updated deck slot {activeDeckSlot}");

                // Updating the model
                modelSingleton.DeckSlots = modelSingleton.DeckSlots.Select(deckSlot =>
                {
                    if (deckSlot.id == activeDeckSlot)
                        deckSlot.deck = deck;

                    return deckSlot;
                }).ToList();
            }
            else
                Debug.Log(response.Error.message);
        }

        public void LoadSavedDeck(int deckSlotIndex)
        {
            var deckSlot = modelSingleton.DeckSlots[deckSlotIndex];

            if (deckSlot == null) return;

            var menuCardsParent = transform.parent.GetComponentInChildren<MenuCardsParentBehaviour>();
            var spots = new Queue<DeckSpotBehaviour>(GetComponentsInChildren<DeckSpotBehaviour>());

            Debug.Log(deckSlot.deck);

            if (deckSlot.deck == null) return;

            foreach (var cardName in deckSlot.deck)
            {
                Debug.Log($"Loading {cardName}");

                var spot = spots.Dequeue();

                if (cardName != null)
                {
                    var card = menuCardsParent.FindCard(cardName);

                    if (card != null)
                        card.SetSpot(spot.gameObject);
                }
            }
        }

        public void Reset()
        {
            var cards = gameObject.GetComponentsInChildren<MenuCardBehaviour>();

            foreach (var menuCardBehaviour in cards)
            {
                menuCardBehaviour.Reset();
            }
        }
    }
}