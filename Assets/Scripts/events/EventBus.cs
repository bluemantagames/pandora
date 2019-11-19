using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Events {
    /// <summary>
    /// Very barbone and basic event bus implementation. Subscribers can subscribe and unsubscribe
    /// and dispatchers can dispatch events at will. This implementation is completely synchronous,
    /// meaning it will block dispatchers until every subscriber has finished executing.
    /// 
    /// Subscribers are also asked to assign a unique id to their subscription (that they also use
    /// when wanting to unsubscribe) to ensure determinism when dispatching events (i.e. events are dispatched in the same order 
    /// in all clients)
    /// </summary>
    public class EventBus<T> {
        Dictionary<Type, List<EventSubscriber<T>>> subscribers = new Dictionary<Type, List<EventSubscriber<T>>> {};

        /// <summary>Subscribes to events of type `A`</summary>
        public void Subscribe<A>(EventSubscriber<T> subscriber) {
            var eventType = typeof(A);

            var subscriberList = subscribers.ContainsKey(eventType) ? subscribers[eventType] : new List<EventSubscriber<T>> {};

            subscriberList.Add(subscriber);
            subscriberList.Sort();

            subscribers[eventType] = subscriberList;
        }


        /// <summary>Unsubscribes from events of type `A`</summary>
        public void Unsubscribe<A>(string subscriberId) {
            var eventType = typeof(A);

            subscribers[eventType]?.RemoveAll(subscriber => subscriber.Id == subscriberId);
            subscribers[eventType]?.Sort();
        }

        /// <summary>
        /// Dispatches an event in this eventbus
        ///
        /// BEWARE: This method is synchronous, meaning it will block until all subscribers
        /// have handled this event
        /// </summary>
        public void Dispatch(T ev) {
            if (subscribers.ContainsKey(ev.GetType())) {
                foreach (var subscriber in subscribers[ev.GetType()]) {
                    subscriber.Action(ev);
                }
            } else {
                Debug.LogWarning($"No subscribers found for {ev}");
            }
        }

    }

}