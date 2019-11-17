using System;

namespace Pandora.Events
{
    public class EventSubscriber<T> : IComparable<EventSubscriber<T>>
    {
        public Action<T> Action;
        public string Id;

        public EventSubscriber(Action<T> action, string subscriberId)
        {
            Action = action;
            Id = subscriberId;
        }

        public int CompareTo(EventSubscriber<T> other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }
}