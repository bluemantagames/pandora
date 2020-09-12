
using UnityEngine;
using Pandora.Events;
using System;
using Pandora.Resource.Gold;

namespace Pandora.Resource
{
    public class WalletsComponent : MonoBehaviour
    {
        public ResourceWallet<GoldEvent> GoldWallet = new ResourceWallet<GoldEvent>(
            (resource, amount) => new GoldEarned(resource, amount),
            (resource, amount) => new GoldSpent(resource, amount),
            25
        );
    }
}