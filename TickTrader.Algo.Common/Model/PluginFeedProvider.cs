using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using Api = TickTrader.Algo.Api;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Common.Model
{
    public class PluginFeedProvider : CrossDomainObject, IPluginFeedProvider, IPluginMetadata, ISynchronizationContext
    {
        private ISyncContext _sync;
        private IFeedSubscription subscription;
        private ISymbolManager symbols;
        private FeedHistoryProviderModel history;
        private Action<QuoteEntity[]> feedUpdateHandler;
        private Dictionary<string, CurrencyInfo> currencies;

        private BufferBlock<QuoteEntity> rxBuffer;
        private ActionBlock<QuoteEntity[]> txBlock;

        public ISynchronizationContext Sync { get { return this; } }

        public PluginFeedProvider(ISymbolManager symbols, FeedHistoryProviderModel history,
            Dictionary<string, CurrencyInfo> currencies, ISyncContext sync)
        {
            _sync = sync;
            this.symbols = symbols;
            this.history = history;
            this.currencies = currencies;

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
            BarPeriod period = FdkToAlgo.ToBarPeriod(timeFrame);
            var result = history.GetBars(symbolCode, FdkToAlgo.Convert(priceType), period, from, to).Result;
            return FdkToAlgo.Convert(result).ToList();
        }

        public List<BarEntity> QueryBars(string symbolCode, Api.BarPriceType priceType, DateTime from, int size, Api.TimeFrames timeFrame)
        {
            BarPeriod period = FdkToAlgo.ToBarPeriod(timeFrame);
            var result = history.GetBars(symbolCode, FdkToAlgo.Convert(priceType), period, from, size).Result;
            return FdkToAlgo.Convert(result).ToList();
        }

        public List<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            var result = history.GetTicks(symbolCode, from, to, depth).Result;
            return FdkToAlgo.Convert(result).ToList();
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

            subscription = symbols.SubscribeAll();
            subscription.NewQuote += q => rxBuffer.Post(FdkToAlgo.Convert(q));
        }

        public void Unsubscribe()
        {
            //System.Diagnostics.Debug.WriteLine("UNSUBSCRIBED!");

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

            subscription.Add(symbolCode, depth);
        }

        public IEnumerable<SymbolEntity> GetSymbolMetadata()
        {
            return symbols.Snapshot.Select(m => FdkToAlgo.Convert(m.Value.Descriptor)).ToList();
        }

        public IEnumerable<CurrencyEntity> GetCurrencyMetadata()
        {
            return currencies.Values.Select(FdkToAlgo.Convert).ToList();
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
                .Select(s => FdkToAlgo.Convert(s.Value.LastQuote))
                .ToList();
        }
    }
}
