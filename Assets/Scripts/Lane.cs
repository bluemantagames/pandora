namespace Pandora {
    public enum Lane {
        Left, Right
    }

    public static class Extensions
    {
        public static int GridXPosition(this Lane lane)
        {
            return (lane == Lane.Left) ? 2 : 13;
        }
    }
}