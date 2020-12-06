using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pandora;
using Pandora.Network;
using Pandora.Resource;
using Pandora.UI.Modal;
using Pandora.Events;

namespace Pandora.Resource.Gold.Shop
{
    public class RewardButtonBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public string RewardId;
        public int GoldCost = 12;

        WalletsComponent wallets;
        Button button;

        void Start()
        {
            button = GetComponent<Button>();
            wallets = MapComponent.Instance.GetComponent<WalletsComponent>();

            wallets.GoldWallet.Bus.Subscribe<GoldEarned>(new EventSubscriber<GoldEvent>(ProcessEvent, RewardId));
            wallets.GoldWallet.Bus.Subscribe<GoldSpent>(new EventSubscriber<GoldEvent>(ProcessEvent, RewardId));

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(OnMatchStart);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (wallets.GoldWallet.Resource >= GoldCost || !NetworkControllerSingleton.instance.matchStarted)
            {
                wallets.GoldWallet.SpendResource(GoldCost);

                MapComponent.Instance.ApplyGoldReward(RewardId, GoldCost);
            }
        }

        void OnMatchStart() {
            button.interactable = false;
        }

        void ProcessEvent(GoldEvent ev)
        {
            if (ev.CurrentAmount < GoldCost && NetworkControllerSingleton.instance.matchStarted)
                button.interactable = false;
            else if (!button.interactable)
                button.interactable = true;
        }
    }

}