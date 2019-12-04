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

        int mulligansAvailable = 1;

        List<HandCard> mulliganSelected = new List<HandCard> { };

        public GameObject MulliganTakeObject;
        public GameObject MulliganRejectObject;

        int handIndex = -1;

        public GameObject[] UIHandSlots;
        public float EaseOutTime = 0.35f;

        Deck deck;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();


            deck = LocalDeck.Instance;

            deck.EventBus.Subscribe<CardDrawn>(new EventSubscriber<DeckEvent>(CardDrawn, "HandDrawHandler"));
            deck.EventBus.Subscribe<CardPlayed>(new EventSubscriber<DeckEvent>(CardPlayed, "HandPlayHandler"));
            deck.EventBus.Subscribe<CardDiscarded>(new EventSubscriber<DeckEvent>(CardDiscarded, "HandDiscardHandler"));
            deck.EventBus.Subscribe<MulliganSelect>(new EventSubscriber<DeckEvent>(MulliganSelected, "MulliganSelectHandled"));
            deck.EventBus.Subscribe<MulliganTaken>(new EventSubscriber<DeckEvent>(MulliganTaken, "MulliganTakenHandled"));
            deck.EventBus.Subscribe<MulliganRejected>(new EventSubscriber<DeckEvent>(MulliganRejected, "MulliganRejectedHandled"));

            if (deck is LocalDeck localDeck)
            {
                List<Card> cards;

                if (Deck == null)
                {
                    var cardNames = new List<string> {
                        "Bard",
                        "Ranger",
                        "Clerics",
                        "Cockatrice",
                        "Fireball",
                        "Harpies",
                        "Mermaids",
                        "Troll",
                        "Zombies"
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
        }

        void AnimateMovementTo(GameObject card, int idx)
        {
            var cardTransform = card.GetComponent<RectTransform>();

            Debug.Log($"Animating {card} to {idx}");

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

            Debug.Log($"Received {cardDrawn}");

            if (handIndex + 1 > deck.HandSize - 1)
            {
                // Shift the cards to the rightest position possible
                for (var i = deck.HandSize - 1; i >= 0; i--)
                {
                    Debug.Log($"Checking {i}");

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

            Debug.Log($"Drawing card {idx}");

            var cardPrefab = Resources.Load($"Cards/{cardDrawn.Name}") as GameObject;
            var card = Instantiate(cardPrefab, transform.position, Quaternion.identity, transform.parent);

            hand[idx] = new HandCard(cardDrawn.Name, card);

            AnimateMovementTo(card, idx);

            Debug.Log($"Playing from {rectTransform} to {idx}");
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

        void MulliganSelected(DeckEvent ev) 
        {
            if (mulligansAvailable <= 0) 
            {
                return;
            }

            var cardSelected = ev as MulliganSelect;

            // This could be slow...
            for (var idx = 0; idx < hand.Length; idx++)
            {
                if (hand[idx] == null || hand[idx].Name != cardSelected.Name)
                {
                    continue;
                }

                var card = hand[idx];
                var mulliganPosition = mulliganSelected.IndexOf(card);

                if (mulliganPosition != -1) {
                    mulliganSelected.RemoveAt(mulliganPosition);
                    card.CardObject.GetComponent<CardBehaviour>().MulliganSelected = false;
                }
                else if (mulliganSelected.Count < deck.MaxMulliganSize)
                {
                    mulliganSelected.Add(card);
                    card.CardObject.GetComponent<CardBehaviour>().MulliganSelected = true;
                }

                break;
            }
        }

        void MulliganTaken(DeckEvent ev) 
        {
            if (mulligansAvailable <= 0 || mulliganSelected.Count <= 0) 
            {
                return;
            }

            for (int i = mulliganSelected.Count - 1; i >= 0; i--)
            {
                var handCard = mulliganSelected[i];
                
                LocalDeck.Instance.DiscardCard(new Card(handCard.Name));
                mulliganSelected.RemoveAt(i);
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

            mulligansAvailable = 0;
            DisableMulliganUI();
        }

        void OnDisable()
        {
            foreach (var graph in graphs)
            {
                if (graph.IsValid()) graph.Destroy();
            }
        }

        void DisableMulliganUI()
        {
            if (MulliganTakeObject != null) MulliganTakeObject.GetComponent<Button>().interactable = false;
            if (MulliganRejectObject != null) MulliganRejectObject.GetComponent<Button>().interactable = false;
        }

    }
}