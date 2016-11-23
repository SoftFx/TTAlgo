using Machinarium.Qnil;
using Machinarium.State;
using NLog;
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
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SymbolCollectionModel : IDynamicDictionarySource<string, SymbolModel>, IOrderDependenciesResolver
    {
        private Logger logger;
        private DynamicDictionary<string, SymbolModel> symbols = new DynamicDictionary<string, SymbolModel>();
        private IDictionary<string, CurrencyInfo> currencies;

        public event DictionaryUpdateHandler<string, SymbolModel> Updated { add { symbols.Updated += value; } remove { symbols.Updated -= value; } }

        public SymbolCollectionModel(ConnectionModel connection)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            Distributor = new QuoteDistributor(connection);
        }

        public IReadOnlyDictionary<string, SymbolModel> Snapshot { get { return symbols.Snapshot; } }
        public QuoteDistributor Distributor { get; private set; }

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
                SymbolModel model;
                if (symbols.TryGetValue(info.Name, out model))
                    model.Update(info);
                else
                {
                    Distributor.AddSymbol(info.Name);
                    model = new SymbolModel(Distributor, info, currencies);
                    symbols.Add(info.Name, model);
                }
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

        SymbolModel IOrderDependenciesResolver.GetSymbolOrNull(string name)
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
