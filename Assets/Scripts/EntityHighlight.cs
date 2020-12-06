using UnityEngine;
using System;

namespace Pandora
{
    #pragma warning disable 661, 659
    public abstract class EntityHighlight : IEquatable<EntityHighlight>
    {
        public abstract bool Equals(EntityHighlight other);

        public static bool operator ==(EntityHighlight highlight1, EntityHighlight highlight2)
        {
            if (((object)highlight1) == null || ((object)highlight2) == null)
                return System.Object.Equals(highlight1, highlight2);

            return highlight1.Equals(highlight2);
        }

        public static bool operator !=(EntityHighlight highlight1, EntityHighlight highlight2) {
            return !(highlight1 == highlight2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
    #pragma warning restore 661, 659

    public class ColorHighlight : EntityHighlight
    {
        public Color Color;

        public ColorHighlight(Color color)
        {
            Color = color;
        }

        public override bool Equals(EntityHighlight other)
        {
            if (other == null || !(other is ColorHighlight))
                return false;

            return (other as ColorHighlight).Color == Color;
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

}