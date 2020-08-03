using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class SymbolsCollection : Api.SymbolProvider, Api.SymbolList
    {
        private readonly FeedProvider _feedProvider;

        private Dictionary<string, SymbolAccessor> _symbols = new Dictionary<string, SymbolAccessor>();
        private List<SymbolAccessor> sortedSymbols;

        private string _mainSymbolCode;

        public SymbolsCollection(FeedProvider feedProvider)
        {
            _feedProvider = feedProvider;
        }

        public string MainSymbolCode
        {
            get => _mainSymbolCode;
            set
            {
                _mainSymbolCode = value;
            }
        }

        public SymbolList List => this;

        public Symbol MainSymbol => _symbols[MainSymbolCode];

        public IEnumerable<SymbolAccessor> Values => _symbols.Values;

        public Symbol this[string symbolCode]
        {
            get
            {
                if (string.IsNullOrEmpty(symbolCode))
                    return new NullSymbol("");

                SymbolAccessor smb;
                if (!_symbols.TryGetValue(symbolCode, out smb))
                    return new NullSymbol(symbolCode);
                return smb;
            }
        }

        public void Add(Domain.SymbolInfo info, CurrenciesCollection currencies)
        {
            _symbols.Add(info.Name, new SymbolAccessor(info, _feedProvider, currencies));
        }

        public void Init(IEnumerable<Domain.SymbolInfo> symbols, CurrenciesCollection currencies)
        {
            _symbols.Clear();

            if (symbols != null)
            {
                foreach (var smb in symbols)
                    _symbols.Add(smb.Name, new SymbolAccessor(smb, _feedProvider, currencies));
            }
        }

        public void Merge(IEnumerable<Domain.SymbolInfo> symbols, CurrenciesCollection currencies)
        {
            if (symbols != null)
            {
                InvalidateAll();

                foreach (var smb in symbols)
                {
                    var smbAccessor = _symbols.GetOrDefault(smb.Name);
                    if (smbAccessor == null)
                    {
                        _symbols.Add(smb.Name, new SymbolAccessor(smb, _feedProvider, currencies));
                    }
                    else
                    {
                        smbAccessor.Update(smb);
                    }

                }
            }
        }

        internal SymbolAccessor GetOrDefault(string symbol)
        {
            var entity = _symbols.GetOrDefault(symbol);
            if (entity?.IsNull ?? true) // deleted symbols will be present after reconnect, but IsNull will be true
                return null;
            return entity;
        }

        public void InvalidateAll()
        {
            // symbols are not deleted from collection
            // deleted symbols will have IsNull set to true
            foreach (var smb in sortedSymbols)
            {
                smb.Update(null);
            }
            sortedSymbols = null;
        }

        public IEnumerator<Symbol> GetEnumerator()
        {
            return SortedSymbols.GetEnumerator();
        }

        private List<SymbolAccessor> SortedSymbols
        {
            get
            {
                if (sortedSymbols == null)
                    sortedSymbols = _symbols.Values.Where(s => !s.IsNull).OrderBy(s => s.Info.GroupSortOrder).ThenBy(s => s.Info.SortOrder).ThenBy(s => s.Info.Name).ToList();

                return sortedSymbols;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //private class SymbolFixture : Api.SymbolProvider, Api.SymbolList
        //{
        //    private Dictionary<string, SymbolAccessor> symbols = new Dictionary<string, SymbolAccessor>();
        //    private List<SymbolAccessor> sortedSymbols;

        //    public Symbol this[string symbolCode]
        //    {
        //        get
        //        {
        //            if (string.IsNullOrEmpty(symbolCode))
        //                return new NullSymbol("");

        //            SymbolAccessor smb;
        //            if (!symbols.TryGetValue(symbolCode, out smb))
        //                return new NullSymbol(symbolCode);
        //            return smb;
        //        }
        //    }

        //    private List<SymbolAccessor> SortedSymbols
        //    {
        //        get
        //        {
        //            if (sortedSymbols == null)
        //                sortedSymbols = symbols.Values.Where(s => !s.IsNull).OrderBy(s => s.Info.GroupSortOrder).ThenBy(s => s.Info.SortOrder).ThenBy(s => s.Info.Name).ToList();

        //            return sortedSymbols;
        //        }
        //    }

        //    public Symbol MainSymbol { get; set; }

        //    public SymbolList List { get { return this; } }

        //    public SymbolAccessor GetOrDefault(string symbol)
        //    {
        //        if (string.IsNullOrEmpty(symbol))
        //            return null;

        //        SymbolAccessor entity;
        //        symbols.TryGetValue(symbol, out entity);
        //        return entity;
        //    }

        //    public void Clear()
        //    {
        //        symbols.Clear();
        //        sortedSymbols = null;
        //    }

        //    public void Add(SymbolAccessor symbol)
        //    {
        //        symbols.Add(symbol.Info.Name, symbol);
        //        sortedSymbols = null;
        //    }

        //    public void InvalidateAll()
        //    {
        //        // symbols are not deleted from collection
        //        // deleted symbols will have IsNull set to true
        //        foreach(var smb in sortedSymbols)
        //        {
        //            smb.Update(null);
        //        }
        //        sortedSymbols = null;
        //    }

        //    public IEnumerable<SymbolAccessor> GetInnerCollection()
        //    {
        //        return SortedSymbols;
        //    }

        //    public IEnumerator<Symbol> GetEnumerator()
        //    {
        //        return SortedSymbols.GetEnumerator();
        //    }

        //    IEnumerator IEnumerable.GetEnumerator()
        //    {
        //        return SortedSymbols.GetEnumerator();
        //    }
        //}
    }
}
