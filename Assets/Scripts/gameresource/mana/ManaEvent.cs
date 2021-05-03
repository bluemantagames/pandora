namespace Pandora.Resource.Mana
{
    public class ManaEvent { }

    public class ManaEarned : ManaEvent
    {
        /// <description>Amount earned</description>
        public int AmountEarned;

        /// <description>Amount of mana after earnings are added in</description>
        public int CurrentAmount;

        public ManaEarned(int currentAmount, int amountEarned)
        {
            CurrentAmount = currentAmount;
            AmountEarned = amountEarned;
        }
    }

    public class EnemyManaEarned : ManaEarned
    {
        public EnemyManaEarned(int currentAmount, int amountEarned) : base(currentAmount, amountEarned) { }
    }

    public class ManaSpent : ManaEvent
    {
        /// <description>Amount spent</description>
        public int AmountSpent;
        /// <description>Amount of mana after earnings are added in</description>
        public int CurrentAmount;

        public ManaSpent(int currentAmount, int amountSpent)
        {
            CurrentAmount = currentAmount;
            AmountSpent = amountSpent;
        }
    }

    public class EnemyManaSpent : ManaSpent
    {
        public EnemyManaSpent(int currentAmount, int amountSpent) : base(currentAmount, amountSpent) { }
    }
}