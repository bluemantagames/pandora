namespace Pandora
{
    public enum TowerPosition { TopLeft, TopRight, TopMiddle, BottomLeft, BottomRight, BottomMiddle };

    public static class TowerPositionMethods
    {
        public static bool IsTop(this TowerPosition position)
        {
            return position == TowerPosition.TopLeft || position == TowerPosition.TopRight || position == TowerPosition.TopMiddle;
        }

        public static bool IsBottom(this TowerPosition position)
        {
            return !position.IsTop();
        }

        public static TowerPosition Flip(this TowerPosition position) {
            if (position == TowerPosition.TopLeft) {
                return TowerPosition.BottomLeft;
            } else if (position == TowerPosition.TopRight) {
                return TowerPosition.BottomRight;
            } else if (position == TowerPosition.TopMiddle) {
                return TowerPosition.BottomMiddle;
            } else if (position == TowerPosition.BottomLeft) {
                return TowerPosition.TopLeft;
            } else if (position == TowerPosition.BottomRight) {
                return TowerPosition.TopRight;
            } else {
                return TowerPosition.TopMiddle;
            }
        }
    }
}