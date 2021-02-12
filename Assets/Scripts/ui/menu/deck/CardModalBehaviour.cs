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

        private async UniTaskVoid LoadCardName()
        {
            if (CurrentCardBehaviour == null) return;

            var cardName = GetComponentInChildren<CardNameBehaviour>()?.gameObject;
            var cardNameText = cardName?.GetComponent<Text>();

            var name = await CurrentCardBehaviour.LocalizedCardName.GetLocalizedString();

            if (cardNameText)
                cardNameText.text = name;
        }

        private async UniTaskVoid LoadSkillDescription()
        {
            if (CurrentCardBehaviour == null) return;

            var cardSkillDescription = GetComponentInChildren<CardSkillDescriptionBehaviour>()?.gameObject;
            var cardSkillDescriptionText = cardSkillDescription?.GetComponent<Text>();

            var description = await CurrentCardBehaviour.LocalizedCardSkillDescription.GetLocalizedString();

            if (cardSkillDescriptionText)
                cardSkillDescriptionText.text = description;
        }

        public void LoadInfo()
        {
            if (CurrentCardBehaviour == null) return;

            var cardSplash = GetComponentInChildren<CardImageBehaviour>()?.gameObject;
            var cardSplashImage = cardSplash?.GetComponent<RawImage>();

            if (cardSplashImage)
                cardSplashImage.texture = CurrentCardBehaviour.CardMainImage;

            _ = LoadCardName();
            _ = LoadSkillDescription();
        }
    }

}