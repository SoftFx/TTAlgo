using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public sealed class CurrencyInfoStorage
    {
        private readonly List<string> _currencyNames = new() { "EUR", "CAD", "USD", "AUD" };

        public static CurrencyInfoStorage Instance { get; } = new CurrencyInfoStorage();


        public readonly Dictionary<string, CurrencyInfo> Currency;

        private CurrencyInfoStorage()
        {
            Currency = _currencyNames.ToDictionary(k => k, v => CurrencyFactory.BuildCurrency(v));
        }
    }
}
