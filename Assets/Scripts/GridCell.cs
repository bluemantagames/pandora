using UnityEngine;
using System;

namespace CRclone
{
    public struct GridCell : IEquatable<GridCell>
    {
        public Vector2 vector;

        public GridCell(Vector2 vector)
        {
            this.vector = vector;
        }
        public GridCell(float x, float y)
        {
            this.vector = new Vector2(x, y);
        }

        public bool Equals(GridCell other)
        {
            return this.vector == other.vector;
        }

        public static bool operator ==(GridCell lhs, GridCell rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GridCell lhs, GridCell rhs)
        {
            return !lhs.Equals(rhs);
        }

        override public int GetHashCode()
        {
            return this.vector.GetHashCode();
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