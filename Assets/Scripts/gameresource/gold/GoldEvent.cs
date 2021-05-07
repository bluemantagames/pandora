namespace Pandora.Resource.Gold
{
    public class GoldEvent
    {
        /// <description>Amount of gold after earnings are added in</description>
        public int CurrentAmount;
    }

    public class GoldEarned : GoldEvent
    {
        /// <description>Amount earned</description>
        public int AmountEarned;

        public GoldEarned(int currentAmount, int amountEarned)
        {
            CurrentAmount = currentAmount;
            AmountEarned = amountEarned;
        }
    }

    public class GoldSpent : GoldEvent
    {
        /// <description>Amount spent</description>
        public int AmountSpent;

        public GoldSpent(int currentAmount, int amountSpent)
        {
            CurrentAmount = currentAmount;
            AmountSpent = amountSpent;
        }
    }

    public class GoldUpperReserve : GoldEvent
    {
        /// <description>Set the gold upper reserve</description>
        public int UpperReserve;

        public GoldUpperReserve(int currentAmount, int upperReserve)
        {
            CurrentAmount = currentAmount;
            UpperReserve = upperReserve;
        }
    }
}