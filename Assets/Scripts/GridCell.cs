using UnityEngine;
using System;

namespace Pandora
{
    public class GridCell : IEquatable<GridCell>
    {
        public Vector2Int vector;

        public GridCell(Vector2Int vector)
        {
            this.vector = vector;
        }
        public GridCell(int x, int y)
        {
            this.vector = new Vector2Int(x, y);
        }

        public bool Equals(GridCell other)
        {
            return this.vector == other?.vector;
        }

        private static bool isNull(object a) => object.Equals(a, null);

        public static bool operator ==(GridCell lhs, GridCell rhs)
        {
            return isNull(lhs) ? isNull(rhs) : lhs.Equals(rhs);
        }

        public static bool operator !=(GridCell lhs, GridCell rhs)
        {
            return !lhs.Equals(rhs);
        }

        override public int GetHashCode()
        {
            return this.vector.GetHashCode();
        }

        override public string ToString()
        {
            return $"GridCell({this.vector.ToString()})";
        }

        override public bool Equals(object obj)
        {
            if (obj is GridCell)
            {
                return ((GridCell)obj) == this;
            }
            else
            {
                return false;
            }
        }
    }
}