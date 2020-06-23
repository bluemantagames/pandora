using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Deck.UI
{
    public class MenuCardsParentBehaviour : MonoBehaviour
    {
        void Awake()
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<CardBehaviour>().IsDeckBuilderUI = true;
            }
        }

        public MenuCardBehaviour FindCard(string cardName) =>
            GetComponentsInChildren<MenuCardBehaviour>()
                .ToList()
                .Find(c => c.CardName == cardName && !c.UiDisabled);

        public void Reset()
        {
            foreach (Transform child in transform)
            {
                var menuCardBehaviour = child.GetComponent<MenuCardBehaviour>();

                if (menuCardBehaviour != null)
                {
                    menuCardBehaviour.Reset();
                }
            }
        }
    }

}