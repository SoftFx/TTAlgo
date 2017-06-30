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
        private IPluginSubscriptionHandler subscriptionHandler;

        internal SymbolProvider SymbolProviderImpl { get { return fixture; } }

        public SymbolsCollection(IPluginSubscriptionHandler subscriptionHandler)
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

        public void Add(SymbolEntity symbol, Dictionary<string, CurrencyEntity> currencies)
        {
            fixture.Add(new SymbolAccessor(symbol, subscriptionHandler, currencies));

            if (symbol.Name == mainSymbolCode)
                InitCurrentSymbol();
        }

        public void Init(IEnumerable<SymbolEntity> symbols, Dictionary<string, CurrencyEntity> currencies)
        {
            fixture.Clear();

            if (symbols != null)
            {
                foreach (var smb in symbols)
                    fixture.Add(new SymbolAccessor(smb, subscriptionHandler, currencies));

                InitCurrentSymbol();
            }
        }

        public void SetRate(Quote quote)
        {
            (fixture.GetOrDefault(quote.Symbol) as SymbolAccessor)?.UpdateRate(quote);
        }

        public IEnumerator<SymbolAccessor> GetEnumerator()
        {
            return fixture.GetInnerCollection().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        internal Symbol GetOrDefault(string symbol)
        {
            return fixture.GetOrDefault(symbol);
        }

        private class SymbolFixture : Api.SymbolProvider, Api.SymbolList
        {
            private Dictionary<string, SymbolAccessor> symbols = new Dictionary<string, SymbolAccessor>();

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

            public Symbol MainSymbol { get; set; }

            public SymbolList List { get { return this; } }

            public Symbol GetOrDefault(string symbol)
            {
                SymbolAccessor entity;
                symbols.TryGetValue(symbol, out entity);
                return entity;
            }

            public void Clear()
            {
                symbols.Clear();
            }

            public void Add(SymbolAccessor symbol)
            {
                symbols.Add(symbol.Name, symbol);
            }

            public IEnumerable<SymbolAccessor> GetInnerCollection()
            {
                return symbols.Values;
            }

            public IEnumerator<Symbol> GetEnumerator()
            {
                return symbols.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return symbols.Values.GetEnumerator();
            }
        }
    }
}
