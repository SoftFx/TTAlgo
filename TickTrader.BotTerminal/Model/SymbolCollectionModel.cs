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
    internal class SymbolCollectionModel : IDynamicDictionarySource<string, SymbolModel>
    {
        private Logger logger;
        private DynamicDictionary<string, SymbolModel> symbols = new DynamicDictionary<string, SymbolModel>();
        private Dictionary<string, CurrencyInfo> currencies;
        private ConnectionModel connection;
        private ActionBlock<Quote> rateUpdater;
        private List<Algo.Core.SymbolEntity> algoSymbolCache = new List<Algo.Core.SymbolEntity>();
        private ActionBlock<Task> requestQueue;

        public event DictionaryUpdateHandler<string, SymbolModel> Updated { add { symbols.Updated += value; } remove { symbols.Updated -= value; } }

        public SymbolCollectionModel(ConnectionModel connection)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.connection = connection;

            connection.Connecting += () => connection.FeedProxy.Tick += FeedProxy_Tick;
            connection.Disconnecting += () => connection.FeedProxy.Tick -= FeedProxy_Tick;
            connection.Deinitalizing += (s, c) => Stop();

            rateUpdater = DataflowHelper.CreateUiActionBlock<Quote>(UpdateRate, 100, 100, CancellationToken.None);
        }

        public IEnumerable<Algo.Core.SymbolEntity> AlgoSymbolCache { get { return algoSymbolCache; } }
        public IReadOnlyDictionary<string, SymbolModel> Snapshot { get { return symbols.Snapshot; } }
        public event Action<Quote> RateUpdated = delegate { };

        private void UpdateRate(Quote tick)
        {
            SymbolModel symbol;
            if (symbols.TryGetValue(tick.Symbol, out symbol))
                symbol.CastNewTick(tick);
            RateUpdated(tick);
        }

        public void Initialize(SymbolInfo[] symbolSnapshot, CurrencyInfo[] currencySnapshot)
        {
            currencies = currencySnapshot.ToDictionary(c => c.Name);
            algoSymbolCache = symbolSnapshot.Select(FdkToAlgo.Convert).ToList();

            Merge(symbolSnapshot);
            Reset();
            requestQueue = new ActionBlock<Task>(t => t.RunSynchronously());
            EnqueuBatchSubscription();
        }

        private void Reset()
        {
            foreach (var smb in symbols.Values)
                smb.Reset();
        }

        private void EnqueuBatchSubscription()
        {
            foreach (var group in symbols.Values.GroupBy(s => s.Depth))
            {
                var depth = group.Key;
                var symbols = group.Select(s => s.Name).ToArray();
                EnqueueSubscriptionRequest(depth, symbols);
            }
        }

        private async Task Stop()
        {
            requestQueue.Complete();
            await requestQueue.Completion;
        }

        void FeedProxy_Tick(object sender, SoftFX.Extended.Events.TickEventArgs e)
        {
            rateUpdater.SendAsync(e.Tick).Wait();
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
                    model = new SymbolModel(this, info, currencies);
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
                symbols.Remove(model.Name);
        }

        public void Dispose()
        {
        }

        public Task EnqueueSubscriptionRequest(int depth, params string[] symbols)
        {
            var subscribeTask = new Task(() => connection.FeedProxy.Server.SubscribeToQuotes(symbols, depth));
            requestQueue.Post(subscribeTask);
            return subscribeTask;
            
        }

        public SymbolModel GetOrDefault(string key)
        {
            SymbolModel result;
            this.symbols.TryGetValue(key, out result);
            return result;
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
