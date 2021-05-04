using UnityEngine;
using Pandora.Events;
using System;

namespace Pandora.Resource
{
    public class ResourceWallet<T>
    {
        private int _resource;
        private Func<int, int, T> resourceEarnedEventFactory;
        private Func<int, int, T> resourceLostEventFactory;

        public int? ResourceUpperCap { get; private set; }
        public int? ResourceLowerCap { get; private set; }

        public int Resource
        {
            get => _resource;

            private set => _resource = value;
        }


        public EventBus<T> Bus;

        /// <summary>
        /// Creates a wallet that dispatches events created by the factory passed as arguments.false
        /// 
        /// The first argument of the factory is always the current amount of resource after the event happened 
        /// (e.g. after a sum of gold is earned)
        /// </summary>
        public ResourceWallet(
            Func<int, int, T> resourceEarnedEventFactory,
            Func<int, int, T> resourceLostEventFactory,
            int? lowerCap,
            int? upperCap
        )
        {
            this.resourceEarnedEventFactory = resourceEarnedEventFactory;
            this.resourceLostEventFactory = resourceLostEventFactory;

            ResourceUpperCap = upperCap;
            ResourceLowerCap = lowerCap;

            _resource = lowerCap != null ? lowerCap.Value : 0;

            Bus = new EventBus<T>();
        }

        public void AddResource(int amount)
        {
            if (ResourceUpperCap != null && Resource + amount > ResourceUpperCap.Value)
            {
                amount = ResourceUpperCap.Value - Resource;
            }

            _resource += amount;

            var ev = resourceEarnedEventFactory(_resource, amount);

            Bus.Dispatch(ev);
        }

        public void SpendResource(int amount)
        {
            _resource -= amount;

            var ev = resourceLostEventFactory(_resource, amount);

            Bus.Dispatch(ev);
        }

    }

}