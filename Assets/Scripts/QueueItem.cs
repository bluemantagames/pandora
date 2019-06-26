using System;
using System.Collections.Generic;
using UnityEngine;

public class QueueItem<T> : IComparable<QueueItem<T>>
{
    public float priority;
    public T item;

    public QueueItem(T _item, float _priority)
    {
        priority = _priority;
        item = _item;
    }

    public override string ToString()
    {
        var items = item as List<Vector2>;

        return $"QueueItem: Priority: {priority}, Item: {string.Join(",", items)}";
    }

    public int CompareTo(QueueItem<T> other)
    {
        return priority.CompareTo(other.priority);
    }
}
