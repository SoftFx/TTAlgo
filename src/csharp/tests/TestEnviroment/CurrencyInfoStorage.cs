using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public sealed class CurrencyInfoStorage
    {
        private readonly List<string> _currencyNames = new() { "EUR", "CAD", "USD", "AUD", "JPY", "BTC" };


        public Dictionary<string, CurrencyInfo> Currency { get; }

        public CurrencyInfoStorage()
        {
            Currency = _currencyNames.ToDictionary(k => k, v => CurrencyFactory.BuildCurrency(v));
        }
    }
}
