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
    }

}