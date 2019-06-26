using System;
using System.Collections.Generic;
using UnityEngine;

struct QueueItem
{
    public List<Vector2> points;
    public HashSet<Vector2> pointsSet;

    public QueueItem(List<Vector2> _points, HashSet<Vector2> _pointsSet)
    {
        points = _points;
        pointsSet = _pointsSet;
    }

    public override string ToString()
    {
        return string.Join(",", points);
    }
}
