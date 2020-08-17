using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Events;

namespace Pandora.Resource.Gold
{
    public class GoldTextBehaviour : MonoBehaviour
    {
        WalletsComponent walletsComponent;

        void Start()
        {
            walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            walletsComponent.GoldWallet.Bus.Subscribe<GoldEarned>(new EventSubscriber<GoldEvent>(ProcessEvent, "GoldEarnedTextBehaviour"));
            walletsComponent.GoldWallet.Bus.Subscribe<GoldSpent>(new EventSubscriber<GoldEvent>(ProcessEvent, "GoldSpentTextBehaviour"));
        }

        void ProcessEvent(GoldEvent ev)
        {
            GetComponent<Text>().text = walletsComponent.GoldWallet.Resource.ToString();
        }
    }
}