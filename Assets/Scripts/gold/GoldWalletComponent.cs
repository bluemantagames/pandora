using UnityEngine;
using Pandora.Events;

namespace Pandora.Gold
{
    public class GoldWalletComponent : MonoBehaviour
    {
        private int _gold = 0;

        public int Gold
        {
            get => _gold;

            private set => _gold = value;
        }


        public EventBus<GoldEvent> Bus;

        void Start()
        {
            Bus = new EventBus<GoldEvent>();
        }

        public void AddGold(int amount)
        {
            _gold += amount;

            var ev = new GoldEarned(amount, _gold);

            Bus.Dispatch(ev);
        }
    }

}