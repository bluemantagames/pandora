using UnityEngine;
using Pandora.Events;
using System;
using System.Linq;
using System.Collections.Generic;

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
        public Dictionary<string, int> UpperReserve { get; private set; } = new Dictionary<string, int>();

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
            if (ResourceUpperCap.HasValue)
            {
                var upperReserve = GetCurrentUpperReserve();
                var upperCap = ResourceUpperCap.Value - upperReserve;

                Logger.Debug($"[MANA] Earning amount {amount} with an uppercap of {upperCap}");

                if (_resource + amount > upperCap)
                    amount = upperCap - _resource;
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

        public void AddUpperReserve(string id, int amount)
        {
            UpperReserve.Add(id, amount);

            if (ResourceUpperCap.HasValue)
            {
                var upperReserve = GetCurrentUpperReserve();
                var upperCap = ResourceUpperCap.Value - upperReserve;

                if (_resource > upperCap)
                    _resource = upperCap;
            }

            var ev = setUpperReserveEventFactory(_resource, amount);

            Bus.Dispatch(ev);
        }

        public void RemoveUpperReserve(string id)
        {
            UpperReserve.Remove(id);

            var upperReserve = GetCurrentUpperReserve();

            var ev = setUpperReserveEventFactory(_resource, upperReserve);

            Bus.Dispatch(ev);
        }

        int GetCurrentUpperReserve()
        {
            return UpperReserve.Values.Sum();
        }
    }

}