using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

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
        private Dictionary<string, object> _outputBuffers = new Dictionary<string, object>();
        private BarSequenceBuilder _mainBarVector;
        private BarSequenceBuilder _equityBuilder;
        private BarSequenceBuilder _marginBuilder;
        private DateTime _startTime;
        private List<BarEntity> _mainSymbolHistory;
        private List<BarEntity> _equityHistory;
        private List<BarEntity> _marginHistory;
        private string _mainSymbol;
        private TimeFrames _mainTimeframe;
        private string _lastStatus;
        private TimeKeyGenerator _logKeyGen = new TimeKeyGenerator();

        public BacktesterCollector(PluginExecutor executor)
        {
            _executor = executor;
            //_invokeEmulator.RateUpdated += r => Stats.TicksCount++;

            Stats = new TestingStatistics();
        }

        public TestingStatistics Stats { get; private set; }
        public InvokeEmulator InvokeEmulator { get; internal set; }

        private DateTime VirtualTimepoint => InvokeEmulator.SafeVirtualTimePoint;

        //public int EventsCount => _events.Count;
        public int BarCount => _mainSymbolHistory.Count;

        public void OnStart(IBacktesterSettings settings)
        {
            _mainSymbolHistory = new List<BarEntity>();
            _equityHistory = new List<BarEntity>();
            _marginHistory = new List<BarEntity>();

            Stats = new TestingStatistics();
            _startTime = DateTime.UtcNow;
            _mainSymbol = settings.MainSymbol;
            _mainTimeframe = settings.MainTimeframe;

            InitJournal(settings);

            _lastStatus = null;

            _mainBarVector = BarSequenceBuilder.Create(_mainTimeframe);
            _mainBarVector.BarOpened += (b) => _mainSymbolHistory.Add(b);

            _equityBuilder = BarSequenceBuilder.Create(_mainBarVector);
            _equityBuilder.BarOpened += (b) => _equityHistory.Add(b);

            _marginBuilder = BarSequenceBuilder.Create(_mainBarVector);
            _marginBuilder.BarOpened += (b) => _marginHistory.Add(b);

            if (settings.ChartDataMode == BacktesterStreamingModes.Snapshot)
                _mainBarVector.BarOpened += (b) => _mainSymbolHistory.Add(b);
            else if(settings.ChartDataMode == BacktesterStreamingModes.BarCompletion)
                _mainBarVector.BarClosed += (b) => SendUpdate(b, ChartDataType.MainRate);
            else if(settings.ChartDataMode == BacktesterStreamingModes.Realtime)
            {
                _mainBarVector.BarUpdated += (b) => SendUpdate(b, ChartDataType.MainRate);
                _mainBarVector.BarOpened += (b) => SendUpdate(b, ChartDataType.MainRate);
            }

            if (settings.EquityDataMode == BacktesterStreamingModes.Snapshot)
                _equityBuilder.BarOpened += (b) => _equityHistory.Add(b);
            else if (settings.ChartDataMode == BacktesterStreamingModes.BarCompletion)
                _equityBuilder.BarClosed += (b) => SendUpdate(b, ChartDataType.Equity);
            else if (settings.ChartDataMode == BacktesterStreamingModes.Realtime)
            {
                _equityBuilder.BarUpdated += (b) => SendUpdate(b, ChartDataType.Equity);
                _equityBuilder.BarOpened += (b) => SendUpdate(b, ChartDataType.Equity);
            }

            if (settings.MarginDataMode == BacktesterStreamingModes.Snapshot)
                _marginBuilder.BarOpened += (b) => _marginHistory.Add(b);
            else if (settings.ChartDataMode == BacktesterStreamingModes.BarCompletion)
                _marginBuilder.BarClosed += (b) => SendUpdate(b, ChartDataType.Margin);
            else if (settings.ChartDataMode == BacktesterStreamingModes.Realtime)
            {
                _marginBuilder.BarUpdated += (b) => SendUpdate(b, ChartDataType.Margin);
                _marginBuilder.BarOpened += (b) => SendUpdate(b, ChartDataType.Margin);
            }
        }

        public void OnStop(IBacktesterSettings settings, AccountAccessor acc)
        {
            if (acc.IsMarginType)
            {
                Stats.InitialBalance = (decimal)settings.InitialBalance;
                Stats.FinalBalance = (decimal)acc.Balance;
                Stats.AccBalanceDigits = acc.BalanceCurrencyInfo.Digits;
            }

            Stats.Elapsed = DateTime.UtcNow - _startTime;

            //if (!string.IsNullOrWhiteSpace(_lastStatus))
            //    AddEvent(LogSeverities.Custom, _lastStatus);
        }

        public override void Dispose()
        {
            Stats = null;

            _mainSymbolHistory = null;
            _equityHistory = null;
            _marginHistory = null;

            _mainBarVector = null;
            _equityBuilder = null;
            _marginBuilder = null;
            //_events = null;

            base.Dispose();
        }

        private void SendUpdate(BarEntity bar, ChartDataType type)
        {
            _executor.OnUpdate(new ChartDataUpdate(type, bar));
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

        public IPagedEnumerator<BarEntity> GetMainSymbolHistory(TimeFrames timeFrame)
        {
            return MarshalBars(_mainSymbolHistory, timeFrame);
        }

        public IPagedEnumerator<BarEntity> GetEquityHistory(TimeFrames timeFrame)
        {
            return MarshalBars(_equityHistory, timeFrame);
        }

        public IPagedEnumerator<BarEntity> GetMarginHistory(TimeFrames timeFrame)
        {
            return MarshalBars(_marginHistory, timeFrame);
        }

        private IPagedEnumerator<BarEntity> MarshalBars(IEnumerable<BarEntity> barCollection, TimeFrames targeTimeframe)
        {
            const int pageSize = 4000;

            if (_mainTimeframe == targeTimeframe)
                return barCollection.GetCrossDomainEnumerator(pageSize);
            else
                return barCollection.Transform(targeTimeframe).GetCrossDomainEnumerator(pageSize);
        }

        #endregion

        #region Output collection

        public void InitOutputCollection<T>(string id)
        {
            var output = _executor.GetOutput<T>(id);
            var outputBuffer = new List<T>();

            output.Appended += a =>
            {
                outputBuffer.Add(a.Value);
            };
        }

        public List<T> GetOutputBuffer<T>(string id)
        {
            return (List<T>)_outputBuffers[id];
        }

        #endregion

        #region Stats collection

        public void OnPositionClosed(DateTime timepoint, decimal profit, decimal comission, decimal swap)
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

        public void OnCommisionCharged(decimal commission)
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

        public void OnBufferExtended(int by)
        {
            Stats.BarsCount += by;
        }

        public void OnRateUpdate(RateUpdate update)
        {
            Stats.TicksCount += update.NumberOfQuotes;

            if (update.Symbol == _mainSymbol)
                _mainBarVector.AppendBarPart(update.Time, update.BidOpen, update.BidHigh, update.BidLow, update.Bid, 0);
        }

        public void RegisterEquity(DateTime timepoint, double equity, double margin)
        {
            _equityBuilder.AppendQuote(timepoint, equity, 0);
            _marginBuilder.AppendQuote(timepoint, margin, 0);
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
