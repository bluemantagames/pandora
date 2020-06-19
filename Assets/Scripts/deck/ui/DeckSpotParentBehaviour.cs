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

                foreach (Transform child in transform)
                {
                    var deckSpot = child.GetComponent<DeckSpotBehaviour>();

                    if (deckSpot != null)
                        deck.Add(deckSpot.Card);
                }

                return deck;
            }
        }

        public async UniTaskVoid SaveDeck()
        {
            var deck = Deck.Select(d => d != null ? d.Name : null).ToList();
            var slotId = modelSingleton.DeckSlots[0].id;
            var token = modelSingleton.Token;

            Debug.Log($"Saving deck slot {slotId} with deck");
            Debug.Log(deck);

            var response = await apiControllerSingleton.DeckSlotUpdate(slotId, deck, token);

            if (response.StatusCode == HttpStatusCode.OK)
                Debug.Log($"Updated deck slot {slotId}");
            else
                Debug.Log(response.Error.message);
        }

        public void LoadSavedDeck()
        {
            var deckSlot = modelSingleton.DeckSlots[0];

            if (deckSlot == null) return;

            var menuCardsParent = transform.parent.GetComponentInChildren<MenuCardsParentBehaviour>();
            var spots = new Queue<DeckSpotBehaviour>(GetComponentsInChildren<DeckSpotBehaviour>());

            Debug.Log(deckSlot.deck);

            if (deckSlot.deck == null) return;

            foreach (var cardName in deckSlot.deck)
            {
                Debug.Log(cardName);
                var spot = spots.Dequeue();

                if (cardName != null)
                {
                    var card = menuCardsParent.FindCard(cardName);
                    card.SetSpot(spot.gameObject);
                }
            }
        }
    }
}