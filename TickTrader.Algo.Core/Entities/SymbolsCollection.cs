using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class SymbolsCollection : IEnumerable<SymbolAccessor>
    {
        private SymbolFixture fixture = new SymbolFixture();
        private string mainSymbolCode;
        private FeedProvider subscriptionHandler;

        internal SymbolProvider SymbolProviderImpl { get { return fixture; } }

        public SymbolsCollection(FeedProvider subscriptionHandler)
        {
            this.subscriptionHandler = subscriptionHandler;
        }

        public string MainSymbolCode
        {
            get { return mainSymbolCode; }
            set
            {
                mainSymbolCode = value;
                InitCurrentSymbol();
            }
        }

        private void InitCurrentSymbol()
        {
            fixture.MainSymbol = fixture[mainSymbolCode];
        }

        public void Add(Domain.SymbolInfo symbol, CurrenciesCollection currencies)
        {
            fixture.Add(new SymbolAccessor(symbol, subscriptionHandler, currencies));

            if (symbol.Name == mainSymbolCode)
                InitCurrentSymbol();
        }

        public void Init(IEnumerable<Domain.SymbolInfo> symbols, CurrenciesCollection currencies)
        {
            fixture.Clear();

            if (symbols != null)
            {
                foreach (var smb in symbols)
                    fixture.Add(new SymbolAccessor(smb, subscriptionHandler, currencies));

                InitCurrentSymbol();
            }
        }

        public void Merge(IEnumerable<Domain.SymbolInfo> symbols, CurrenciesCollection currencies)
        {
            if (symbols != null)
            {
                fixture.InvalidateAll();

                foreach (var smb in symbols)
                {
                    var smbAccessor = fixture.GetOrDefault(smb.Name);
                    if (smbAccessor == null)
                    {
                        fixture.Add(new SymbolAccessor(smb, subscriptionHandler, currencies));
                    }
                    else
                    {
                        smbAccessor.Update(smb, currencies);
                    }

                }

                InitCurrentSymbol();
            }
        }

        //public void SetRate(Quote quote)
        //{
        //    (fixture.GetOrDefault(quote.Symbol) as SymbolAccessor)?.UpdateRate(quote);
        //}

        public IEnumerator<SymbolAccessor> GetEnumerator()
        {
            return fixture.GetInnerCollection().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        internal SymbolAccessor GetOrDefault(string symbol)
        {
            var entity = fixture.GetOrDefault(symbol);
            if (entity?.IsNull ?? true) // deleted symbols will be present after reconnect, but IsNull will be true
                return null;
            return entity;
        }

        private class SymbolFixture : Api.SymbolProvider, Api.SymbolList
        {
            private Dictionary<string, SymbolAccessor> symbols = new Dictionary<string, SymbolAccessor>();
            private List<SymbolAccessor> sortedSymbols;

            public Symbol this[string symbolCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(symbolCode))
                        return new NullSymbol("");

                    SymbolAccessor smb;
                    if (!symbols.TryGetValue(symbolCode, out smb))
                        return new NullSymbol(symbolCode);
                    return smb;
                }
            }

            private List<SymbolAccessor> SortedSymbols
            {
                get
                {
                    if (sortedSymbols == null)
                        sortedSymbols = symbols.Values.Where(s => !s.IsNull).OrderBy(s => s.GroupSortOrder).ThenBy(s => s.SortOrder).ThenBy(s => s.Name).ToList();

                    return sortedSymbols;
                }
            }

            public Symbol MainSymbol { get; set; }

            public SymbolList List { get { return this; } }

            public SymbolAccessor GetOrDefault(string symbol)
            {
                if (string.IsNullOrEmpty(symbol))
                    return null;

                SymbolAccessor entity;
                symbols.TryGetValue(symbol, out entity);
                return entity;
            }

            public void Clear()
            {
                symbols.Clear();
                sortedSymbols = null;
            }

            public void Add(SymbolAccessor symbol)
            {
                symbols.Add(symbol.Name, symbol);
                sortedSymbols = null;
            }

            public void InvalidateAll()
            {
                // symbols are not deleted from collection
                // deleted symbols will have IsNull set to true
                foreach(var smb in sortedSymbols)
                {
                    smb.Update(null, null);
                }
                sortedSymbols = null;
            }

            public IEnumerable<SymbolAccessor> GetInnerCollection()
            {
                return SortedSymbols;
            }

            public IEnumerator<Symbol> GetEnumerator()
            {
                return SortedSymbols.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return SortedSymbols.GetEnumerator();
            }
        }
    }
}
