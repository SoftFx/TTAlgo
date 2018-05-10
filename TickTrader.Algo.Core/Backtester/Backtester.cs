using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class Backtester : IDisposable
    {
        private AlgoPluginRef _pluginRef;
        private FeedEmulator _feed = new FeedEmulator();
        private PluginExecutor _executor;
        private EmulationControlFixture _control;

        public Backtester(AlgoPluginRef pluginRef)
        {
            _pluginRef = pluginRef ?? throw new ArgumentNullException("pluginRef");
        }

        public string MainSymbol { get; set; }
        public TimeFrames MainTimeframe { get; set; }
        public DateTime? EmulationPeriodStart { get; set; }
        public DateTime? EmulationPeriodEnd { get; set; }
        public int EventsCount => _control?.Collector.EventsCount ?? 0;

        public DateTime? CurrentTimePoint => _control?.EmulationTimePoint;

        public void Run(CancellationToken cToken)
        {
            Dispose();

            _executor = _pluginRef.CreateExecutor();
            _executor.InitSlidingBuffering(4000);

            _executor.MainSymbolCode = MainSymbol;
            _executor.TimeFrame = MainTimeframe;
            _executor.InitBarStrategy(_feed, Api.BarPriceType.Bid);
            _control = _executor.InitEmulation(EmulationPeriodStart ?? DateTime.MinValue);
            
            _executor.Start();

            _control.EmulateExecution();
        }

        public IPagedEnumerator<BotLogRecord> GetEvents()
        {
            return _control.Collector.GetEvents();
        }

        public void AddFeed(string symbol, IEnumerable<QuoteEntity> stream)
        {
            _feed.AddFeedSource(symbol, new BacktesterTickFeed(stream));
        }

        public void AddFeed(string symbol, IEnumerable<BarEntity> bidStream, IEnumerable<BarEntity> askStream)
        {
            _feed.AddFeedSource(symbol, new BacktesterBarFeed(symbol, bidStream, askStream));
        }

        public void Dispose()
        {
            _control?.Dispose();
            _control = null;
            _executor?.Dispose();
            _executor = null;
        }
    }
}
