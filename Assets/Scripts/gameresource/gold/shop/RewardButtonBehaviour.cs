using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pandora;
using Pandora.Network;
using Pandora.Resource;
using Pandora.UI.Modal;

namespace Pandora.Resource.Gold.Shop
{
    public class RewardButtonBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public string RewardId;
        public int GoldCost = 12;

        WalletsComponent wallets;

        void Start()
        {
            wallets = MapComponent.Instance.GetComponent<WalletsComponent>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (wallets.GoldWallet.Resource >= GoldCost || !NetworkControllerSingleton.instance.matchStarted)
            {
                wallets.GoldWallet.SpendResource(GoldCost);

                MapComponent.Instance.ApplyGoldReward(RewardId, GoldCost);
            }
        }
    }

}