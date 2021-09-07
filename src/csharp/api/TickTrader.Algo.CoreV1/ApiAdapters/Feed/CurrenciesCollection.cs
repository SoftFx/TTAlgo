using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public sealed class CurrenciesCollection : SymbolEntityBaseCollection<CurrencyAccessor, CurrencyInfo,
        Currency, NullCurrency>, CurrencyList
    {
        public override void Add(CurrencyInfo info)
        {
            _entities.TryAdd(info.Name, new CurrencyAccessor(info));
        }
    }
}