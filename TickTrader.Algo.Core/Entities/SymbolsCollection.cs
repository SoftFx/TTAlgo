using System.Collections.Generic;
using System.Linq;
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

        public Symbol MainSymbol => (Symbol)GetOrNull(MainSymbolCode) ?? new NullSymbol();

        public override void Add(SymbolInfo info)
        {
            _entities.TryAdd(info.Name, new SymbolAccessor(info, _feedProvider, _currencies));
        }

        //public override IEnumerable<SymbolAccessor> Values => _entities.Values.Where(e => !e.IsNull).OrderBy(s => s.Info.GroupSortOrder).ThenBy(s => s.Info.SortOrder).ThenBy(s => s.Info.Name);
    }
}
