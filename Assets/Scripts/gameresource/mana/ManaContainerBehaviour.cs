using UnityEngine;
using Pandora.Events;

namespace Pandora.Resource.Mana
{
    public class ManaContainerBehaviour : MonoBehaviour
    {
        WalletsComponent walletsComponent;

        void Start()
        {
            walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            walletsComponent.ManaWallet.Bus.Subscribe<ManaEarned>(new EventSubscriber<ManaEvent>(OnManaEarned, "UIManaEarned"));
            walletsComponent.ManaWallet.Bus.Subscribe<ManaSpent>(new EventSubscriber<ManaEvent>(OnManaSpent, "UIManaSpent"));
        }

        void OnManaEarned(ManaEvent manaEvent)
        {
            var manaEarned = manaEvent as ManaEarned;

            UpdateManaUI(manaEarned.CurrentAmount);
        }

        void OnManaSpent(ManaEvent manaEvent)
        {
            var manaEarned = manaEvent as ManaSpent;

            UpdateManaUI(manaEarned.CurrentAmount);
        }

        void UpdateManaUI(int currentMana)
        {
            int manaIndex = currentMana / 10;
            int unitPercent = currentMana - (manaIndex * 10);

            if (manaIndex < 0) return;

            var childMask = transform.GetChild(manaIndex).GetComponentInChildren<ManaMaskComponent>();

            var percent = (float)unitPercent / 10;

            Logger.Debug($"Setting percent as {percent}");

            childMask.SetPercent(percent);

            var prevIndex = manaIndex - 1;

            if (prevIndex > 0)
                transform.GetChild(prevIndex).GetComponentInChildren<ManaMaskComponent>().SetPercent(1f);
        }
    }
}