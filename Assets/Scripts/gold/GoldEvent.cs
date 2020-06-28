namespace Pandora.Gold
{
    public class GoldEvent {}

    public class GoldEarned: GoldEvent {
        /// <description>Amount earned</description>
        public int AmountEarned;

        /// <description>Amount of gold after earnings are added in</description>
        public int CurrentAmount;

        public GoldEarned(int amountEarned, int currentAmount) {
            AmountEarned = amountEarned;
            CurrentAmount = currentAmount;
        }
    }

    public class GoldSpent: GoldEvent {
        /// <description>Amount spent</description>
        public int AmountSpent;
        /// <description>Amount of gold after earnings are added in</description>
        public int CurrentAmount;

        public GoldSpent(int amountSpent, int currentAmount) {
            AmountSpent = amountSpent;
            CurrentAmount = currentAmount;
        }
    }
}