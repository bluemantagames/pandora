using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;
using Pandora.Deck.Event;
using Pandora.Events;
using System.Linq;

namespace Pandora.Deck
{
    public class HandBehaviour : MonoBehaviour
    {
        Animator animator;

        PlayableGraph graph = new PlayableGraph();

        Vector2 originalPosition;
        RectTransform rectTransform;

        Card[] hand = new Card[32];

        int handIndex = -1;

        public GameObject[] UIHandSlots;
        public float EaseOutTime = 0.35f;
        Queue<CardDrawn> events = new Queue<CardDrawn> {};

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();

            var cardNames = new List<string> {
                "Bard",
                "Ranger",
                "Clerics",
                "Cockatrice",
                "Fireball",
                "Harpies",
                "Mermaids",
                "Troll"
            };

            var cards =
                from card in cardNames
                select new Card(card);

            var deck = LocalDeck.Instance;

            deck.EventBus.Subscribe<CardDrawn>(new EventSubscriber<DeckEvent>(CardDrawn, "HandDrawHandler"));

            deck.Deck = cards.ToList();
        }

        bool IsPlaying() {
            return graph.IsValid() && graph.GetRootPlayable(0).GetTime() < EaseOutTime;
        }

        void Update()
        {
            if (!IsPlaying() && events.Count > 0)
            {
                CardDrawn(events.Dequeue());
            }
        }

        void CardDrawn(DeckEvent ev)
        {
            var cardDrawn = ev as CardDrawn;

            Debug.Log($"Received {cardDrawn}");

            if (IsPlaying()) // queue events if an animation is being played
            {
                events.Enqueue(cardDrawn);

                return;
            }

            if (handIndex + 1 > LocalDeck.Instance.HandSize - 1)
            {
                // ShiftPositions() - shift cards as much to the right as possible
            }
            else
            {
                handIndex++;
            }


            Debug.Log($"Drawing card {handIndex}");

            var cardPrefab = Resources.Load($"Cards/{cardDrawn.Name}") as GameObject;
            var card = Instantiate(cardPrefab, transform.position, Quaternion.identity, transform.parent);

            hand[handIndex] = new Card(cardDrawn.Name);

            var cardTransform = card.GetComponent<RectTransform>();

            var targetRectTransform = UIHandSlots[handIndex].GetComponent<RectTransform>();

            cardTransform.pivot = targetRectTransform.pivot;

            var xCurve = AnimationCurve.EaseInOut(0f, cardTransform.anchoredPosition.x, EaseOutTime, targetRectTransform.anchoredPosition.x);
            var yCurve = AnimationCurve.EaseInOut(0f, cardTransform.anchoredPosition.y, EaseOutTime, targetRectTransform.anchoredPosition.y);

            var clip = new AnimationClip();

            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", xCurve);
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", yCurve);

            AnimationPlayableUtilities.PlayClip(card.GetComponent<Animator>(), clip, out graph);

            Debug.Log($"Playing from {rectTransform} to {targetRectTransform}");
        }

        void OnDisable()
        {
            if (graph.IsValid()) graph.Destroy();
        }

    }
}