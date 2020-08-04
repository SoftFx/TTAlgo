using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class SymbolsCollection : SymbolEntityBaseCollection<SymbolAccessor, SymbolInfo,
        Symbol, NullSymbol>, Api.SymbolProvider, Api.SymbolList
    {
        private readonly FeedProvider _feedProvider;
        private readonly CurrenciesCollection _currencies;

        public SymbolsCollection(FeedProvider feedProvider, CurrenciesCollection currencies)
        {
            _feedProvider = feedProvider;
            _currencies = currencies;
        }

        public string MainSymbolCode { get; set; }

        public SymbolList List => this;

        public Symbol MainSymbol => _entities[MainSymbolCode];

        public override void Add(SymbolInfo info)
        {
            _entities.TryAdd(info.Name, new SymbolAccessor(info, _feedProvider, _currencies));
        }
    }
}
