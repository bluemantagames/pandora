using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;
using Pandora.Deck.Event;
using Pandora.Events;
using UnityEngine.UI;
using System;
using System.Linq;

namespace Pandora.Deck
{
    public class HandBehaviour : MonoBehaviour
    {
        /// <summary>
        /// This is set before loading the GameScene
        ///
        /// It will be then loaded into the deck implementation by `Start()`
        ///</summary>
        public static List<Card> Deck;

        Animator animator;

        List<PlayableGraph> graphs = new List<PlayableGraph> { };

        Vector2 originalPosition;
        RectTransform rectTransform;

        HandCard[] hand = new HandCard[32];


        int handIndex = -1;

        public GameObject[] UIHandSlots;
        public float EaseOutTime = 0.35f;

        Deck deck;

        List<HandCard> selectedCards = new List<HandCard> { };

        // Mulligan stuff
        int mulligansAvailable = 1;
        float mulliganSecondsLeft = 20f;
        public GameObject MulliganTakeObject;
        public GameObject MulliganRejectObject;
        public GameObject MulliganTimerText;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();


            deck = LocalDeck.Instance;

            deck.EventBus.Subscribe<CardDrawn>(new EventSubscriber<DeckEvent>(CardDrawn, "HandDrawHandler"));
            deck.EventBus.Subscribe<CardPlayed>(new EventSubscriber<DeckEvent>(CardPlayed, "HandPlayHandler"));
            deck.EventBus.Subscribe<CardDiscarded>(new EventSubscriber<DeckEvent>(CardDiscarded, "HandDiscardHandler"));
            deck.EventBus.Subscribe<CardSelected>(new EventSubscriber<DeckEvent>(CardSelected, "MulliganSelectHandled"));
            deck.EventBus.Subscribe<MulliganTaken>(new EventSubscriber<DeckEvent>(MulliganTaken, "MulliganTakenHandled"));
            deck.EventBus.Subscribe<MulliganRejected>(new EventSubscriber<DeckEvent>(MulliganRejected, "MulliganRejectedHandled"));

            if (deck is LocalDeck localDeck)
            {
                List<Card> cards;

                if (Deck == null)
                {
                    /*var cardNames = new List<string> {
                        "Bard",
                        "Ranger",
                        "Clerics",
                        "Cockatrice",
                        "Fireball",
                        "Harpies",
                        "Mermaids",
                        "Troll",
                        "Zombies"
                    };*/
                    
                    var cardNames = new List<string> {
                        "Bard",
                        "Ranger",
                        "HalfOrc",
                        "Troll"
                    };

                    cards =
                        (from card in cardNames
                         select new Card(card)).ToList();
                }
                else
                {
                    cards = Deck;
                }

                localDeck.Deck = cards;
            }
        }

        void Update()
        {
            if (mulligansAvailable > 0) {
                if (mulliganSecondsLeft <= 0) DisableMulligan();

                mulliganSecondsLeft -= Time.deltaTime;
                UpdateMulliganText();
            }
        }

        void AnimateMovementTo(GameObject card, int idx)
        {
            var cardTransform = card.GetComponent<RectTransform>();

            Logger.Debug($"Animating {card} to {idx}");

            var targetRectTransform = UIHandSlots[idx].GetComponent<RectTransform>();

            cardTransform.pivot = targetRectTransform.pivot;

            var xCurve = AnimationCurve.EaseInOut(0f, cardTransform.anchoredPosition.x, EaseOutTime, targetRectTransform.anchoredPosition.x);
            var yCurve = AnimationCurve.EaseInOut(0f, cardTransform.anchoredPosition.y, EaseOutTime, targetRectTransform.anchoredPosition.y);

            var clip = new AnimationClip();
            clip.legacy = true;

            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", xCurve);
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", yCurve);

            var animation = card.GetComponent<Animation>();

            animation.AddClip(clip, clip.name);
            animation.Play(clip.name);
        }

        void CardPlayed(DeckEvent ev)
        {
            if (mulligansAvailable > 0) 
            {
                LocalDeck.Instance.MulliganReject();
            }

            var cardPlayed = ev as CardPlayed;

            for (var i = 0; i < hand.Length; i++)
            {
                if (hand[i] != null && hand[i].Name == cardPlayed.Name)
                {
                    hand[i] = null;
                }
            }
        }

        void CardDrawn(DeckEvent ev)
        {
            var cardDrawn = ev as CardDrawn;
            int idx;

            Logger.Debug($"Received {cardDrawn}");

            if (handIndex + 1 > deck.HandSize - 1)
            {
                // Shift the cards to the rightest position possible
                for (var i = deck.HandSize - 1; i >= 0; i--)
                {
                    Logger.Debug($"Checking {i}");

                    if (hand[i] == null) continue;

                    int? freePosition = null;

                    for (var j = i; j < deck.HandSize; j++)
                    {
                        if (hand[j] == null)
                        {
                            freePosition = j;

                            break;
                        }
                    }

                    if (freePosition.HasValue)
                    {
                        AnimateMovementTo(hand[i].CardObject, freePosition.Value);

                        hand[freePosition.Value] = hand[i];
                        hand[i] = null;
                    }
                }

                idx = 0;
            }
            else
            {
                idx = ++handIndex;
            }

            Logger.Debug($"Drawing card {idx}");

            var cardPrefab = Resources.Load($"Cards/{cardDrawn.Name}") as GameObject;
            var card = Instantiate(cardPrefab, transform.position, Quaternion.identity, transform.parent);

            hand[idx] = new HandCard(cardDrawn.Name, card);

            AnimateMovementTo(card, idx);

            Logger.Debug($"Playing from {rectTransform} to {idx}");
        }

        void CardDiscarded(DeckEvent ev)
        {
            var cardPlayed = ev as CardDiscarded;

            for (var i = 0; i < hand.Length; i++)
            {
                if (hand[i] != null && hand[i].Name == cardPlayed.Name)
                {
                    Destroy(hand[i].CardObject);
                    hand[i] = null;
                }
            }
        }

        void CardSelected(DeckEvent ev) 
        {
            var cardSelected = ev as CardSelected;
            var isMulligan = mulligansAvailable > 0;

            // This could be slow...
            for (var idx = 0; idx < hand.Length; idx++)
            {
                if (hand[idx] == null || hand[idx].Name != cardSelected.Name)
                {
                    continue;
                }

                var card = hand[idx];
                var selectedIndex = selectedCards.IndexOf(card);

                // If we are in the Mulligan and we select
                // a card that was already selected
                if (isMulligan && selectedIndex != -1)
                {
                    Deselect(selectedIndex);
                    break;
                }

                // If we are in the Mulligan, we select
                // a card that was NOT already selected
                // and we have still "space"
                if (isMulligan && selectedCards.Count < deck.MaxMulliganSize)
                {
                    Select(card);
                    break;
                }

                // If we are not in the Mulligan we just 
                // select cards individually
                if (!isMulligan) 
                {
                    // Deselect the previus cards (it shoul be just one but whatev)
                    for (var i = 0; i <= selectedCards.Count - 1; i++)
                        Deselect(i);

                    Select(card);
                    break;
                }
            }
        }

        void MulliganTaken(DeckEvent ev) 
        {
            if (mulligansAvailable <= 0 || selectedCards.Count <= 0) 
            {
                return;
            }

            for (int i = selectedCards.Count - 1; i >= 0; i--)
            {
                var handCard = selectedCards[i];
                
                LocalDeck.Instance.DiscardCard(new Card(handCard.Name));
                selectedCards.RemoveAt(i);
            }

            mulligansAvailable -= 1;

            if (mulligansAvailable <= 0)
            {
                DisableMulliganUI();
            }
        }

        void MulliganRejected(DeckEvent ev) 
        {
            if (mulligansAvailable <= 0) 
            {
                return;
            }

            DisableMulligan();
        }

        void OnDisable()
        {
            foreach (var graph in graphs)
            {
                if (graph.IsValid()) graph.Destroy();
            }
        }

        void Select(HandCard card) {
            selectedCards.Add(card);
            card.CardObject.GetComponent<CardBehaviour>().MulliganSelected = true;
        }

        void Deselect(int index) {
            if (selectedCards.ElementAt(index) == null) return;
            
            selectedCards[index].CardObject.GetComponent<CardBehaviour>().MulliganSelected = false;
            selectedCards.RemoveAt(index);
        }

        void UpdateMulliganText()
        {
            if (MulliganTimerText == null)
            {
                return;
            }

            var textComponent = MulliganTimerText.GetComponent<Text>();
            var timerValue = mulliganSecondsLeft > 0 ? mulliganSecondsLeft : 0;

            textComponent.text = 
                $"Mulligan ends in {string.Format("{0:F1}", timerValue)} seconds";
        }

        void DisableMulliganUI()
        {
            if (MulliganTakeObject != null) Destroy(MulliganTakeObject);
            if (MulliganRejectObject != null) Destroy(MulliganRejectObject);
            if (MulliganTimerText != null) Destroy(MulliganTimerText);
        }

        void DisableMulligan()
        {
            mulligansAvailable = 0;
            DisableMulliganUI();
        }

    }
}