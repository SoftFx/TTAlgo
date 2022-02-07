using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal sealed class SymbolCollection<TSymbol> : ISymbolCollection<TSymbol> where TSymbol : ISymbolData
    {
        private readonly ConcurrentDictionary<string, TSymbol> _symbols;


        public TSymbol this[string name] => _symbols.GetOrDefault(name);

        public List<TSymbol> Symbols => _symbols.Values.ToList();


        public string StorageFolder { get; }


        public event Action<TSymbol> SymbolAdded, SymbolRemoved;
        public event Action<TSymbol, TSymbol> SymbolUpdated;


        internal SymbolCollection(string folder)
        {
            _symbols = new ConcurrentDictionary<string, TSymbol>();

            StorageFolder = folder;
        }


        internal void Initialize(IEnumerable<TSymbol> symbols)
        {
            foreach (var s in symbols)
                TryAddSymbol(s);
        }


        public bool TryAddSymbol(TSymbol symbol)
        {
            var result = _symbols.TryAdd(symbol.Name, symbol);

            if (result)
                SymbolAdded?.Invoke(symbol);

            return false;
        }

        public bool TryRemoveSymbol(TSymbol symbol)
        {
            SymbolRemoved?.Invoke(symbol);

            return true;
        }

        public bool TryUpdateSymbol(TSymbol symbol)
        {
            SymbolUpdated?.Invoke(symbol, symbol);

            return true;
        }
    }
}
