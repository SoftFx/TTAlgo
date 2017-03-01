using System.Collections.Generic;

namespace TickTrader.Algo.Api
{
    public interface Currency
    {
        string Name { get; } 
        int Digits { get; }
        bool IsNull { get; }
    }

    public interface CurrencyList : IEnumerable<Currency>
    {
        Currency this[string currencyName] { get; }
    }
}