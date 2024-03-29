
using UnityEngine;
using Pandora.Events;
using System;
using Pandora.Resource.Gold;
using Pandora.Resource.Mana;

namespace Pandora.Resource
{
    public class WalletsComponent : MonoBehaviour
    {
        ResourceWallet<GoldEvent> _goldWallet = null;

        ResourceWallet<ManaEvent> _manaWallet = null;

        ResourceWallet<ManaEvent> _enemyManaWallet = null;

        public ResourceWallet<GoldEvent> GoldWallet
        {
            get
            {
                if (_goldWallet == null)
                    _goldWallet = new ResourceWallet<GoldEvent>(
                        (resource, amount) => new GoldEarned(resource, amount),
                        (resource, amount) => new GoldSpent(resource, amount),
                        (resource, amount) => new GoldUpperReserve(resource, amount),
                        0,
                        25
                    );

                return _goldWallet;
            }
        }

        public ResourceWallet<ManaEvent> ManaWallet
        {
            get
            {
                if (_manaWallet == null)
                    _manaWallet = new ResourceWallet<ManaEvent>(
                        (resource, amount) => new ManaEarned(resource, amount),
                        (resource, amount) => new ManaSpent(resource, amount),
                        (resource, amount) => new ManaUpperReserve(resource, amount),
                        0,
                        100
                    );

                return _manaWallet;
            }
        }

        public ResourceWallet<ManaEvent> EnemyManaWallet
        {
            get
            {
                if (_enemyManaWallet == null)
                    _enemyManaWallet = new ResourceWallet<ManaEvent>(
                        (resource, amount) => new EnemyManaEarned(resource, amount),
                        (resource, amount) => new EnemyManaSpent(resource, amount),
                        (resource, amount) => new EnemyManaUpperReserve(resource, amount),
                        0,
                        100
                    );

                return _enemyManaWallet;
            }
        }

        public void ResetWallets()
        {
            _goldWallet = null;
            _manaWallet = null;
            _enemyManaWallet = null;
        }
    }
}