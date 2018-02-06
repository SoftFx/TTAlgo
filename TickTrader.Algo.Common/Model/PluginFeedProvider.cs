using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using Api = TickTrader.Algo.Api;
using System.Threading.Tasks.Dataflow;
using Machinarium.Qnil;

namespace TickTrader.Algo.Common.Model
{
    public class PluginFeedProvider : CrossDomainObject, IPluginFeedProvider, IPluginMetadata, ISynchronizationContext
    {
        private ISyncContext _sync;
        private IFeedSubscription subscription;
        private IVarSet<string, SymbolModel> symbols;
        private QuoteDistributor _distributor;
        private FeedHistoryProviderModel.Handler history;
        private Action<QuoteEntity[]> feedUpdateHandler;
        private Dictionary<string, int> _subscriptionCache;
        private IReadOnlyDictionary<string, CurrencyEntity> currencies;

        private BufferBlock<QuoteEntity> rxBuffer;
        private ActionBlock<QuoteEntity[]> txBlock;

        public ISynchronizationContext Sync { get { return this; } }

        public PluginFeedProvider(IVarSet<string, SymbolModel> symbols, QuoteDistributor quoteDistributor, FeedHistoryProviderModel.Handler history,
            IReadOnlyDictionary<string, CurrencyEntity> currencies, ISyncContext sync)
        {
            _sync = sync;
            this.symbols = symbols;
            _distributor = quoteDistributor;
            this.history = history;
            this.currencies = currencies;
            _subscriptionCache = new Dictionary<string, int>();

            rxBuffer = new BufferBlock<QuoteEntity>();
            txBlock = new ActionBlock<QuoteEntity[]>(uList =>
                {
                    try
                    {
                        feedUpdateHandler?.Invoke(uList);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex.ToString());
                    }
                });

            rxBuffer.BatchLinkTo(txBlock, 30);
        }

        public List<BarEntity> QueryBars(string symbolCode, Api.BarPriceType priceType, DateTime from, DateTime to, Api.TimeFrames timeFrame)
        {
            //return history.GetBars(symbolCode, priceType, timeFrame, from, to).Result;
            throw new NotImplementedException();
        }

        public List<BarEntity> QueryBars(string symbolCode, Api.BarPriceType priceType, DateTime from, int size, Api.TimeFrames timeFrame)
        {
            return history.GetBarPage(symbolCode, priceType, timeFrame, from, size).Result.ToList();
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            //return history.IterateTicks(symbolCode, from, to, depth).Result;
            throw new NotImplementedException();
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, int count, DateTime to, int depth)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Action<QuoteEntity[]> handler)
        {
            //System.Diagnostics.Debug.WriteLine("SUBSCRIBED!");

            if (subscription != null)
                throw new InvalidOperationException("Already subscribed!");

            feedUpdateHandler = handler;

            subscription = _distributor.SubscribeAll();
            subscription.NewQuote += q => rxBuffer.Post(q);
        }

        public void Unsubscribe()
        {
            //System.Diagnostics.Debug.WriteLine("UNSUBSCRIBED!");

            foreach (var pair in _subscriptionCache)
            {
                subscription.Remove(pair.Key);
            }
            _subscriptionCache.Clear();

            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
            }
        }

        public void SetSymbolDepth(string symbolCode, int depth)
        {
            if (subscription == null)
                throw new InvalidOperationException("No subscription to change! You must call Subscribe() prior to calling SetSymbolDepth()!");

            _subscriptionCache[symbolCode] = depth;
            subscription.Add(symbolCode, depth);
        }

        public IEnumerable<SymbolEntity> GetSymbolMetadata()
        {
            return symbols.Snapshot.Select(m => m.Value.Descriptor).ToList();
        }

        public IEnumerable<CurrencyEntity> GetCurrencyMetadata()
        {
            return currencies.Values.ToList();
        }

        public IEnumerable<BarEntity> QueryBars(string symbolCode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QuoteEntity> QueryTicks()
        {
            throw new NotImplementedException();
        }

        public void Invoke(Action action)
        {
            _sync.Invoke(action);
        }

        public IEnumerable<QuoteEntity> GetSnapshot()
        {
            return symbols.Snapshot
                .Where(s => s.Value.LastQuote != null)
                .Select(s => s.Value.LastQuote)
                .ToList();
        }
    }
}
