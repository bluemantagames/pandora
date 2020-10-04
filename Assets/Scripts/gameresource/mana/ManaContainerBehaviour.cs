using UnityEngine;
using Pandora.Events;

namespace Pandora.Resource.Mana
{
    public class ManaContainerBehaviour : MonoBehaviour
    {
        void Start()
        {
            var walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            walletsComponent.ManaWallet.Bus.Subscribe<ManaEarned>(new EventSubscriber<ManaEvent>(OnManaEarned, "UIManaEarned"));
        }

        void OnManaEarned(ManaEvent manaEvent) {
            var manaEarned = manaEvent as ManaEarned;
        }

        void OnManaSpent(ManaEvent manaEvent) {
            var manaEarned = manaEvent as ManaSpent;
        }

        void UpdateManaUI(int currentMana) {
            int manaIndex = (currentMana % 10) - 1;
            int unitPercent = currentMana - ((manaIndex + 1) * 10);

            if (manaIndex < 0) return;
        }
    }
}