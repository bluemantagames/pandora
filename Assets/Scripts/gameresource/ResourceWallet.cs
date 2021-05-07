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
        private Func<int, int, T> setUpperReserveEventFactory;

        public int? ResourceUpperCap { get; private set; }
        public int? ResourceLowerCap { get; private set; }
        public int? UpperReserve { get; private set; }

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
            Func<int, int, T> setUpperReserveEventFactory,
            int? lowerCap,
            int? upperCap
        )
        {
            this.resourceEarnedEventFactory = resourceEarnedEventFactory;
            this.resourceLostEventFactory = resourceLostEventFactory;
            this.setUpperReserveEventFactory = setUpperReserveEventFactory;

            ResourceUpperCap = upperCap;
            ResourceLowerCap = lowerCap;

            _resource = lowerCap != null ? lowerCap.Value : 0;

            Bus = new EventBus<T>();
        }

        public void AddResource(int amount)
        {
            int? upperCap = ResourceUpperCap.HasValue && UpperReserve.HasValue
                ? Math.Min(ResourceUpperCap.Value, UpperReserve.Value)
                : ResourceUpperCap.HasValue
                ? ResourceUpperCap.Value
                : null;

            if (upperCap != null && Resource + amount > upperCap)
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

        public void SetUpperReserve(int amount)
        {
            UpperReserve = amount;

            var ev = setUpperReserveEventFactory(_resource, amount);

            Bus.Dispatch(ev);
        }
    }

}