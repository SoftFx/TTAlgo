using TickTrader.Algo.Domain;

public class PositionSide : IPositionSide
{
    private decimal margin;
    private decimal profit;

    public decimal Amount { get; set; }
    public decimal Price { get; set; }

    public PositionSide(double amount, double price)
    {
        Amount = (decimal)amount;
        Price = (decimal)price;
    }

    public decimal Margin
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

    public decimal Profit
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
