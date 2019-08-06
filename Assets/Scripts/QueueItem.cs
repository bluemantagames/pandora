namespace CRclone
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    struct QueueItem
    {
        public List<GridCell> points;
        public HashSet<GridCell> pointsSet;

        public QueueItem(List<GridCell> _points, HashSet<GridCell> _pointsSet)
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