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

        public bool Contains(Vector2Int point)
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
                Contains(box.UpperLeft)  ||
                Contains(box.UpperRight) ||
                Contains(box.LowerLeft)  ||
                Contains(box.LowerRight) ||
                box.Contains(UpperLeft)  ||
                box.Contains(UpperRight) ||
                box.Contains(LowerLeft)  ||
                box.Contains(LowerRight);
        }

        public void Translate(Vector2Int center) {
            var halfWidth = Width / 2;
            var halfHeight = Height / 2;

            Center = center;

            UpperRight = Center + new Vector2Int(halfWidth, halfHeight);
            UpperLeft = Center + new Vector2Int(-halfWidth, halfHeight);
            LowerRight = Center + new Vector2Int(halfWidth, -halfHeight);
            LowerLeft = Center + new Vector2Int(-halfWidth, -halfHeight);
        }

        public BoxBounds Clear() {
            return this;
        }

        override public string ToString() {
            return $"UpperLeft({UpperLeft}), UpperRight({UpperRight}), LowerLeft({LowerLeft}), LowerRight({LowerRight})";
        }
    }
}