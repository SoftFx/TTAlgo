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
        private List<BotLogRecord> _events = new List<BotLogRecord>();
        private Dictionary<string, object> _outputBuffers = new Dictionary<string, object>();
        private BarSequenceBuilder _mainBarVector;
        private BarSequenceBuilder _equityBuilder;
        private BarSequenceBuilder _marginBuilder;
        private DateTime _startTime;
        private List<BarEntity> _mainSymbolHistory;
        private List<BarEntity> _equityHistory;
        private List<BarEntity> _marginHistory;
        private TimeFrames _mainTimeframe;

        public BacktesterCollector(PluginExecutor executor)
        {
            _executor = executor;
            //_invokeEmulator.RateUpdated += r => Stats.TicksCount++;

            Stats = new TestingStatistics();
        }

        public TestingStatistics Stats { get; private set; }
        public InvokeEmulator InvokeEmulator { get; internal set; }

        private DateTime VirtualTimepoint => InvokeEmulator.VirtualTimePoint;

        public int EventsCount => _events.Count;
        public int BarCount => _mainSymbolHistory.Count;

        public void OnStart(IBacktesterSettings settings)
        {
            _mainSymbolHistory = new List<BarEntity>();
            _equityHistory = new List<BarEntity>();
            _marginHistory = new List<BarEntity>();

            Stats = new TestingStatistics();
            _startTime = DateTime.UtcNow;
            _mainTimeframe = settings.MainTimeframe;

            _mainBarVector = BarSequenceBuilder.Create(_mainTimeframe);
            _mainBarVector.BarOpened += (b) => _mainSymbolHistory.Add(b);

            _equityBuilder = BarSequenceBuilder.Create(_mainBarVector);
            _equityBuilder.BarOpened += (b) => _equityHistory.Add(b);

            _marginBuilder = BarSequenceBuilder.Create(_mainBarVector);
            _marginBuilder.BarOpened += (b) => _marginHistory.Add(b);
        }

        public void OnStop(IBacktesterSettings settings, AccountAccessor acc)
        {
            if (acc.IsMarginType)
            {
                Stats.InitialBalance = (decimal)settings.InitialBalance;
                Stats.FinalBalance = (decimal)acc.Equity;
            }

            Stats.Elapsed = DateTime.UtcNow - _startTime;
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
            _events = null;

            base.Dispose();
        }

        #region Journal

        public void AddEvent(LogSeverities severity, string message, string description = null)
        {
            _events.Add(new BotLogRecord(VirtualTimepoint, severity, message, description));
        }

        public void LogTrade(string message)
        {
            AddEvent(LogSeverities.TradeSuccess, message);
        }

        public void LogTradeFail(string message)
        {
            AddEvent(LogSeverities.TradeFail, message);
        }

        public IPagedEnumerator<BotLogRecord> GetEvents()
        {
            return _events.GetCrossDomainEnumerator(4000);
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

        public void OnPositionClosed(DateTime timepoint, decimal profit)
        {
            if (profit < 0)
            {
                Stats.GrossLoss -= profit;
                Stats.LossByHours[timepoint.Hour] -= profit;
                Stats.LossByWeekDays[(int)timepoint.DayOfWeek] -= profit;
            }
            else
            {
                Stats.GrossProfit += profit;
                Stats.ProfitByHours[timepoint.Hour] += profit;
                Stats.ProfitByWeekDays[(int)timepoint.DayOfWeek] += profit;
            }
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

        public void OnTick(QuoteEntity tick)
        {
            Stats.TicksCount++;

            _mainBarVector.AppendQuote(tick.Time, tick.Bid, 0);
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
        }

        #endregion
    }
}
