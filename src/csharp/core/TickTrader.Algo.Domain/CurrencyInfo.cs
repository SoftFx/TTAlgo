namespace TickTrader.Algo.Domain
{
    public interface ICurrencyInfo : IBaseSymbolInfo
    {
        int Digits { get; }
    }

    public partial class CurrencyInfo : ICurrencyInfo
    {
    }
}
