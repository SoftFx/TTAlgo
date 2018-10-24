using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    public class Backtester : CrossDomainObject, IDisposable, IPluginSetupTarget, IPluginMetadata, IBacktesterSettings
    {
        private static int IdSeed;

        //private AlgoPluginRef _pluginRef;
        private readonly FeedEmulator _feed;
        private readonly PluginExecutor _executor;
        private EmulationControlFixture _control;
        private Dictionary<string, double> _initialAssets = new Dictionary<string, double>();
        private Dictionary<string, SymbolEntity> _symbols = new Dictionary<string, SymbolEntity>();
        private Dictionary<string, CurrencyEntity> _currencies = new Dictionary<string, CurrencyEntity>();

        public Backtester(AlgoPluginRef pluginRef, DateTime? from, DateTime? to)
        {
            pluginRef = pluginRef ?? throw new ArgumentNullException("pluginRef");
            _executor = pluginRef.CreateExecutor();
            _executor.Metadata = this;

            EmulationPeriodStart = from;
            EmulationPeriodEnd = to;

            _control = _executor.InitEmulation(this);
            _feed = _control.Feed;
            _executor.InitBarStrategy(_feed, Api.BarPriceType.Bid);

            Leverage = 100;
            InitialBalance = 10000;
            BalanceCurrency = "USD";
            AccountType = AccountTypes.Gross;
        }

        public string MainSymbol { get; set; }
        public AccountTypes AccountType { get; set; }
        public string BalanceCurrency { get; set; }
        public int Leverage { get; set; }
        public double InitialBalance { get; set; }
        public Dictionary<string, double> InitialAssets => _initialAssets;
        public Dictionary<string, SymbolEntity> Symbols => _symbols;
        public Dictionary<string, CurrencyEntity> Currencies => _currencies;
        public TimeFrames MainTimeframe { get; set; }
        public DateTime? EmulationPeriodStart { get; }
        public DateTime? EmulationPeriodEnd { get; }
        public int EventsCount => _control.Collector.EventsCount;
        public int BarHistoryCount => _control.Collector.BarCount;
        public FeedEmulator Feed => _feed;
        public TimeSpan ServerPing { get; set; }
        public int WarmupSize { get; set; } = 10;
        public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;
        public DateTime? CurrentTimePoint => _control?.EmulationTimePoint;
        public JournalOptions JournalFlags { get; set; } = JournalOptions.Enabled | JournalOptions.WriteInfo | JournalOptions.WriteCustom | JournalOptions.WriteTrade;

        public void Run(CancellationToken cToken)
        {
            cToken.Register(() => _control.CancelEmulation());

            _executor.InitSlidingBuffering(4000);

            _executor.MainSymbolCode = MainSymbol;
            _executor.TimeFrame = MainTimeframe;
            _executor.InstanceId = "Baktesting-" + Interlocked.Increment(ref IdSeed).ToString();
            _executor.Permissions = new PluginPermissions() { TradeAllowed = true };

            _control.OnStart();

            if (!_control.WarmUp(WarmupSize, WarmupUnits))
                return;

            _executor.Start();

            try
            {
                _control.EmulateExecution();
            }
            finally
            {
                _control.OnStop();
            }
        }

        public void CancelTesting()
        {
            _control.CancelEmulation();
        }

        public IPagedEnumerator<BotLogRecord> GetEvents()
        {
            return _control.Collector.GetEvents();
        }

        public IPagedEnumerator<BarEntity> GetMainSymbolHistory(TimeFrames timeFrame)
        {
            return _control.Collector.GetMainSymbolHistory(timeFrame);
        }

        public IPagedEnumerator<BarEntity> GetEquityHistory(TimeFrames timeFrame)
        {
            return _control.Collector.GetEquityHistory(timeFrame);
        }

        public IPagedEnumerator<BarEntity> GetMarginHistory(TimeFrames timeFrame)
        {
            return _control.Collector.GetMarginHistory(timeFrame);
        }

        public void InitOutputCollection<T>(string id)
        {
            _control.Collector.InitOutputCollection<T>(id);
        }

        public List<T> GetOutputBuffer<T>(string id)
        {
            return _control.Collector.GetOutputBuffer<T>(id);
        }

        public override void Dispose()
        {
            base.Dispose();

            _executor?.Dispose();
            _control?.Dispose();
            _control = null;
        }

        public TestingStatistics GetStats()
        {
            return _control.Collector.Stats;
        }

        #region IPluginSetupTarget

        void IPluginSetupTarget.SetParameter(string id, object value)
        {
            _executor.SetParameter(id, value);
        }

        T IPluginSetupTarget.GetFeedStrategy<T>()
        {
            return _executor.GetFeedStrategy<T>();
        }

        void IPluginSetupTarget.MapInput(string inputName, string symbolCode, Mapping mapping)
        {
            _executor.MapInput(inputName, symbolCode, mapping);
        }

        #endregion

        #region IPluginMetadata

        IEnumerable<SymbolEntity> IPluginMetadata.GetSymbolMetadata() => _symbols.Values;
        IEnumerable<CurrencyEntity> IPluginMetadata.GetCurrencyMetadata() => _currencies.Values;

        #endregion
    }
}
