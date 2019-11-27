using System.Collections.Generic;

namespace Pandora.Engine {
    public class Collision {
        public EngineEntity First, Second;
        public BoxBounds FirstBox, SecondBox;

        public Collision(EngineEntity first, EngineEntity second, BoxBounds firstBox, BoxBounds secondBox) {
            First = first;
            Second = second;

            FirstBox = firstBox;
            SecondBox = secondBox;
        }

        public Collision() {}

        public override int GetHashCode()
        {
            var hashCode = 887775774;

            var first = (First.Timestamp > Second.Timestamp) ? First : Second;
            var second = (first == First) ? Second : First;

            hashCode = hashCode * -1521134295 + EqualityComparer<EngineEntity>.Default.GetHashCode(first);
            hashCode = hashCode * -1521134295 + EqualityComparer<EngineEntity>.Default.GetHashCode(second);

            return hashCode;
        }
        
        public override string ToString() =>
            $"Collision({First}, {Second})";
    }

}