using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using Api = TickTrader.Algo.Api;
using Cl = Caliburn.Micro;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.BotTerminal
{
    internal abstract class PluginFeedProvider : CrossDomainObject, IPluginFeedProvider, IPluginMetadata, ISynchronizationContext
    {
        private IFeedSubscription subscription;
        private SymbolCollectionModel symbols;
        private FeedHistoryProviderModel history;
        private Action<QuoteEntity[]> feedUpdateHandler;

        private BufferBlock<QuoteEntity> rxBuffer;
        private ActionBlock<QuoteEntity[]> txBlock;

        public ISynchronizationContext Sync { get { return this; } }

        public PluginFeedProvider(SymbolCollectionModel symbols, FeedHistoryProviderModel history)
        {
            this.symbols = symbols;
            this.history = history;

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

        public IEnumerable<BarEntity> QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame)
        {
            BarPeriod period = FdkToAlgo.ToBarPeriod(timeFrame);
            var result = history.GetBars(symbolCode, PriceType.Ask, period, from, to).Result;
            return FdkToAlgo.Convert(result).ToList();
        }

        public IEnumerable<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            try
            {
                var result = history.GetTicks(symbolCode, from, to, depth).Result;
                return FdkToAlgo.Convert(result).ToList();
            }
            catch (Exception)
            {
                // TO DO : return corresponding error code
                return Enumerable.Empty<QuoteEntity>();
            }
        }

        public void Subscribe(Action<QuoteEntity[]> handler)
        {
            System.Diagnostics.Debug.WriteLine("SUBSCRIBED!");

            if (subscription != null)
                throw new InvalidOperationException("Already subscribed!");

            feedUpdateHandler = handler;

            subscription = symbols.SubscribeAll();
            subscription.NewQuote += q => rxBuffer.Post(FdkToAlgo.Convert(q));
        }

        public void Unsubscribe()
        {
            System.Diagnostics.Debug.WriteLine("UNSUBSCRIBED!");

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
            Caliburn.Micro.Execute.OnUIThread(action);
        }

        List<BarEntity> IPluginFeedProvider.QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame)
        {
            throw new NotImplementedException();
        }

        List<QuoteEntity> IPluginFeedProvider.QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QuoteEntity> GetSnapshot()
        {
            return symbols.Snapshot
                .Where(s => s.Value.LastQuote != null)
                .Select(s => FdkToAlgo.Convert(s.Value.LastQuote))
                .ToList();
        }
    }

    internal class BarBasedFeedProvider : PluginFeedProvider, IBarBasedFeed
    {
        private Func<List<BarEntity>> mainSeriesProvider;

        public BarBasedFeedProvider(TraderClientModel feed, Func<List<BarEntity>> mainSeriesProvider)
            : base(feed.Symbols, feed.History)
        {
            if (mainSeriesProvider == null)
                throw new ArgumentNullException("mainSeriesProvider");

            this.mainSeriesProvider = mainSeriesProvider;
        }

        public List<BarEntity> GetMainSeries()
        {
            return mainSeriesProvider();
        }
    }

    internal class QuoteBasedFeedProvider : PluginFeedProvider, IQuoteBasedFeed
    {
        private Func<List<QuoteEntity>> mainSeriesProvider;

        public QuoteBasedFeedProvider(TraderClientModel feed, Func<List<QuoteEntity>> mainSeriesProvider)
            : base(feed.Symbols, feed.History)
        {
            if (mainSeriesProvider == null)
                throw new ArgumentNullException("mainSeriesProvider");

            this.mainSeriesProvider = mainSeriesProvider;
        }

        public List<QuoteEntity> GetMainSeries()
        {
            return mainSeriesProvider();
        }
    }
}
