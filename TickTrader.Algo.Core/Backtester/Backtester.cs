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

        private readonly FeedEmulator _feed;
        private readonly ExecutorHandler _executor;
        private EmulationControlFixture _control;
        private Dictionary<string, double> _initialAssets = new Dictionary<string, double>();
        private Dictionary<string, SymbolEntity> _symbols = new Dictionary<string, SymbolEntity>();
        private Dictionary<string, CurrencyEntity> _currencies = new Dictionary<string, CurrencyEntity>();

        public Backtester(AlgoPluginRef pluginRef, ISynchronizationContext updateSync, DateTime? from, DateTime? to)
        {
            pluginRef = pluginRef ?? throw new ArgumentNullException("pluginRef");
            _executor = new ExecutorHandler(pluginRef, updateSync);
            _executor.Core.Metadata = this;

            EmulationPeriodStart = from;
            EmulationPeriodEnd = to;

            _control = _executor.Core.InitEmulation(this);
            _feed = _control.Feed;
            _executor.Core.InitBarStrategy(_feed, Api.BarPriceType.Bid);

            Leverage = 100;
            InitialBalance = 10000;
            BalanceCurrency = "USD";
            AccountType = AccountTypes.Gross;
        }

        public ExecutorHandler Executor => _executor;
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
        public int TradesCount => _control.TradeHistory.Count;
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

            _executor.Core.InitSlidingBuffering(4000);

            _executor.Core.MainSymbolCode = MainSymbol;
            _executor.Core.TimeFrame = MainTimeframe;
            _executor.Core.InstanceId = "Baktesting-" + Interlocked.Increment(ref IdSeed).ToString();
            _executor.Core.Permissions = new PluginPermissions() { TradeAllowed = true };

            _control.OnStart();
            _executor.Start();

            if (!_control.WarmUp(WarmupSize, WarmupUnits))
                return;

            _executor.Core.Start();

            try
            {
                _control.EmulateExecution();
            }
            finally
            {
                _control.OnStop();
                _executor.Stop();
            }
        }

        public void CancelTesting()
        {
            _control.CancelEmulation();
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

        public IPagedEnumerator<TradeReportEntity> GetTradeHistory()
        {
            return _control.TradeHistory.Marshal();
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
            _executor.Core.SetParameter(id, value);
        }

        T IPluginSetupTarget.GetFeedStrategy<T>()
        {
            return _executor.Core.GetFeedStrategy<T>();
        }

        void IPluginSetupTarget.MapInput(string inputName, string symbolCode, Mapping mapping)
        {
            _executor.Core.MapInput(inputName, symbolCode, mapping);
        }

        #endregion

        #region IPluginMetadata

        IEnumerable<SymbolEntity> IPluginMetadata.GetSymbolMetadata() => _symbols.Values;
        IEnumerable<CurrencyEntity> IPluginMetadata.GetCurrencyMetadata() => _currencies.Values;

        #endregion
    }
}
