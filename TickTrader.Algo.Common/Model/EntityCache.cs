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
        private AccountModel _acc;
        private VarDictionary<string, SymbolModel> _symbols = new VarDictionary<string, SymbolModel>();
        private VarDictionary<string, CurrencyEntity> _currencies = new VarDictionary<string, CurrencyEntity>();

        public EntityCache(AccountModelOptions accOptions)
        {
            _acc = new AccountModel(_currencies, _symbols, accOptions);
            _symbols = new VarDictionary<string, SymbolModel>();
            _currencies = new VarDictionary<string, CurrencyEntity>();
        }

        public IVarSet<string, SymbolModel> Symbols => _symbols;
        public IVarSet<string, CurrencyEntity> Currencies => _currencies;
        public AccountModel Account => _acc;

        internal EntityCacheUpdate GetSnapshotUpdate()
        {
            var smbSnapshot = _symbols.Snapshot.Values.Select(s => s.Descriptor).ToList();
            var currSnapshot = _currencies.Snapshot.Values.ToList();
            return new LoadSnapshot(smbSnapshot, currSnapshot, _acc.GetSnapshotUpdate());
        }

        [Serializable]
        private class LoadSnapshot : EntityCacheUpdate
        {
            private List<SymbolEntity> _symbols;
            private List<CurrencyEntity> _currencies;
            private EntityCacheUpdate _accountSnaphsotUpdate;

            public LoadSnapshot(List<SymbolEntity> symbols, List<CurrencyEntity> currencies, EntityCacheUpdate accUpdate)
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
            }
        }
    }

    public interface EntityCacheUpdate
    {
        void Apply(EntityCache cache);
    }
}
