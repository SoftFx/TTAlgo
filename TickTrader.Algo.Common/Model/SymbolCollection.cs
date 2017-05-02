using Machinarium.Qnil;
using Machinarium.State;
using SoftFX.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public abstract class SymbolCollectionBase : IDynamicDictionarySource<string, SymbolModel>, IOrderDependenciesResolver, ISymbolManager 
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger("SymbolCollectionModel");

        private ISyncContext _sync;
        private DynamicDictionary<string, SymbolModel> symbols = new DynamicDictionary<string, SymbolModel>();
        private IDictionary<string, CurrencyInfo> currencies;

        public event DictionaryUpdateHandler<string, SymbolModel> Updated { add { symbols.Updated += value; } remove { symbols.Updated -= value; } }

        public SymbolCollectionBase(ConnectionModel connection, ISyncContext sync)
        {
            _sync = sync;
        }

        protected virtual SymbolModel CreateSymbolsEntity(QuoteDistributorBase distributor, SymbolInfo info, IDictionary<string, CurrencyInfo> currencies)
        {
            return new SymbolModel(info, currencies);
        }

        public IReadOnlyDictionary<string, SymbolModel> Snapshot { get { return symbols.Snapshot; } }
        public abstract QuoteDistributorBase Distributor { get; }

        public void Initialize(SymbolInfo[] symbolSnapshot, IDictionary<string, CurrencyInfo> currencySnapshot)
        {
            this.currencies = currencySnapshot;
            Merge(symbolSnapshot);
            Distributor.Init();
        }

        public IFeedSubscription SubscribeAll()
        {
            return Distributor.SubscribeAll();
        }

        private void Merge(IEnumerable<SymbolInfo> freshSnashot)
        {
            var freshSnapshotDic = freshSnashot.ToDictionary(i => i.Name);

            // upsert
            foreach (var info in freshSnashot)
            {
                _sync.Invoke(() =>
                {
                    SymbolModel model;
                    if (symbols.TryGetValue(info.Name, out model))
                        model.Update(info);
                    else
                    {
                        Distributor.AddSymbol(info.Name);
                        model = CreateSymbolsEntity(Distributor, info, currencies);
                        symbols.Add(info.Name, model);
                    }
                });
            }

            // delete
            List<SymbolModel> toRemove = new List<SymbolModel>();
            foreach (var symbolModel in symbols.Values)
            {
                if (!freshSnapshotDic.ContainsKey(symbolModel.Name))
                    toRemove.Add(symbolModel);
            }

            foreach (var model in toRemove)
            {
                symbols.Remove(model.Name);
                model.Close();
                Distributor.RemoveSymbol(model.Name);
            }
        }

        public Task Deinit()
        {
            return Distributor.Stop();
        }

        public void Dispose()
        {
        }

        public SymbolModel GetOrDefault(string key)
        {
            SymbolModel result;
            this.symbols.TryGetValue(key, out result);
            return result;
        }

        Algo.Common.Model.SymbolModel IOrderDependenciesResolver.GetSymbolOrNull(string name)
        {
            return GetOrDefault(name);
        }

        public SymbolModel this[string key]
        {
            get
            {
                SymbolModel result;
                if (!this.symbols.TryGetValue(key, out result))
                    throw new ArgumentException("Symbol Not Found: " + key);
                return result;
            }
        }
    }
}
