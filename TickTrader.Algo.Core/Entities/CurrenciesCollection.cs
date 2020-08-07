using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class CurrenciesCollection : SymbolEntityBaseCollection<CurrencyAccessor, CurrencyInfo,
        Currency, NullCurrency>, Api.CurrencyList
    {
        public override void Add(CurrencyInfo info)
        {
            _entities.TryAdd(info.Name, new CurrencyAccessor(info));
        }
    }
}