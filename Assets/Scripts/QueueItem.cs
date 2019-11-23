namespace Pandora
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Priority_Queue;

    public class QueueItem<T> where T: IEquatable<T>
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
            return Item.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is QueueItem<T> item) {
                return Item.Equals(item.Item);
            } else{
                return false;
            }
        }
    }
}