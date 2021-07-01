using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class PositionSide : IPositionSide
    {
        private double margin;
        private double profit;

        public double Amount { get; set; }
        public double Price { get; set; }

        public PositionSide(double amount, double price)
        {
            Amount = amount;
            Price = price;
        }

        public double Margin
        {
            get { return margin; }
            set
            {
                if (margin != value)
                {
                    margin = value;
                    MarginUpdated?.Invoke();
                }
            }
        }

        public double Profit
        {
            get { return profit; }
            set
            {
                if (profit != value)
                {
                    profit = value;
                    ProfitUpdated?.Invoke();
                }
            }
        }

        public System.Action ProfitUpdated;
        public System.Action MarginUpdated;
    }
}
