﻿using System;
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

        private ISynchronizationContext _sync;
        private readonly FeedEmulator _feed;
        private readonly ExecutorHandler _executor;
        private readonly EmulationControlFixture _control;
        private Dictionary<string, double> _initialAssets = new Dictionary<string, double>();
        private Dictionary<string, SymbolEntity> _symbols = new Dictionary<string, SymbolEntity>();
        private Dictionary<string, CurrencyEntity> _currencies = new Dictionary<string, CurrencyEntity>();

        public Backtester(AlgoPluginRef pluginRef, ISynchronizationContext syncObj, DateTime? from, DateTime? to)
        {
            pluginRef = pluginRef ?? throw new ArgumentNullException("pluginRef");
            PluginInfo = pluginRef.Metadata.Descriptor;
            _sync = syncObj;
            _executor = new ExecutorHandler(pluginRef, syncObj);
            _executor.Core.Metadata = this;

            EmulationPeriodStart = from;
            EmulationPeriodEnd = to;

            _control = _executor.Core.InitEmulation(this, PluginInfo.Type);
            _feed = _control.Feed;
            _executor.Core.InitBarStrategy(_feed, Api.BarPriceType.Bid);

            Leverage = 100;
            InitialBalance = 10000;
            BalanceCurrency = "USD";
            AccountType = AccountTypes.Gross;

            _control.StateUpdated += s => _sync.Send(() =>
            {
                State = s;
                StateChanged?.Invoke(s);
            });
        }

        public ExecutorHandler Executor => _executor;
        public PluginDescriptor PluginInfo { get; }
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
        public FeedEmulator Feed => _feed;
        public TimeSpan ServerPing { get; set; }
        public int WarmupSize { get; set; } = 10;
        public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;
        public DateTime? CurrentTimePoint => _control?.EmulationTimePoint;
        public JournalOptions JournalFlags { get; set; } = JournalOptions.Enabled | JournalOptions.WriteInfo | JournalOptions.WriteCustom | JournalOptions.WriteTrade;
        public EmulatorStates State { get; private set; }
        public event Action<EmulatorStates> StateChanged;

        public event Action<BarEntity, string, SeriesUpdateActions> OnChartUpdate
        {
            add { Executor.ChartBarUpdated += value; }
            remove { Executor.ChartBarUpdated -= value; }
        }

        public event Action<IDataSeriesUpdate> OnOutputUpdate
        {
            add { Executor.OutputUpdate += value; }
            remove { Executor.OutputUpdate -= value; }
        }

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

        private void SetupAndRun()
        {
            _executor.Core.InitSlidingBuffering(4000);

            _executor.Core.MainSymbolCode = MainSymbol;
            _executor.Core.TimeFrame = MainTimeframe;
            _executor.Core.InstanceId = "Baktesting-" + Interlocked.Increment(ref IdSeed).ToString();
            _executor.Core.Permissions = new PluginPermissions() { TradeAllowed = true };

            bool isRealtime = MarginDataMode.IsFlagSet(TestDataSeriesFlags.Realtime) | EquityDataMode.IsFlagSet(TestDataSeriesFlags.Realtime)
                | OutputDataMode.IsFlagSet(TestDataSeriesFlags.Realtime) | SymbolDataConfig.Any(s => s.Value.IsFlagSet(TestDataSeriesFlags.Realtime));

            _executor.StartCollection(isRealtime);

            if (!_control.OnStart())
            {
                _control.Collector.AddEvent(LogSeverities.Error, "No data for requested period!");
                _executor.StopCollection();
                return;
            }

            //if (PluginInfo.Type == AlgoTypes.Robot) // no warm-up for indicators
            //{
            //    if (!_control.WarmUp(WarmupSize, WarmupUnits))
            //        return;
            //}

            //_executor.Core.Start();

            try
            {
                if (PluginInfo.Type == AlgoTypes.Robot)
                    _control.EmulateExecution(WarmupSize, WarmupUnits);
                else // no warm-up for indicators
                    _control.EmulateExecution(0, WarmupUnitTypes.Bars);
            }
            finally
            {
                _control.OnStop();
                _executor.StopCollection();
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

        public int GetSymbolHistoryBarCount(string symbol)
        {
            return _control.Collector.GetSymbolHistoryBarCount(symbol);
        }

        public IPagedEnumerator<BarEntity> GetSymbolHistory(string symbol, TimeFrames timeFrame)
        {
            return _control.Collector.GetSymbolHistory(symbol, timeFrame);
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

        public IPagedEnumerator<T> GetOutputData<T>(string id)
        {
            return _control.Collector.GetOutputData<T>(id);
        }

        public override void Dispose()
        {
            base.Dispose();

            _executor?.Dispose();
            _control.Dispose();
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