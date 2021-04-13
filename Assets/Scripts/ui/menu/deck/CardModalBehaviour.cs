using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using Cysharp.Threading.Tasks;

namespace Pandora.UI.Menu.Deck
{
    public class CardModalBehaviour : MonoBehaviour
    {
        public CardBehaviour CurrentCardBehaviour;
        public LocalizedString HPString, DamageString, MovementSpeedString;
        public LocalizedString HPLowString, HPMediumString, HPHighString;
        public LocalizedString DamageLowString, DamageMediumString, DamageHighString;
        public LocalizedString MovementSpeedLowString, MovementSpeedMediumString, MovementSpeedHighString;
        public float HPMediumThreshold, HPHighThreshold;
        public float DamageMediumThreshold, DamageHighThreshold;
        public float MovementSpeedMediumThreshold, MovementSpeedHighThreshold;

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

        object buildArguments(float amount, string label) {
            return new { amount = amount.ToString(), label = label };
        }

        private async UniTaskVoid LoadSkillDescription()
        {
            if (CurrentCardBehaviour == null) return;

            var cardSkillDescription = GetComponentInChildren<CardSkillDescriptionBehaviour>()?.gameObject;
            var cardSkillDescriptionText = cardSkillDescription?.GetComponent<Text>();

            var description = await CurrentCardBehaviour.LocalizedCardSkillDescription.GetLocalizedString();


            var HPLabel = 
                (CurrentCardBehaviour.HP > HPMediumThreshold) ?
                    (CurrentCardBehaviour.HP > HPHighThreshold) ? HPHighString :
                        HPMediumString : HPLowString;

            var hpLabelString = await HPLabel.GetLocalizedString();

            var hpString = await HPString.GetLocalizedString(buildArguments(CurrentCardBehaviour.HP, hpLabelString));

            var damageLabel = 
                (CurrentCardBehaviour.Damage > DamageMediumThreshold) ?
                    (CurrentCardBehaviour.Damage > DamageHighThreshold) ? DamageHighString :
                        DamageMediumString : DamageLowString;

            var damageLabelString = await damageLabel.GetLocalizedString();

            var damageString = await DamageString.GetLocalizedString(buildArguments(CurrentCardBehaviour.Damage, damageLabelString));

            var movementSpeedLabel = 
                (CurrentCardBehaviour.MovementSpeed > MovementSpeedMediumThreshold) ?
                    (CurrentCardBehaviour.MovementSpeed > MovementSpeedHighThreshold) ? MovementSpeedHighString :
                        MovementSpeedMediumString : MovementSpeedLowString;

            var movementSpeedLabelString = await movementSpeedLabel.GetLocalizedString();

            var movementSpeedString = await MovementSpeedString.GetLocalizedString(buildArguments(CurrentCardBehaviour.MovementSpeed, movementSpeedLabelString));

            if (cardSkillDescriptionText)
                cardSkillDescriptionText.text = description + "\n\n" + 
                    hpString + "\n" +
                    damageString + "\n" +
                    movementSpeedString + "\n";
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