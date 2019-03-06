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

            InitChartDataCollection(settings);
            InitOutputCollection(settings);
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

            StopOutputCollectors();

            _mainBarVector.CloseSequence();
            _equityBuilder.CloseSequence();
            _marginBuilder.CloseSequence();

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

        private void InitChartDataCollection(IBacktesterSettings settings)
        {
            _mainBarVector = BarSequenceBuilder.Create(_mainTimeframe);
            _equityBuilder = BarSequenceBuilder.Create(_mainBarVector);
            _marginBuilder = BarSequenceBuilder.Create(_mainBarVector);

            InitSeriesCollection(settings.ChartDataMode, _mainBarVector, DataSeriesTypes.SymbolRate, settings.MainSymbol, _mainSymbolHistory);
            InitSeriesCollection(settings.EquityDataMode, _equityBuilder, DataSeriesTypes.NamedStream, EquityStreamName, _equityHistory);
            InitSeriesCollection(settings.MarginDataMode, _marginBuilder, DataSeriesTypes.NamedStream, MarginStreamName, _marginHistory);
        }

        private void InitSeriesCollection(TestDataSeriesFlags seriesFlags, BarSequenceBuilder builder, DataSeriesTypes dataType, string seriesId, List<BarEntity> snapshot)
        {
            if (seriesFlags.HasFlag(TestDataSeriesFlags.Snapshot))
                builder.BarOpened += (b) => snapshot.Add(b);
            if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
            {
                if (seriesFlags.HasFlag(TestDataSeriesFlags.Realtime))
                    builder.BarClosed += (b) => SendUpdate(b, dataType, seriesId, SeriesUpdateActions.Append);
                else
                {
                    builder.BarUpdated += (b) => SendUpdate(b, dataType, seriesId, SeriesUpdateActions.Update);
                    builder.BarOpened += (b) => SendUpdate(b, dataType, seriesId, SeriesUpdateActions.Append);
                }
            }
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

        private void SendUpdate(BarEntity bar, DataSeriesTypes type, string streamId, SeriesUpdateActions action)
        {
            //System.Diagnostics.Debug.WriteLine("BAR - " + bar.OpenTime);

            var update = new DataSeriesUpdate<BarEntity>(type, streamId, action, bar);
            _executor.OnUpdate(update);
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

        private IPagedEnumerator<T> MarshalLongCollection<T>(IEnumerable<T> collection)
        {
            const int pageSize = 4000;

            return collection.GetCrossDomainEnumerator(pageSize);
        }

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

        public IPagedEnumerator<T> GetOutputData<T>(string id)
        {
            var collector = _outputCollectors[id];
            var data = ((OutputCollector<T>)collector).Snapshot;
            return MarshalLongCollection(data);
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
