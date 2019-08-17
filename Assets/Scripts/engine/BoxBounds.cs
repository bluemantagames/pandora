using UnityEngine;
using System;

namespace Pandora.Engine
{
    public class BoxBounds
    {
        public Vector2Int UpperLeft;
        public Vector2Int UpperRight;
        public Vector2Int LowerLeft;
        public Vector2Int LowerRight;
        public Vector2Int Center;

        public int Width {
            get {
                return Math.Abs(LowerRight.x - LowerLeft.x);
            }
        }

        public int Height {
            get {
                return Math.Abs(UpperRight.y - LowerRight.y);
            }
        }

        public bool IsContained(Vector2Int point)
        {
            return
                (point.x >= UpperLeft.x && point.y <= UpperLeft.y) &&
                (point.x <= UpperRight.x && point.y <= UpperRight.y) &&
                (point.x <= LowerRight.x && point.y >= LowerRight.y) &&
                (point.x >= LowerLeft.x && point.y >= LowerLeft.y);
        }

        public bool Collides(BoxBounds box)
        {
            return
                IsContained(box.UpperLeft)  ||
                IsContained(box.UpperRight) ||
                IsContained(box.LowerLeft)  ||
                IsContained(box.LowerRight) ||
                box.IsContained(UpperLeft)  ||
                box.IsContained(UpperRight) ||
                box.IsContained(LowerLeft)  ||
                box.IsContained(LowerRight);
        }

        override public string ToString() {
            return $"UpperLeft({UpperLeft}), UpperRight({UpperRight}), LowerLeft({LowerLeft}), LowerRight({LowerRight})";
        }
    }
}