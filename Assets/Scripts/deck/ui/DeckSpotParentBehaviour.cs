using UnityEngine;
using System.Collections.Generic;
using Pandora.Deck;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Net;
using Pandora.Network;
using UnityEngine.UI;
using Pandora.Pool;
using System;

namespace Pandora.Deck.UI
{
    public class DeckSpotParentBehaviour : MonoBehaviour
    {
        public Text ManaCurveText;
        public MenuCardsParentBehaviour MenuCardsParent;
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

        public List<CardBehaviour> DeckInfo
        {
            get
            {
                var cards = gameObject.GetComponentsInChildren<CardBehaviour>();

                return new List<CardBehaviour>(cards);
            }
        }

        public async UniTaskVoid SaveDeck()
        {
            UpdateManaCurve(DeckInfo);

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

            var spots = new Queue<DeckSpotBehaviour>(GetComponentsInChildren<DeckSpotBehaviour>());

            if (deckSlot.deck == null) return;

            foreach (var cardName in deckSlot.deck)
            {
                Logger.Debug($"Loading {cardName}");

                var spot = spots.Dequeue();

                if (cardName != null)
                {
                    var card = MenuCardsParent.FindCard(cardName);

                    if (card != null)
                        card.SetSpotWithPlaceholder(spot.gameObject);
                }
            }

            UpdateManaCurve(DeckInfo);
        }

        public void Reset()
        {
            var cards = gameObject.GetComponentsInChildren<MenuCardBehaviour>();

            foreach (var menuCardBehaviour in cards)
            {
                menuCardBehaviour.Reset();
            }

            UpdateManaCurve(DeckInfo);
        }

        private void UpdateManaCurve(List<CardBehaviour> deck)
        {
            if (ManaCurveText == null) return;

            // We could probably use the decimal pool here
            // but it's a really minor improvement
            var cardsMana = deck.Select(c => c.RequiredManaShowed);
            var curve = CalculateManaCurve(cardsMana);
            var decimalCurve = Convert.ToDecimal(curve);
            var roudedCurve = decimalCurve.ToString("#.#");

            Logger.Debug($"Setting mana curve to: {roudedCurve}");

            ManaCurveText.text = $"{roudedCurve}";
        }

        private double CalculateManaCurve(IEnumerable<int> manas)
        {
            var curve = manas.Count() <= 0 ? 0 : manas.Average() / 10;

            return curve;
        }
    }
}