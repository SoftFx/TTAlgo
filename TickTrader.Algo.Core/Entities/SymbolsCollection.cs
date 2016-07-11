using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class SymbolsCollection
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
            fixture.Current = fixture[mainSymbolCode];
        }

        public void Add(SymbolEntity symbol)
        {
            fixture.InnerCollection.Add(symbol.Code, new SymbolAccessor(symbol, subscriptionHandler));

            if (symbol.Code == mainSymbolCode)
                InitCurrentSymbol();
        }

        public void Init(IEnumerable<SymbolEntity> symbols)
        {
            fixture.InnerCollection.Clear();

            if (symbols != null)
            {
                foreach (var smb in symbols)
                    fixture.InnerCollection.Add(smb.Code, new SymbolAccessor(smb, subscriptionHandler));

                InitCurrentSymbol();
            }
        }

        private class SymbolFixture : Api.SymbolProvider
        {
            private Dictionary<string, Api.Symbol> symbols = new Dictionary<string, Api.Symbol>();

            public Symbol this[string symbolCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(symbolCode))
                        return new NullSymbol("");

                    Api.Symbol smb;
                    if (!symbols.TryGetValue(symbolCode, out smb))
                        return new NullSymbol(symbolCode);
                    return smb;
                }
            }

            public Symbol Current { get; set; }
            public Dictionary<string, Api.Symbol> InnerCollection { get { return symbols; } }

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
