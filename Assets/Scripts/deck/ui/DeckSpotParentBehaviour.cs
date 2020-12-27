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
        private PlayerModelSingleton playerModelSingleton;
        private ApiControllerSingleton apiControllerSingleton;

        void Awake()
        {
            playerModelSingleton = PlayerModelSingleton.instance;
            apiControllerSingleton = ApiControllerSingleton.instance;
        }

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
            var activeDeckSlot = playerModelSingleton?.User?.activeDeckSlot;

            if (activeDeckSlot == null) return;

            var deck = Deck.Select(d => d != null ? d.Name : null).ToList();
            var token = playerModelSingleton.Token;

            Logger.Debug($"Saving deck slot {activeDeckSlot}");

            var response = await apiControllerSingleton.DeckSlotUpdate((long)activeDeckSlot, deck, token);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Logger.Debug($"Updated deck slot {activeDeckSlot}");

                // Updating the model
                playerModelSingleton.DeckSlots = playerModelSingleton.DeckSlots.Select(deckSlot =>
                {
                    if (deckSlot.id == activeDeckSlot)
                        deckSlot.deck = deck;

                    return deckSlot;
                }).ToList();
            }
            else
                Logger.Debug(response.Error.message);
        }

        public void LoadSavedDeck(long deckSlotId)
        {
            Logger.Debug($"Loading deck {deckSlotId}");

            var deckSlot = playerModelSingleton?.DeckSlots.Find(slot => slot.id == deckSlotId);

            if (deckSlot == null) return;

            var menuCardsParent = transform.parent.GetComponentInChildren<MenuCardsParentBehaviour>();
            var spots = new Queue<DeckSpotBehaviour>(GetComponentsInChildren<DeckSpotBehaviour>());

            if (deckSlot.deck == null) return;

            foreach (var cardName in deckSlot.deck)
            {
                Logger.Debug($"Loading {cardName}");

                var spot = spots.Dequeue();

                if (cardName != null)
                {
                    var card = menuCardsParent.FindCard(cardName);

                    if (card != null)
                        card.SetSpotWithPlaceholder(spot.gameObject);
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