using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Pandora.UI.Menu.Deck
{
    public class CardModalBehaviour : MonoBehaviour
    {
        public CardBehaviour CurrentCardBehaviour;

        void Awake()
        {

        }

        private async UniTaskVoid LoadSkillDescription()
        {
            if (CurrentCardBehaviour == null) return;

            var cardSkillDescription = GetComponentInChildren<CardSkillDescriptionBehaviour>()?.gameObject;
            var cardSkillDescriptionText = cardSkillDescription?.GetComponent<Text>();

            var description = await CurrentCardBehaviour.CardSkillDescription.GetLocalizedString();

            if (cardSkillDescriptionText)
                cardSkillDescriptionText.text = description;
        }

        public void LoadInfo()
        {
            if (CurrentCardBehaviour == null) return;

            var cardName = GetComponentInChildren<CardNameBehaviour>()?.gameObject;
            var cardNameText = cardName?.GetComponent<Text>();

            var cardSplash = GetComponentInChildren<CardImageBehaviour>()?.gameObject;
            var cardSplashImage = cardSplash?.GetComponent<RawImage>();



            if (cardNameText)
                cardNameText.text = CurrentCardBehaviour.CardName;

            if (cardSplashImage)
                cardSplashImage.texture = CurrentCardBehaviour.CardMainImage;

            _ = LoadSkillDescription();
        }
    }

}