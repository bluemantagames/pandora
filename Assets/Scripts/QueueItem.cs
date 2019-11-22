namespace Pandora
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Priority_Queue;

    public class QueueItem<T>: FastPriorityQueueNode
    {
        public T Item;

        public QueueItem() {}

        public QueueItem(T item) {
            this.Item = item;
        }

        public override string ToString()
        {
            return Item.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 1156408058;

            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Item);

            return hashCode;
        }
    }
}