using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.UI.Menu.Deck
{
    public class CardModalBehaviour : MonoBehaviour
    {
        public CardBehaviour CurrentCardBehaviour;

        void Awake()
        {

        }

        public void LoadInfo()
        {
            if (CurrentCardBehaviour == null) return;

            var cardName = GetComponentInChildren<CardNameBehaviour>()?.gameObject;
            var cardNameText = cardName?.GetComponent<Text>();

            var cardSplash = GetComponentInChildren<CardImageBehaviour>()?.gameObject;
            var cardSplashImage = cardSplash?.GetComponent<RawImage>();

            if (cardNameText)
                cardNameText.text = CurrentCardBehaviour.UnitName;

            if (cardSplashImage)
                cardSplashImage.texture = CurrentCardBehaviour.CardMainImage;
        }
    }

}