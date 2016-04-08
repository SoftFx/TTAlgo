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

        internal SymbolProvider SymbolProviderImpl { get { return fixture; } }

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
            fixture.InnerCollection.Add(symbol.Code, symbol);

            if (symbol.Code == mainSymbolCode)
                InitCurrentSymbol();
        }

        public void Init(IEnumerable<SymbolEntity> symbols)
        {
            fixture.InnerCollection.Clear();

            foreach (var smb in symbols)
                fixture.InnerCollection.Add(smb.Code, smb);

            InitCurrentSymbol();
        }

        private class SymbolFixture : Api.SymbolProvider
        {
            private Dictionary<string, SymbolEntity> symbols = new Dictionary<string, SymbolEntity>();

            public Symbol this[string symbolCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(symbolCode))
                        return new NullSymbol("");

                    SymbolEntity entity;
                    if (!symbols.TryGetValue(symbolCode, out entity))
                        return new NullSymbol(symbolCode);
                    return entity;
                }
            }

            public Symbol Current { get; set; }
            public Dictionary<string, SymbolEntity> InnerCollection { get { return symbols; } }

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
