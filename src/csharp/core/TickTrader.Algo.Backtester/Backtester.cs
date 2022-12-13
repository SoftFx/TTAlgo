using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public class Backtester : IDisposable, IPluginSetupTarget, IPluginMetadata, IBacktesterSettings, ITestExecController
    {
        private static int IdSeed;

        private readonly PluginKey _pluginKey;
        private readonly FeedEmulator _feed;
        private readonly PluginExecutorCore _executorCore;
        private readonly EmulationControlFixture _control;

        public Backtester(PluginKey pluginKey, DateTime from, DateTime to)
        {
            _pluginKey = pluginKey ?? throw new ArgumentNullException(nameof(pluginKey));
            var metadata = PackageMetadataCache.GetPlugin(pluginKey) ?? throw new ArgumentException("metadata not found", nameof(pluginKey));
            PluginInfo = metadata.Descriptor;
            _executorCore = new PluginExecutorCore(pluginKey, "Baсktesting-" + Interlocked.Increment(ref IdSeed).ToString());
            _executorCore.Metadata = this;

            CommonSettings.EmulationPeriodStart = from;
            CommonSettings.EmulationPeriodEnd = to;

            _control = InitEmulation();
            _feed = _control.Feed;
            _executorCore.Feed = _feed;
            _executorCore.FeedHistory = _feed;
            _executorCore.InitBarStrategy(Domain.Feed.Types.MarketSide.Bid);

            CommonSettings.Leverage = 100;
            CommonSettings.InitialBalance = 10000;
            CommonSettings.BalanceCurrency = "USD";
            CommonSettings.AccountType = AccountInfo.Types.Type.Gross;

            _control.StateUpdated += s =>
            {
                State = s;
                StateChanged?.Invoke(s);
            };
        }

        public CommonTestSettings CommonSettings { get; } = new CommonTestSettings();

        public PluginDescriptor PluginInfo { get; }
        public int TradesCount => _control.TradeHistory.Count;
        public FeedEmulator Feed => _feed;
        //public TimeSpan ServerPing { get; set; }
        //public int WarmupSize { get; set; } = 10;
        //public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;
        public DateTime? CurrentTimePoint => _control?.EmulationTimePoint;
        public JournalOptions JournalFlags { get; set; } = JournalOptions.Enabled | JournalOptions.WriteInfo | JournalOptions.WriteCustom | JournalOptions.WriteTrade | JournalOptions.WriteAlert;
        public string JournalPath { get; set; }
        public EmulatorStates State { get; private set; }

        public event Action<EmulatorStates> StateChanged;
        public event Action<Exception> ErrorOccurred;
        public event Action<BarData, string, DataSeriesUpdate.Types.Action> OnChartUpdate;
        public event Action<DataSeriesUpdate> OnOutputUpdate;

        public Dictionary<string, TestDataSeriesFlags> SymbolDataConfig { get; } = new Dictionary<string, TestDataSeriesFlags>();
        public TestDataSeriesFlags MarginDataMode { get; set; } = TestDataSeriesFlags.Snapshot;
        public TestDataSeriesFlags EquityDataMode { get; set; } = TestDataSeriesFlags.Snapshot;
        public TestDataSeriesFlags OutputDataMode { get; set; } = TestDataSeriesFlags.Disabled;
        public bool StreamExecReports { get; set; }

        public async Task Run(CancellationToken cToken)
        {
            cToken.Register(() => _control.CancelEmulation());

            await Task.Factory.StartNew(SetupAndRun, TaskCreationOptions.LongRunning);
        }


        private EmulationControlFixture InitEmulation()
        {
            var calcFixture = _executorCore.GetCalcFixture();
            var fixture = new EmulationControlFixture(this, _executorCore, calcFixture, null);

            var iStrategy = fixture.InvokeEmulator;
            calcFixture.OnFatalError = e => fixture.InvokeEmulator.SetFatalError(e);
            _executorCore.InitEmulation(fixture.InvokeEmulator, null, null, null,
                c => new TradeEmulator(c, this, calcFixture, fixture.InvokeEmulator, fixture.Collector, fixture.TradeHistory, PluginInfo.Type),
                fixture.Collector, new TimerApiEmulator(_executorCore, fixture.InvokeEmulator), m => new SimplifiedBuilder(m));
            return fixture;
        }

        private void SetupAndRun()
        {
            _executorCore.InitSlidingBuffering(4000);

            _executorCore.MainSymbolCode = CommonSettings.MainSymbol;
            _executorCore.TimeFrame = CommonSettings.MainTimeframe;
            _executorCore.ModelTimeFrame = CommonSettings.ModelTimeframe;
            _executorCore.Permissions = new PluginPermissions() { TradeAllowed = true };

            _executorCore.OnInternalError += err => _control.Collector.AddEvent(PluginLogRecord.Types.LogSeverity.Error, $"Internal executor error: {err.Exception.Message}");

            bool isRealtime = MarginDataMode.HasFlag(TestDataSeriesFlags.Realtime)
                || EquityDataMode.HasFlag(TestDataSeriesFlags.Realtime)
                || OutputDataMode.HasFlag(TestDataSeriesFlags.Realtime)
                || SymbolDataConfig.Any(s => s.Value.HasFlag(TestDataSeriesFlags.Realtime));

            try
            {
                if (!_control.OnStart())
                {
                    _control.Collector.AddEvent(PluginLogRecord.Types.LogSeverity.Error, "No data for requested period!");
                    return;
                }

                //if (PluginInfo.Type == AlgoTypes.Robot) // no warm-up for indicators
                //{
                //    if (!_control.WarmUp(WarmupSize, WarmupUnits))
                //        return;
                //}

                //_executor.Core.Start();


                if (PluginInfo.IsTradeBot)
                    _control.EmulateExecution(CommonSettings.WarmupSize, CommonSettings.WarmupUnits);
                else // no warm-up for indicators
                    _control.EmulateExecution(0, WarmupUnitTypes.Bars);
            }
            finally
            {
                _control.OnStop();
            }
        }

        public void Pause()
        {
            _control.Pause();
        }

        public void Resume()
        {
            _control.Resume();
        }

        public void CancelTesting()
        {
            _control.CancelEmulation();
        }

        public void SetExecDelay(int delayMs)
        {
            _control.SetExecDelay(delayMs);
        }

        public void Dispose()
        {
            _control.Dispose();
        }

        public TestingStatistics GetStats()
        {
            return _control.Collector.Stats;
        }

        public void SaveResults(string resultsDirPath)
        {
            var descriptor = PluginInfo;
            var mainTimeframe = CommonSettings.MainTimeframe;

            BacktesterResults.Internal.SaveVersionInfo(resultsDirPath);
            BacktesterResults.Internal.SaveStats(resultsDirPath, _control.Collector.Stats);
            BacktesterResults.Internal.SavePluginInfo(resultsDirPath, descriptor);

            // Journal should be already in correct place

            foreach (var symbol in SymbolDataConfig.Keys)
            {
                BacktesterResults.Internal.SaveFeedData(resultsDirPath, symbol, _control.Collector.LocalGetSymbolHistory(symbol, mainTimeframe));
            }

            foreach (var output in descriptor.Outputs)
            {
                var id = output.Id;
                var precision = output.Precision == -1 ? CommonSettings.Symbols[CommonSettings.MainSymbol].Digits : output.Precision;

                BacktesterResults.Internal.SaveOutputData(resultsDirPath, id, precision, _control.Collector.LocalGetOutputData(id));
            }

            if (descriptor.IsTradeBot)
            {
                BacktesterResults.Internal.SaveEquity(resultsDirPath, _control.Collector.LocalGetEquityHistory(mainTimeframe));
                BacktesterResults.Internal.SaveMargin(resultsDirPath, _control.Collector.LocalGetMarginHistory(mainTimeframe));

                BacktesterResults.Internal.SaveTradeHistory(resultsDirPath, _control.TradeHistory.LocalGetReports());
            }
        }


        #region IPluginSetupTarget

        void IPluginSetupTarget.SetParameter(string id, object value)
        {
            _executorCore.SetParameter(id, value);
        }

        T IPluginSetupTarget.GetFeedStrategy<T>()
        {
            return _executorCore.GetFeedStrategy<T>();
        }

        void IPluginSetupTarget.SetupOutput<T>(string id, bool enabled)
        {
            _control.Collector.SetupOutput<T>(id, OutputDataMode);
        }

        #endregion

        #region IPluginMetadata

        IEnumerable<Domain.SymbolInfo> IPluginMetadata.GetSymbolMetadata() => CommonSettings.Symbols.Values;
        IEnumerable<Domain.CurrencyInfo> IPluginMetadata.GetCurrencyMetadata() => CommonSettings.Currencies.Values;
        public IEnumerable<Domain.FullQuoteInfo> GetLastQuoteMetadata() => CommonSettings.Symbols.Values.Select(u => (u.LastQuote as QuoteInfo)?.GetFullQuote());

        #endregion
    }
}
