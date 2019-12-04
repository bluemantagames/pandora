using Pandora.Deck.Event;
using Pandora.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Deck
{
    public class LocalDeck : Deck
    {
        EventBus<DeckEvent> _eventBus = new EventBus<DeckEvent>();
        static LocalDeck _instance = null;
        bool mulliganAsked = false;

        Queue<Card> DeckQueue = new Queue<Card>();
        List<Card> _deck;

        public List<Card> Deck
        {
            get => _deck;

            set
            {
                _deck = value;

                var shuffledDeck = new List<Card>(value);
                var random = new System.Random();

                // Fisher-Yates shuffle: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
                for (var i = shuffledDeck.Count - 1; i >= 0; i--)
                {
                    var j = random.Next(shuffledDeck.Count - 1);
                    var iCard = shuffledDeck[i];

                    shuffledDeck[i] = shuffledDeck[j];
                    shuffledDeck[j] = iCard;
                }

                for (var idx = 0; idx < shuffledDeck.Count; idx++)
                {
                    if (idx < HandSize)
                    {
                        Debug.Log("Dispatching");

                        EventBus.Dispatch(new CardDrawn(shuffledDeck[idx].Name));
                    }
                    else
                    {
                        DeckQueue.Enqueue(shuffledDeck[idx]);
                    }
                }

                if (!mulliganAsked)
                {
                    mulliganAsked = true;

                    EventBus.Dispatch(new MulliganRequest());
                }

            }
        }

        public int HandSize { get => 4; }

        public int MaxMulliganSize { get => 2; }

        public EventBus<DeckEvent> EventBus
        {
            get => _eventBus;
        }

        static public LocalDeck Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalDeck();
                }

                return _instance;
            }
        }

        /// <summary>Reject mulligan</summary>
        public void RejectMulligan()
        {
            EventBus.Dispatch(new MulliganRejected());
        }

        /// <summary>Take mulligan</summary>
        public void TakeMulligan()
        {
            EventBus.Dispatch(new MulliganTaken());

            Deck = _deck;
        }

        public void PlayCard(Card card)
        {
            DeckQueue.Enqueue(card);

            EventBus.Dispatch(new CardPlayed(card.Name));
            DrawCard();
        }

        public void DrawCard()
        {
            EventBus.Dispatch(new CardDrawn(DeckQueue.Dequeue().Name));
        }

        public void MulliganSelect(Card card)
        {
            EventBus.Dispatch(new MulliganSelect(card.Name));
        }
    }
}