using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public sealed class SymbolsCollection : SymbolEntityBaseCollection<SymbolAccessor, SymbolInfo,
        Symbol, NullSymbol>, SymbolProvider, SymbolList
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

        public Symbol MainSymbol => (Symbol)GetOrNull(MainSymbolCode) ?? new NullSymbol();

        public override void Add(SymbolInfo info)
        {
            _entities.TryAdd(info.Name, new SymbolAccessor(info, _feedProvider, _currencies));
        }
    }
}
