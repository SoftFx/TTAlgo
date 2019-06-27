using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    /// <summary>
    /// Collects:
    ///     1. Events (user logs + orders)
    ///     2. Outputs
    /// </summary>
    internal class BacktesterCollector : CrossDomainObject, IPluginLogger
    {
        private PluginExecutor _executor;
        private Dictionary<string, IOutputCollector> _outputCollectors = new Dictionary<string, IOutputCollector>();
        private Dictionary<string, FeedSeriesCollector> _symbolDataCollectors = new Dictionary<string, FeedSeriesCollector>();
        //private ChartDataCollector _mainSymbolCollector;
        private ChartDataCollector _equityCollector;
        private ChartDataCollector _marginCollector;
        private DateTime _startTime;
        private string _mainSymbol;
        private TimeFrames _mainTimeframe;
        private string _lastStatus;
        private TimeKeyGenerator _logKeyGen = new TimeKeyGenerator();

        public const string EquityStreamName = "Equity";
        public const string MarginStreamName = "Margin";

        public BacktesterCollector(PluginExecutor executor)
        {
            _executor = executor;
            //_invokeEmulator.RateUpdated += r => Stats.TicksCount++;

            Stats = new TestingStatistics();
        }

        public TestingStatistics Stats { get; private set; }
        public InvokeEmulator InvokeEmulator { get; internal set; }

        private DateTime VirtualTimepoint => InvokeEmulator.UnsafeVirtualTimePoint;

        public void OnStart(IBacktesterSettings settings, FeedEmulator feed)
        {
            Stats = new TestingStatistics();
            _startTime = DateTime.UtcNow;
            _mainSymbol = settings.MainSymbol;
            _mainTimeframe = settings.MainTimeframe;

            InitJournal(settings);

            _lastStatus = null;

            InitChartDataCollection(settings, feed);
            InitOutputCollection(settings);
        }

        public void OnStop(IBacktesterSettings settings, AccountAccessor acc, FeedEmulator feed)
        {
            if (acc != null && acc.IsMarginType)
            {
                Stats.InitialBalance = settings.InitialBalance;
                Stats.FinalBalance = acc.Balance;
                Stats.AccBalanceDigits = acc.BalanceCurrencyInfo.Digits;
            }

            var mainVector = feed?.GetBarBuilder(_mainSymbol, _mainTimeframe, BarPriceType.Bid);
            Stats.BarsCount = mainVector?.Count ?? 0;

            Stats.Elapsed = DateTime.UtcNow - _startTime;

            StopOutputCollectors();

            foreach (var sCollector in _symbolDataCollectors)
                sCollector.Value.Dispose();

            _equityCollector.OnStop();
            _marginCollector.OnStop();

            //if (!string.IsNullOrWhiteSpace(_lastStatus))
            //    AddEvent(LogSeverities.Custom, _lastStatus);
        }

        public override void Dispose()
        {
            Stats = null;

            _symbolDataCollectors.Clear();
            _equityCollector = null;
            _marginCollector = null;
            //_events = null;

            base.Dispose();
        }

        public int GetSymbolHistoryBarCount(string symbol)
        {
            var collector = _symbolDataCollectors.GetOrDefault(symbol);
            if (collector != null)
                return collector.BarCount;
            return 0;
        }

        private void InitChartDataCollection(IBacktesterSettings settings, FeedEmulator feed)
        {
            var mainVector = feed.GetBarBuilder(_mainSymbol, _mainTimeframe, BarPriceType.Bid);

            foreach (var record in settings.SymbolDataConfig)
            {
                var symbol = record.Key;
                var collector = new FeedSeriesCollector(feed, record.Value, symbol, _mainTimeframe, _executor.OnUpdate);
                _symbolDataCollectors.Add(symbol, collector);
            }

            _equityCollector = new ChartDataCollector(settings.EquityDataMode, DataSeriesTypes.NamedStream, EquityStreamName, _executor.OnUpdate, mainVector.Ref);
            _marginCollector = new ChartDataCollector(settings.MarginDataMode, DataSeriesTypes.NamedStream, MarginStreamName, _executor.OnUpdate, mainVector.Ref);
        }

        private void InitOutputCollection(IBacktesterSettings settings)
        {
            var pDescriptor = _executor.GetDescriptor();

            foreach (var outputDescripot in pDescriptor.Outputs)
                SetupOutput(outputDescripot, settings.OutputDataMode);
        }

        private void SetupOutput(OutputDescriptor descriptor, TestDataSeriesFlags flags)
        {
            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": SetupOutput<double>(descriptor, flags); break;
                case "TickTrader.Algo.Api.Marker": SetupOutput<Marker>(descriptor, flags); break;
                default: throw new NotImplementedException("Type " + descriptor.DataSeriesBaseTypeFullName + " is not supported as series base type!");
            }
        }

        private void SetupOutput<T>(OutputDescriptor descriptor, TestDataSeriesFlags flags)
        {
            var fixture = _executor.GetOutput<T>(descriptor.Id);
            var collector = new OutputCollector<T>(descriptor.Id, fixture, _executor.OnUpdate, flags);
            _outputCollectors.Add(descriptor.Id, collector);
        }

        private void StopOutputCollectors()
        {
            foreach (var collector in _outputCollectors.Values)
                collector.Stop();
        }

        #region Journal

        public bool WriteJournal { get; protected set; }
        public bool WriteCustom { get; private set; }
        public bool WriteInfo { get; private set; }
        public bool WriteTrade { get; private set; }
        public bool WriteOrderModifications { get; private set; }

        private void InitJournal(IBacktesterSettings settings)
        {
            var flags = settings.JournalFlags;

            WriteJournal = flags.HasFlag(JournalOptions.Enabled);
            WriteCustom = WriteJournal && flags.HasFlag(JournalOptions.WriteCustom);
            WriteInfo = WriteJournal && flags.HasFlag(JournalOptions.WriteInfo);
            WriteTrade = WriteJournal && flags.HasFlag(JournalOptions.WriteTrade);
            WriteOrderModifications = WriteJournal && WriteTrade && flags.HasFlag(JournalOptions.WriteOrderModifications);
        }

        private bool CheckFilter(LogSeverities severity)
        {
            switch (severity)
            {
                case LogSeverities.Info: return WriteInfo;
                case LogSeverities.Custom: return WriteCustom;
                case LogSeverities.CustomStatus: return false;
                case LogSeverities.Error: return WriteJournal;
                case LogSeverities.Trade: return WriteTrade;
                case LogSeverities.TradeFail: return WriteTrade;
                case LogSeverities.TradeSuccess: return WriteTrade;
            }
            return false;
        }

        public void AddEvent(LogSeverities severity, string message, string description = null)
        {
            if (CheckFilter(severity))
            {
                var record = new BotLogRecord(_logKeyGen.NextKey(VirtualTimepoint), severity, message, description);
                _executor.OnUpdate(record);

                //_events.Add();
            }
        }

        public void LogTrade(string message)
        {
            AddEvent(LogSeverities.TradeSuccess, message);
        }

        public void LogTradeFail(string message)
        {
            AddEvent(LogSeverities.TradeFail, message);
        }

        #endregion

        public IPagedEnumerator<BarEntity> GetSymbolHistory(string symbol, TimeFrames timeFrame)
        {
            var collector = _symbolDataCollectors.GetOrDefault(symbol);
            if (collector != null)
                return MarshalBars(collector.Snapshot, timeFrame);
            return null;
        }

        public IPagedEnumerator<BarEntity> GetEquityHistory(TimeFrames timeFrame)
        {
            return MarshalBars(_equityCollector.Snapshot, timeFrame);
        }

        public IPagedEnumerator<BarEntity> GetMarginHistory(TimeFrames timeFrame)
        {
            return MarshalBars(_marginCollector.Snapshot, timeFrame);
        }

        private IPagedEnumerator<BarEntity> MarshalBars(IEnumerable<BarEntity> barCollection, TimeFrames targeTimeframe)
        {
            const int pageSize = 4000;

            if (_mainTimeframe == targeTimeframe)
                return barCollection.GetCrossDomainEnumerator(pageSize);
            else
                return barCollection.Transform(targeTimeframe).GetCrossDomainEnumerator(pageSize);
        }

        private IPagedEnumerator<T> MarshalLongCollection<T>(IEnumerable<T> collection)
        {
            const int pageSize = 4000;

            return collection.GetCrossDomainEnumerator(pageSize);
        }

        #region Output collection

        public IPagedEnumerator<T> GetOutputData<T>(string id)
        {
            var collector = _outputCollectors[id];
            var data = ((OutputCollector<T>)collector).Snapshot;
            return MarshalLongCollection(data);
        }

        #endregion

        #region Stats collection

        public void OnPositionClosed(DateTime timepoint, double profit, double comission, double swap)
        {
            if (profit < 0)
            {
                Stats.GrossLoss += profit;
                Stats.LossByHours[timepoint.Hour] -= profit;
                Stats.LossByWeekDays[(int)timepoint.DayOfWeek] -= profit;
            }
            else
            {
                Stats.GrossProfit += profit;
                Stats.ProfitByHours[timepoint.Hour] += profit;
                Stats.ProfitByWeekDays[(int)timepoint.DayOfWeek] += profit;
            }

            Stats.TotalComission += comission;
            Stats.TotalSwap += swap;
        }

        public void OnCommisionCharged(double commission)
        {
            Stats.TotalComission += commission;
        }

        public void OnOrderOpened()
        {
            Stats.OrdersOpened++;
        }

        public void OnOrderRejected()
        {
            Stats.OrdersRejected++;
        }

        public void OnOrderModified()
        {
            Stats.OrderModifications++;
        }

        public void OnOrderModificatinRejected()
        {
            Stats.OrderModificationRejected++;
        }

        public void OnRateUpdate(RateUpdate update)
        {
            Stats.TicksCount += update.NumberOfQuotes;
        }

        public void RegisterEquity(DateTime timepoint, double equity, double margin)
        {
            _equityCollector.AppendQuote(equity);
            _marginCollector.AppendQuote(margin);
        }

        #endregion

        #region IPluginLogger

        void IPluginLogger.OnAbort()
        {
        }

        void IPluginLogger.OnError(Exception ex)
        {
            AddEvent(LogSeverities.Error, ex.Message);
        }

        void IPluginLogger.OnError(string message, Exception ex)
        {
            AddEvent(LogSeverities.Error, message);
        }

        void IPluginLogger.OnError(string message)
        {
            AddEvent(LogSeverities.Error, message);
        }

        void IPluginLogger.OnExit()
        {
            AddEvent(LogSeverities.Info, "Bot called Exit()");
        }

        void IPluginLogger.OnInitialized()
        {
            AddEvent(LogSeverities.Info, "Initialized");
        }

        void IPluginLogger.OnPrint(string entry)
        {
            AddEvent(LogSeverities.Custom, entry);
        }

        void IPluginLogger.OnPrint(string entry, params object[] parameters)
        {
            AddEvent(LogSeverities.Custom, string.Format(entry, parameters));
        }

        void IPluginLogger.OnPrintError(string entry)
        {
            AddEvent(LogSeverities.Error, entry);
        }

        void IPluginLogger.OnPrintError(string entry, params object[] parameters)
        {
            AddEvent(LogSeverities.Error, string.Format(entry, parameters));
        }

        void IPluginLogger.OnPrintInfo(string info)
        {
        }

        void IPluginLogger.OnPrintTrade(string entry)
        {

            AddEvent(LogSeverities.Trade, entry);
        }

        void IPluginLogger.OnPrintTradeFail(string entry)
        {
            AddEvent(LogSeverities.TradeFail, entry);
        }

        void IPluginLogger.OnPrintTradeSuccess(string entry)
        {
            AddEvent(LogSeverities.TradeSuccess, entry);
        }

        void IPluginLogger.OnStart()
        {
            AddEvent(LogSeverities.Info, "Start");
        }

        void IPluginLogger.OnStop()
        {
            AddEvent(LogSeverities.Info, "Stop");
        }

        void IPluginLogger.UpdateStatus(string status)
        {
            _lastStatus = status;
        }

        void IPluginLogger.OnConnected()
        {
        }

        void IPluginLogger.OnDisconnected()
        {
        }

        void IPluginLogger.OnConnectionInfo(string connectionInfo)
        {
        }

        #endregion
    }
}
