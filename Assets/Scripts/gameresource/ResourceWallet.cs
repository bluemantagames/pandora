using UnityEngine;
using Pandora.Events;
using System;

namespace Pandora.Resource
{
    public class ResourceWallet<T>
    {
        private int _resource = 0;
        private Func<int, int, T> resourceEarnedEventFactory;
        private Func<int, int, T> resourceLostEventFactory;

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
            Func<int, int, T> resourceLostEventFactory
        )
        {
            this.resourceEarnedEventFactory = resourceEarnedEventFactory;
            this.resourceLostEventFactory = resourceLostEventFactory;

            Bus = new EventBus<T>();
        }

        public void AddResource(int amount)
        {
            _resource += amount;

            var ev = resourceEarnedEventFactory(_resource, amount);

            Bus.Dispatch(ev);
        }
    }

}