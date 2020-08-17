namespace Pandora.Resource.Gold
{
    public class GoldEvent {}

    public class GoldEarned: GoldEvent {
        /// <description>Amount earned</description>
        public int AmountEarned;

        /// <description>Amount of gold after earnings are added in</description>
        public int CurrentAmount;

        public GoldEarned(int currentAmount, int amountEarned) {
            CurrentAmount = currentAmount;
            AmountEarned = amountEarned;
        }
    }

    public class GoldSpent: GoldEvent {
        /// <description>Amount spent</description>
        public int AmountSpent;
        /// <description>Amount of gold after earnings are added in</description>
        public int CurrentAmount;

        public GoldSpent(int currentAmount, int amountSpent) {
            CurrentAmount = currentAmount;
            AmountSpent = amountSpent;
        }
    }
}