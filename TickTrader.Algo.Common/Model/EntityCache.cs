using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class EntityCache : EntityBase
    {
        private readonly AccountModel _acc;
        private readonly VarDictionary<string, SymbolModel> _symbols = new VarDictionary<string, SymbolModel>();
        private readonly VarDictionary<string, CurrencyEntity> _currencies = new VarDictionary<string, CurrencyEntity>();

        public EntityCache()
        {
            _acc = new AccountModel(_currencies, _symbols);
        }

        public IVarSet<string, SymbolModel> Symbols => _symbols;
        public IVarSet<string, CurrencyEntity> Currencies => _currencies;
        public AccountModel Account => _acc;

        internal void Clear()
        {
            _currencies.Clear();
            _symbols.Clear();
            Account.Clear();
        }

        internal List<SymbolEntity> GetSymbolsCopy()
        {
            return _symbols.Values.Select(s => s.Descriptor).ToList();
        }

        internal List<CurrencyEntity> GetCurrenciesCopy()
        {
            return _currencies.Values.ToList();
        }

        internal EntityCacheUpdate GetSnapshot()
        {
            var smbSnapshot = _symbols.Snapshot.Values.Select(s => s.Descriptor).ToList();
            var currSnapshot = _currencies.Snapshot.Values.ToList();
            return new Snapshot(smbSnapshot, currSnapshot, _acc.GetSnapshotUpdate());
        }

        internal void ApplyQuote(QuoteEntity quote)
        {
            var smb = _symbols.GetOrDefault(quote.Symbol);
            smb?.OnNewTick(quote);
        }

        [Serializable]
        public class SymbolUpdate : EntityCacheUpdate
        {
            public SymbolUpdate(SymbolEntity symbol)
            {
                Symbol = symbol ?? throw new ArgumentNullException("symbol");
            }

            private SymbolEntity Symbol { get; }

            public void Apply(EntityCache cache)
            {
                if (cache._symbols.ContainsKey(Symbol.Name))
                    cache._symbols[Symbol.Name].Update(Symbol);
                else
                    cache._symbols.Add(Symbol.Name, new SymbolModel(Symbol, cache._currencies));
            }
        }

        [Serializable]
        public class CurrencyUpdate : EntityCacheUpdate
        {
            public CurrencyUpdate(CurrencyEntity currency)
            {
                Currency = currency ?? throw new ArgumentNullException("symbol");
            }

            private CurrencyEntity Currency { get; }

            public void Apply(EntityCache cache)
            {
                cache._currencies.Add(Currency.Name, Currency);
            }
        }

        [Serializable]
        public class ClearAll : EntityCacheUpdate
        {
            public void Apply(EntityCache cache) => cache.Clear();
        }

        [Serializable]
        public class Snapshot : EntityCacheUpdate
        {
            private IEnumerable<SymbolEntity> _symbols;
            private IEnumerable<CurrencyEntity> _currencies;
            private EntityCacheUpdate _accountSnaphsotUpdate;

            public Snapshot(IEnumerable<SymbolEntity> symbols, IEnumerable<CurrencyEntity> currencies, EntityCacheUpdate accUpdate)
            {
                _symbols = symbols;
                _currencies = currencies;
                _accountSnaphsotUpdate = accUpdate;
            }

            public void Apply(EntityCache cache)
            {
                cache._currencies.Clear();
                cache._symbols.Clear();

                foreach (var curr in _currencies)
                    cache._currencies.Add(curr.Name, curr);

                foreach (var smb in _symbols)
                    cache._symbols.Add(smb.Name, new SymbolModel(smb, cache.Currencies));

                _accountSnaphsotUpdate.Apply(cache);
            }
        }
    }

    public interface EntityCacheUpdate
    {
        void Apply(EntityCache cache);
    }
}
