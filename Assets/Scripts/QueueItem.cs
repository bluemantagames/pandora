namespace Pandora
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public struct QueueItem<T>
    {
        public List<T> points;
        public HashSet<T> pointsSet;

        public QueueItem(List<T> _points, HashSet<T> _pointsSet)
        {
            points = _points;
            pointsSet = _pointsSet;
        }

        public override string ToString()
        {
            return string.Join(",", points);
        }
    }
}