namespace TickTrader.Algo.Domain
{
    public interface ICurrencyInfo
    {
        string Name { get; }

        int Digits { get; }

        int SortOrder { get; }
    }

    public partial class CurrencyInfo : ICurrencyInfo
    {
    }
}
