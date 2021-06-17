using System;
using Pandora.Resource;
using Pandora;
using Pandora.Resource.Mana;

namespace Pandora
{
    public class ManaSingleton
    {
        static ManaSingleton _instance = null;
        WalletsComponent walletsComponent;

        public int ManaValue
        {
            get
            {
                return walletsComponent.ManaWallet.Resource;
            }
        }

        public int ManaUpperReserve
        {
            get
            {
                return walletsComponent.ManaWallet.GetCurrentUpperReserve();
            }
        }

        public int EnemyManaValue
        {
            get
            {
                return walletsComponent.EnemyManaWallet.Resource;
            }
        }

        public int MaxMana
        {
            get
            {
                return walletsComponent.ManaWallet.ResourceUpperCap.Value;
            }
        }

        public int MinMana
        {
            get
            {
                return walletsComponent.ManaWallet.ResourceLowerCap.Value;
            }
        }

        // This is only used in dev
        public int ManaUnit { get; set; } = 0;

        static public ManaSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ManaSingleton();
                }

                return _instance;
            }
        }

        public ManaSingleton()
        {
            walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();
        }

        public void UpdateMana(int newValue, ResourceWallet<ManaEvent> manaWallet)
        {
            var difference = newValue - manaWallet.Resource;

            if (difference < 0)
                manaWallet.SpendResource(-difference);
            else
                manaWallet.AddResource(difference);
        }

        public void UpdateMana(int newValue)
        {
            UpdateMana(newValue, walletsComponent.ManaWallet);
        }

        public void UpdateEnemyMana(int newValue)
        {
            UpdateMana(newValue, walletsComponent.EnemyManaWallet);
        }

        public void SetManaUpperReserve(string id, int amount)
        {
            walletsComponent.ManaWallet.AddUpperReserve(id, amount);
        }

        public void RemoveManaUpperReserve(string id)
        {
            walletsComponent.ManaWallet.RemoveUpperReserve(id);
        }

        public void SetEnemyManaUpperReserve(string id, int amount)
        {
            walletsComponent.EnemyManaWallet.AddUpperReserve(id, amount);
        }

        public static void Reset()
        {
            _instance = null;
        }
    }
}