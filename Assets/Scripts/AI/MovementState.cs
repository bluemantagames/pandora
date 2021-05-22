namespace Pandora.AI
{
    public class MovementState
    {
        public MovementStateEnum state;
        public Enemy enemy; // might be null

        public MovementState(Enemy enemy, MovementStateEnum state)
        {
            this.enemy = enemy;
            this.state = state;
        }

        override public string ToString()
        {
            return $"MovementState enemy = {enemy}, state = {state}";
        }
    }
}