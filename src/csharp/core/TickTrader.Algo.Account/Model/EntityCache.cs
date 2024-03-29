﻿using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class EntityCache : EntityBase
    {
        private readonly AccountModel _acc;
        private readonly VarDictionary<string, SymbolInfo> _symbols = new VarDictionary<string, SymbolInfo>();
        private readonly VarDictionary<string, CurrencyInfo> _currencies = new VarDictionary<string, CurrencyInfo>();

        public EntityCache()
        {
            _acc = new AccountModel(_currencies, _symbols);
        }

        public IVarSet<string, SymbolInfo> Symbols => _symbols;
        public IVarSet<string, CurrencyInfo> Currencies => _currencies;
        public AccountModel Account => _acc;

        internal void Clear()
        {
            _currencies.Clear();
            _symbols.Clear();
            Account.Clear();
        }

        internal List<SymbolUpdate> GetMergeUpdate(IEnumerable<Domain.SymbolInfo> snapshot)
        {
            var updates = new List<SymbolUpdate>();

            foreach (var newSmb in snapshot)
                updates.Add(new SymbolUpdate(newSmb, EntityCacheActions.Upsert));

            var symbolsByName = snapshot.ToDictionary(s => s.Name);

            foreach (var existingSmb in _symbols.Values)
            {
                if (!symbolsByName.ContainsKey(existingSmb.Name))
                    updates.Add(new SymbolUpdate(existingSmb, EntityCacheActions.Remove));
            }

            return updates;
        }

        internal List<CurrencyUpdate> GetMergeUpdate(IEnumerable<CurrencyInfo> snapshot)
        {
            var updates = new List<CurrencyUpdate>();

            foreach (var newCurr in snapshot)
                updates.Add(new CurrencyUpdate(newCurr, EntityCacheActions.Upsert));

            var currenciesByNAme = snapshot.ToDictionary(s => s.Name);

            foreach (var existingCurr in _currencies.Values)
            {
                if (!currenciesByNAme.ContainsKey(existingCurr.Name))
                    updates.Add(new CurrencyUpdate(existingCurr, EntityCacheActions.Remove));
            }

            return updates;
        }

        internal List<Domain.SymbolInfo> GetSymbolsCopy()
        {
            return _symbols.Values.ToList();
        }

        internal List<CurrencyInfo> GetCurrenciesCopy()
        {
            return _currencies.Values.ToList();
        }

        public Domain.SymbolInfo GetDefaultSymbol()
        {
            return _symbols.Values.OrderBy(s => s.GroupSortOrder).ThenBy(s => s.SortOrder).ThenBy(s => s.Name).Where(d => !d.Name.EndsWith("_L")).FirstOrDefault();
        }

        internal EntityCacheUpdate GetAccSnapshot()
        {
            return _acc.GetSnapshotUpdate();
        }

        internal void ApplyQuote(QuoteInfo quote)
        {
            var smb = _symbols.GetOrDefault(quote.Symbol);
            smb?.UpdateRate(quote);
            //_acc?.Market?.UpdateRate(quote);
        }

        [Serializable]
        public class SymbolUpdate : EntityCacheUpdate
        {
            public SymbolUpdate(SymbolInfo symbol, EntityCacheActions action)
            {
                Symbol = symbol ?? throw new ArgumentNullException("symbol");
                Action = action;
            }

            private SymbolInfo Symbol { get; }
            public EntityCacheActions Action { get; }

            public void Apply(EntityCache cache)
            {
                if (Action == EntityCacheActions.Upsert)
                {
                    if (cache._symbols.ContainsKey(Symbol.Name))
                        cache._symbols[Symbol.Name].Update(Symbol);
                    else
                        cache._symbols.Add(Symbol.Name, Symbol);
                }
                else
                    cache._symbols.Remove(Symbol.Name);
            }
        }

        [Serializable]
        public class CurrencyUpdate : EntityCacheUpdate
        {
            public CurrencyUpdate(CurrencyInfo currency, EntityCacheActions action)
            {
                Currency = currency ?? throw new ArgumentNullException("currency");
                Action = action;
            }

            private CurrencyInfo Currency { get; }
            private EntityCacheActions Action { get; }

            public void Apply(EntityCache cache)
            {
                if (Action == EntityCacheActions.Upsert)
                    cache._currencies[Currency.Name] = Currency;
                else
                    cache._currencies.Remove(Currency.Name);
            }
        }

        [Serializable]
        public class ClearAll : EntityCacheUpdate
        {
            public void Apply(EntityCache cache) => cache.Clear();
        }
    }

    public interface EntityCacheUpdate
    {
        void Apply(EntityCache cache);
    }

    public enum EntityCacheActions { Upsert, Remove }
}
