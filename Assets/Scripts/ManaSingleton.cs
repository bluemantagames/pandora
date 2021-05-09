using System;
using Pandora.Resource;
using Pandora;
using Pandora.Resource.Mana;

namespace Pandora
{
    public class ManaSingleton
    {
        static ManaSingleton _instance = null;
        ResourceWallet<ManaEvent> manaWallet;
        ResourceWallet<ManaEvent> enemyManaWallet;

        public int ManaValue
        {
            get
            {
                return manaWallet.Resource;
            }
        }

        public int EnemyManaValue
        {
            get
            {
                return enemyManaWallet.Resource;
            }
        }

        public int MaxMana
        {
            get
            {
                return manaWallet.ResourceUpperCap.Value;
            }
        }

        public int MinMana
        {
            get
            {
                return manaWallet.ResourceLowerCap.Value;
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
            var walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            manaWallet = walletsComponent.ManaWallet;
            enemyManaWallet = walletsComponent.EnemyManaWallet;
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
            UpdateMana(newValue, manaWallet);
        }

        public void UpdateEnemyMana(int newValue)
        {
            UpdateMana(newValue, enemyManaWallet);
        }

        public void SetManaUpperReserve(string id, int amount)
        {
            manaWallet.SetUpperReserve(id, amount);
        }

        public void SetEnemyManaUpperReserve(string id, int amount)
        {
            enemyManaWallet.SetUpperReserve(id, amount);
        }
    }
}