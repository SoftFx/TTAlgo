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
    internal abstract class PluginFeedProvider : NoTimeoutByRefObject, IPluginFeedProvider, IPluginMetadata, ISynchronizationContext
    {
        private Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>();
        private SymbolCollectionModel symbols;
        private FeedHistoryProviderModel history;

        private BufferBlock<FeedUpdate> rxBuffer;
        private ActionBlock<FeedUpdate[]> txBlock;

        public ISynchronizationContext Sync { get { return this; } }

        public PluginFeedProvider(SymbolCollectionModel symbols, FeedHistoryProviderModel history)
        {
            this.symbols = symbols;
            this.history = history;

            rxBuffer = new BufferBlock<FeedUpdate>();
            txBlock = new ActionBlock<FeedUpdate[]>(uList =>
                {
                    try
                    {
                        FeedUpdated(uList);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex.ToString());
                    }
                });

            rxBuffer.BatchLinkTo(txBlock, 30);
        }

        public event Action<FeedUpdate[]> FeedUpdated = delegate { };

        public IEnumerable<BarEntity> QueryBars(string symbolCode, DateTime from, DateTime to, Api.TimeFrames timeFrame)
        {
            BarPeriod period = FdkToAlgo.ToBarPeriod(timeFrame);
            var result = history.GetBars(symbolCode, PriceType.Ask, period, from, to).Result;
            return FdkToAlgo.Convert(result).ToList();
        }

        public IEnumerable<QuoteEntity> QueryTicks(string symbolCode, DateTime from, DateTime to, int depth)
        {
            var result = history.GetTicks(symbolCode, from, to, depth).Result;
            return FdkToAlgo.Convert(result).ToList();
        }

        public void Subscribe(string symbolCode, int depth)
        {
            Cl.Execute.OnUIThread(() =>
            {
                Subscription s;
                if (subscriptions.TryGetValue(symbolCode, out s))
                    s.ChangeDepth(depth);
                else
                    subscriptions.Add(symbolCode, new Subscription(symbolCode, depth, symbols, rxBuffer));
            });
        }

        public void Unsubscribe(string symbolCode)
        {
            Cl.Execute.OnUIThread(() =>
            {
                Subscription s;
                if (subscriptions.TryGetValue(symbolCode, out s))
                    s.Dispose();
            });
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

        private class Subscription : IRateUpdatesListener
        {
            private SymbolCollectionModel symbols;
            private BufferBlock<FeedUpdate> rxBuffer;

            public Subscription(string symbolCode, int depth, SymbolCollectionModel symbols, BufferBlock<FeedUpdate> rxBuffer)
            {
                this.symbols = symbols;
                this.SymbolCode = symbolCode;
                this.Depth = depth;
                this.rxBuffer = rxBuffer;

                symbols.GetOrDefault(symbolCode)?.Subscribe(this);
            }

            public string SymbolCode { get; private set; }
            public int Depth { get; private set; }
            public event Action DepthChanged = delegate { };

            public void ChangeDepth(int newDepth)
            {
                if (Depth != newDepth)
                {
                    Depth = newDepth;
                    DepthChanged();
                }
            }

            public void OnRateUpdate(Quote tick)
            {
                rxBuffer.Post(new FeedUpdate(tick.Symbol, FdkToAlgo.Convert(tick)));
            }

            public void Dispose()
            {
                symbols.GetOrDefault(SymbolCode)?.Unsubscribe(this);
            }
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
