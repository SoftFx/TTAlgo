using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    /// <summary>
    /// Collects:
    ///     1. Events (user logs + orders)
    ///     2. Outputs
    /// </summary>
    internal class BacktesterCollector : IPluginLogger
    {
        private PluginExecutorCore _executor;
        private Dictionary<string, IOutputCollector> _outputCollectors = new Dictionary<string, IOutputCollector>();
        private Dictionary<string, FeedSeriesCollector> _symbolDataCollectors = new Dictionary<string, FeedSeriesCollector>();
        //private ChartDataCollector _mainSymbolCollector;
        private ChartDataCollector _equityCollector;
        private ChartDataCollector _marginCollector;
        private DateTime _startTime;
        private string _mainSymbol;
        private Feed.Types.Timeframe _mainTimeframe;
        private string _lastStatus;
        private TimeKeyGenerator _logKeyGen = new TimeKeyGenerator();
        private StreamWriter _journalWriter;
        private CsvWriter _journalCsvWriter;

        public const string EquityStreamName = "Equity";
        public const string MarginStreamName = "Margin";

        public BacktesterCollector(PluginExecutorCore executor)
        {
            _executor = executor;
            //_invokeEmulator.RateUpdated += r => Stats.TicksCount++;

            Stats = new TestingStatistics();
        }

        public TestingStatistics Stats { get; private set; }
        public InvokeEmulator InvokeEmulator { get; internal set; }

        internal int EquityHistorySize => _equityCollector.Count;

        private DateTime VirtualTimepoint => InvokeEmulator.UnsafeVirtualTimePoint;

        public void OnStart(IBacktesterSettings settings, FeedEmulator feed)
        {
            Stats = new TestingStatistics();
            _startTime = DateTime.UtcNow;
            _mainSymbol = settings.CommonSettings.MainSymbol;
            _mainTimeframe = settings.CommonSettings.MainTimeframe;

            InitJournal(settings);

            _lastStatus = null;

            InitChartDataCollection(settings, feed);
        }

        public void OnStop(IBacktesterSettings settings, AccountAccessor acc, FeedEmulator feed)
        {
            if (acc != null && acc.IsMarginType)
            {
                Stats.InitialBalance = settings.CommonSettings.InitialBalance;
                Stats.FinalBalance = (double)acc.Balance;
                Stats.AccBalanceDigits = acc.BalanceCurrencyInfo?.Digits ?? 00;
            }

            var mainVector = feed?.GetBarBuilder(_mainSymbol, _mainTimeframe, Feed.Types.MarketSide.Bid);
            Stats.BarsCount = mainVector?.Count ?? 0;

            Stats.ElapsedMs = (DateTime.UtcNow - _startTime).TotalMilliseconds;

            StopOutputCollectors();

            foreach (var sCollector in _symbolDataCollectors)
                sCollector.Value.Dispose();

            _equityCollector.OnStop();
            _marginCollector.OnStop();

            //if (!string.IsNullOrWhiteSpace(_lastStatus))
            //    AddEvent(LogSeverities.Custom, _lastStatus);

            if (_journalWriter != null)
            {
                _journalCsvWriter.Flush();
                _journalCsvWriter.Dispose();
                _journalWriter.Dispose();
            }
        }

        public void Dispose()
        {
            Stats = null;

            _symbolDataCollectors.Clear();
            _equityCollector = null;
            _marginCollector = null;
            //_events = null;
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
            var mainVector = feed.GetBarBuilder(_mainSymbol, _mainTimeframe, Feed.Types.MarketSide.Bid);

            foreach (var record in settings.SymbolDataConfig)
            {
                var symbol = record.Key;
                var collector = new FeedSeriesCollector(feed, record.Value, symbol, _mainTimeframe, _executor.OnUpdate);
                _symbolDataCollectors.Add(symbol, collector);
            }

            _equityCollector = new ChartDataCollector(settings.EquityDataMode, BarSeriesUpdate.Types.Type.NamedStream, EquityStreamName, _executor.OnUpdate, mainVector.Ref);
            _marginCollector = new ChartDataCollector(settings.MarginDataMode, BarSeriesUpdate.Types.Type.NamedStream, MarginStreamName, _executor.OnUpdate, mainVector.Ref);
        }

        internal void SetupOutput<T>(string outputId, TestDataSeriesFlags flags)
        {
            var fixture = _executor.GetOutput<T>(outputId);
            var collector = new OutputCollector<T>(outputId, fixture, _executor.OnUpdate, flags);
            _outputCollectors.Add(outputId, collector);
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
        public bool WriteAlert { get; private set; }

        private void InitJournal(IBacktesterSettings settings)
        {
            var flags = settings.JournalFlags;

            WriteJournal = flags.HasFlag(JournalOptions.Enabled);
            WriteCustom = WriteJournal && flags.HasFlag(JournalOptions.WriteCustom);
            WriteInfo = WriteJournal && flags.HasFlag(JournalOptions.WriteInfo);
            WriteTrade = WriteJournal && flags.HasFlag(JournalOptions.WriteTrade);
            WriteOrderModifications = WriteJournal && WriteTrade && flags.HasFlag(JournalOptions.WriteOrderModifications);
            WriteAlert = WriteJournal && flags.HasFlag(JournalOptions.WriteAlert);

            if (WriteJournal)
            {
                _journalWriter = new StreamWriter(settings.JournalPath, false);
                _journalCsvWriter = new CsvWriter(_journalWriter, CultureInfo.InvariantCulture);
                _journalCsvWriter.Context.RegisterClassMap<CsvMapping.ForLogRecord>();
                _journalCsvWriter.WriteHeader<PluginLogRecord>();
            }
        }

        private bool CheckFilter(PluginLogRecord.Types.LogSeverity severity)
        {
            switch (severity)
            {
                case PluginLogRecord.Types.LogSeverity.Info: return WriteInfo;
                case PluginLogRecord.Types.LogSeverity.Custom: return WriteCustom;
                case PluginLogRecord.Types.LogSeverity.Error: return WriteJournal;
                case PluginLogRecord.Types.LogSeverity.Trade: return WriteTrade;
                case PluginLogRecord.Types.LogSeverity.TradeFail: return WriteTrade;
                case PluginLogRecord.Types.LogSeverity.TradeSuccess: return WriteTrade;
                case PluginLogRecord.Types.LogSeverity.Alert: return WriteAlert;
            }
            return false;
        }

        public void AddEvent(PluginLogRecord.Types.LogSeverity severity, string message, string details = null)
        {
            if (CheckFilter(severity))
            {
                var timeUtc = _logKeyGen.NextKey(VirtualTimepoint);

                // Streaming refactoring for visualizer
                var record = new PluginLogRecord(timeUtc, severity, message, details);
                //_executor.OnUpdate(record);

                _journalCsvWriter.NextRecord();
                _journalCsvWriter.WriteRecord(record);
            }
        }

        public void LogTrade(string message)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.TradeSuccess, message);
        }

        public void LogTradeFail(string message)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.TradeFail, message);
        }

        #endregion

        private IEnumerable<BarData> TransformBars(IEnumerable<BarData> bars, Feed.Types.Timeframe targetTimeframe)
        {
            return _mainTimeframe == targetTimeframe ? bars : bars.Transform(targetTimeframe);
        }

        internal IEnumerable<BarData> LocalGetEquityHistory(Feed.Types.Timeframe targetTimeframe)
        {
            return TransformBars(_equityCollector.Snapshot, targetTimeframe);
        }

        internal IEnumerable<BarData> LocalGetMarginHistory(Feed.Types.Timeframe targetTimeframe)
        {
            return TransformBars(_marginCollector.Snapshot, targetTimeframe);
        }

        internal IEnumerable<BarData> LocalGetSymbolHistory(string symbol, Feed.Types.Timeframe targetTimeframe)
        {
            if (!_symbolDataCollectors.TryGetValue(symbol, out var collector))
                return Enumerable.Empty<BarData>();

            return TransformBars(collector.Snapshot, targetTimeframe);
        }

        #region Output collection

        internal IReadOnlyList<OutputPoint> LocalGetOutputData(string id)
        {
            if (!_outputCollectors.TryGetValue(id, out var collector))
                return new List<OutputPoint>();

            return collector.Snapshot;
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

        public void OnRateUpdate(IRateInfo update)
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
            AddEvent(PluginLogRecord.Types.LogSeverity.Error, ex.Message);
        }

        void IPluginLogger.OnError(string message, Exception ex)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Error, message);
        }

        void IPluginLogger.OnError(string message)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Error, message);
        }

        void IPluginLogger.OnExit()
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Info, "Bot called Exit()");
        }

        void IPluginLogger.OnInitialized()
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Info, "Initialized");
        }

        void IPluginLogger.OnPrint(string entry)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Custom, entry);
        }

        void IPluginLogger.OnPrint(string entry, params object[] parameters)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Custom, string.Format(entry, parameters));
        }

        void IPluginLogger.OnPrintError(string entry)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Error, entry);
        }

        void IPluginLogger.OnPrintError(string entry, params object[] parameters)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Error, string.Format(entry, parameters));
        }

        void IPluginLogger.OnPrintInfo(string info)
        {
        }

        void IPluginLogger.OnPrintTrade(string entry)
        {

            AddEvent(PluginLogRecord.Types.LogSeverity.Trade, entry);
        }

        void IPluginLogger.OnPrintTradeFail(string entry)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.TradeFail, entry);
        }

        void IPluginLogger.OnPrintTradeSuccess(string entry)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.TradeSuccess, entry);
        }

        void IPluginLogger.OnStart()
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Info, "Start");
        }

        void IPluginLogger.OnStop()
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Info, "Stop");
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

        void IPluginLogger.OnPrintAlert(string entry)
        {
            AddEvent(PluginLogRecord.Types.LogSeverity.Alert, entry);
        }

        #endregion
    }
}
