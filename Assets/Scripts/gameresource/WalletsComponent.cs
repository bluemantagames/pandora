
using UnityEngine;
using Pandora.Events;
using System;
using Pandora.Resource.Gold;
using Pandora.Resource.Mana;

namespace Pandora.Resource
{
    public class WalletsComponent : MonoBehaviour
    {
        public ResourceWallet<GoldEvent> GoldWallet = new ResourceWallet<GoldEvent>(
            (resource, amount) => new GoldEarned(resource, amount),
            (resource, amount) => new GoldSpent(resource, amount),
            25
        );

        public ResourceWallet<ManaEvent> ManaWallet = new ResourceWallet<ManaEvent>(
            (resource, amount) => new ManaEarned(resource, amount),
            (resource, amount) => new ManaSpent(resource, amount),
            100
        );
    }
}