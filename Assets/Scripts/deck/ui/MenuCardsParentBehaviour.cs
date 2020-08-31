using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Deck.UI
{
    public class MenuCardsParentBehaviour : MonoBehaviour
    {
        void Awake()
        {
            var cards = gameObject.GetComponentsInChildren<CardBehaviour>();

            foreach (var cardBehaviour in cards)
            {
                cardBehaviour.IsDeckBuilderUI = true;
            }
        }

        public MenuCardBehaviour FindCard(string cardName) =>
            GetComponentsInChildren<MenuCardBehaviour>()
                .ToList()
                .Find(c => c.CardName == cardName && !c.UiDisabled);
    }

}