using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;
using Pandora.Deck.Event;
using Pandora.Events;
using Pandora.Network;
using UnityEngine.UI;
using System;
using System.Linq;
using Pandora.Engine;
using Pandora.Animations;
using Cysharp.Threading.Tasks;

namespace Pandora.Deck
{
    public class HandBehaviour : MonoBehaviour, EngineBehaviour
    {
        /// <summary>
        /// This is set before loading the GameScene
        ///
        /// It will be then loaded into the deck implementation by `Start()`
        ///</summary>
        public static List<Card> Deck;

        public string ComponentName {
            get => "HandBehaviour";
        }

        Animator animator;

        List<PlayableGraph> graphs = new List<PlayableGraph> { };

        Vector2 originalPosition;
        RectTransform rectTransform;

        HandCard[] hand = new HandCard[32];

        int handIndex = -1;

        public GameObject[] UIHandSlots;

        GameObject baseCard;

        public float EaseOutTime = 0.35f;

        Deck deck;

        public List<HandCard> SelectedCards = new List<HandCard> { };

        static private HandBehaviour _instance = null;

        static public HandBehaviour Instance {
            get => _instance;
        }

        // Mulligan stuff
        uint mulligansAvailable = 1;
        uint mulliganSecDuration = 20;
        public GameObject MulliganTakeObject;
        public GameObject MulliganRejectObject;
        public GameObject MulliganTimerText;
        private uint mulliganMsDuration;
        private uint mulliganTimePassed;
        private uint mulliganTimeLeft;

        void Start()
        {
            // Check if it's a replay and disable the hand
            if (ReplayControllerSingleton.instance.IsActive)
            {
                DisableMulliganUI();
                return;
            }

            baseCard = Resources.Load($"Cards/BaseCard") as GameObject;

            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();

            deck = LocalDeck.Instance;

            deck.EventBus.Subscribe<CardDrawn>(new EventSubscriber<DeckEvent>(CardDrawn, "HandDrawHandler"));
            deck.EventBus.Subscribe<CardPlayed>(new EventSubscriber<DeckEvent>(CardPlayed, "HandPlayHandler"));
            deck.EventBus.Subscribe<CardDiscarded>(new EventSubscriber<DeckEvent>(CardDiscarded, "HandDiscardHandler"));
            deck.EventBus.Subscribe<CardSelected>(new EventSubscriber<DeckEvent>(CardSelected, "MulliganSelectHandled"));
            deck.EventBus.Subscribe<MulliganTaken>(new EventSubscriber<DeckEvent>(MulliganTaken, "MulliganTakenHandled"));
            deck.EventBus.Subscribe<MulliganRejected>(new EventSubscriber<DeckEvent>(MulliganRejected, "MulliganRejectedHandled"));

            mulliganTimePassed = 0;
            mulliganMsDuration = mulliganSecDuration * 1000;

            _instance = this;

            // Assign this component to the engine
            var engineComponent = GetComponent<EngineComponent>();

            if (engineComponent != null) 
            {
                engineComponent.Engine.AddBehaviour(this);
            }

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
                        "Troll",
                        "Zombies",
                        "Ranger",
                        "Fireball",
                        "Bard",
                        "Clerics",
                        "Cockatrice",
                        "Mermaids",
                        "HalfOrc",
                        "Harpies"
                    };

                    cards =
                        (from card in cardNames
                         select new Card(card)).ToList();
                }
                else
                {
                    cards = Deck;
                }

                _ = SetDeck(cards);
            }
        }

        async UniTaskVoid SetDeck(List<Card> cards) {
            // Wait for 10 frames before setting the deck,
            // in order to give the layout component time to adjust children positions
            await UniTask.DelayFrame(10);

            if (deck is LocalDeck localDeck) {
                localDeck.Deck = cards;
            }
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (mulligansAvailable > 0)
            {
                mulliganTimePassed += timeLapsed;
                
                if (mulliganTimePassed % mulliganMsDuration <= 0) DisableMulligan();

                UpdateMulliganText();
            }
        }

        void AnimateMovementTo(GameObject card, int fromIdx, int idx)
        {
            var startTransform =
                (fromIdx < 0) ?
                    card.GetComponent<RectTransform>() : 
                    UIHandSlots[fromIdx].GetComponent<RectTransform>();

            var cardTransform = card.GetComponent<RectTransform>();

            var targetRectTransform = UIHandSlots[idx].GetComponent<RectTransform>();

            var targetPosition = targetRectTransform.position;

            var cardAnimation = card.GetComponent<CustomTransformAnimation>();

            cardTransform.pivot = targetRectTransform.pivot;

            var xCurve = AnimationCurve.EaseInOut(Time.time, startTransform.position.x, Time.time + EaseOutTime, targetPosition.x);
            var yCurve = AnimationCurve.EaseInOut(Time.time, startTransform.position.y, Time.time + EaseOutTime, targetPosition.y);

            Logger.Debug($"Animating {card} from ({startTransform.position.x}, {startTransform.position.y}) to {idx} ({startTransform.position.x}, {startTransform.position.y})");

            cardAnimation.SetCurves(xCurve, yCurve);
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
                        AnimateMovementTo(hand[i].CardObject, i, freePosition.Value);

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
            var card = Instantiate(cardPrefab, new Vector2(0, 0), Quaternion.identity, transform.parent);

            var slotRectTransform = UIHandSlots[idx].GetComponent<RectTransform>();

            card.GetComponent<RectTransform>().sizeDelta = slotRectTransform.sizeDelta;

            hand[idx] = new HandCard(cardDrawn.Name, card);

            AnimateMovementTo(card, -1, idx);

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
                var selectedIndex = SelectedCards.IndexOf(card);

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
                if (isMulligan && SelectedCards.Count < deck.MaxMulliganSize)
                {
                    Select(card);
                    break;
                }

                // If we are not in the Mulligan we just 
                // select cards individually
                if (!isMulligan)
                {
                    // Deselect the previous cards
                    for (var i = 0; i <= SelectedCards.Count - 1; i++)
                        Deselect(i);

                    Select(card);
                    break;
                }
            }
        }

        void MulliganTaken(DeckEvent ev)
        {
            if (mulligansAvailable <= 0 || SelectedCards.Count <= 0)
            {
                return;
            }

            for (int i = SelectedCards.Count - 1; i >= 0; i--)
            {
                var handCard = SelectedCards[i];

                LocalDeck.Instance.DiscardCard(new Card(handCard.Name));
                SelectedCards.RemoveAt(i);
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

        void Select(HandCard card)
        {
            SelectedCards.Add(card);
            card.CardObject.GetComponent<CardBehaviour>().MulliganSelected = true;
        }

        void Deselect(int index)
        {
            if (SelectedCards.ElementAt(index) == null) return;

            var combatBehaviour = SelectedCards[index]?.CardObject?.GetComponent<CardBehaviour>();

            if (combatBehaviour != null)
            {
                combatBehaviour.MulliganSelected = false;
            }

            SelectedCards.RemoveAt(index);
        }

        void UpdateMulliganText()
        {
            if (MulliganTimerText == null)
            {
                return;
            }
            
            var textComponent = MulliganTimerText.GetComponent<Text>();
            var timeLeft = mulliganMsDuration - mulliganTimePassed;

            var mulliganTimer = TimeSpan
                .FromMilliseconds(timeLeft)
                .ToString(@"mm\:ss\:ff");

            textComponent.text =
                $"Mulligan ends in {mulliganTimer} seconds";
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