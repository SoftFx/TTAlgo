using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class PluginBuilder : IPluginContext, EnvironmentInfo, IHelperApi
    {
        private Dictionary<object, IDataBuffer> inputBuffers = new Dictionary<object, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;
        private bool isCurrenciesInitialized;
        private volatile bool isStopped;
        private StatusApiImpl statusApi = new StatusApiImpl();
        private PluginLoggerAdapter logAdapter = new PluginLoggerAdapter();
        internal SynchronizationContextAdapter syncContext = new SynchronizationContextAdapter();
        private TradeApiAdapter _tradeApater;
        private TradeCommands _commands;
        //private bool _isolated;
        private string _instanceId;
        private PluginPermissions _permissions;
        private ICalculatorApi _calc;
        private IndicatorsCollection _indicators;

        internal PluginBuilder(PluginMetadata descriptor)
        {
            Metadata = descriptor;
            marketData = new MarketDataImpl(this);
            Currencies = new CurrenciesCollection();
            Symbols = new SymbolsCollection(marketData, Currencies);
            Account = new AccountAccessor(this);

            PluginProxy = PluginAdapter.Create(descriptor, this);
            Diagnostics = Null.Diagnostics;
            TimerApi = new SimpleTimerFixture();

            syncContext.OnAsyncAction = OnPluginThread;

            _tradeApater = new TradeApiAdapter(Symbols, Account, logAdapter);
            _commands = _tradeApater;

            _permissions = new PluginPermissions { Isolated = true, TradeAllowed = false };

            _indicators = new IndicatorsCollection();

            GetDefaultOptMetric = GetFinalEquity;

            //OnException = ex => Logger.OnError("Exception: " + ex.Message, ex.ToString());
        }

        internal PluginAdapter PluginProxy { get; private set; }
        internal ITimerApi TimerApi { get; set; }
        public string MainSymbol { get; set; }
        public Tuple<string, Feed.Types.MarketSide> MainBufferId { get; set; }
        public SymbolsCollection Symbols { get; private set; }
        public CurrenciesCollection Currencies { get; private set; }
        public int DataSize { get { return PluginProxy.Coordinator.VirtualPos; } }
        public PluginMetadata Metadata { get; private set; }
        public AccountAccessor Account { get; private set; }
        public Action AccountDataRequested { get; set; }
        public Action SymbolDataRequested { get; set; }
        public Action CurrencyDataRequested { get; set; }
        public DiagnosticInfo Diagnostics { get; set; }
        public ITradeApi TradeApi { get => _tradeApater.ExternalApi; set => _tradeApater.ExternalApi = value; }
        public ICalculatorApi Calculator
        {
            get => _calc;
            set
            {
                _calc = value;
                _tradeApater.Calc = value;
            }
        }
        public IPluginLogger Logger { get { return logAdapter.Logger; } set { logAdapter.Logger = value; } }
        public TradeHistory TradeHistoryProvider { get { return Account.HistoryProvider; } set { Account.HistoryProvider = value; } }
        public CustomFeedProvider CustomFeedProvider { get { return marketData.CustomCommds; } set { marketData.CustomCommds = value; } }
        public Action<Exception> OnException { get; set; }
        public Action<Exception> OnInitFailed { get; set; }
        public Action<Action> OnAsyncAction { get; set; }
        public Func<double> GetDefaultOptMetric { get; set; }
        public Action OnExit { get; set; }
        public Action<int> OnInputResize { get; set; }
        public string Status { get { return statusApi.Status; } }
        public string DataFolder { get; set; }
        public string BotDataFolder { get; set; }
        public PluginPermissions Permissions
        {
            get => _permissions;
            set
            {
                _permissions = value ?? throw new InvalidOperationException("Permissions cannot be null!");
                _tradeApater.Permissions = value;
                Account.Isolated = value.Isolated;
            }
        }
        public bool Isolated => _permissions.Isolated;
        //{
        //    get { return _isolated; }
        //    set
        //    {
        //        _isolated = value;
        //        Account.Isolated = _isolated;
        //    }
        //}
        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                _instanceId = value;
                Account.InstanceId = value;
                _tradeApater.IsolationTag = value;
            }
        }
        public Feed.Types.Timeframe TimeFrame { get; set; }

        internal PluginLoggerAdapter LogAdapter => logAdapter;

        public Action<string> StatusUpdated { get { return statusApi.Updated; } set { statusApi.Updated = value; } }

        public IReadOnlyDictionary<object, IDataBuffer> DataBuffers { get { return inputBuffers; } }

        public InputBuffer<T> GetBuffer<T>(string bufferId)
        {
            if (inputBuffers.ContainsKey(bufferId))
                return (InputBuffer<T>)inputBuffers[bufferId];

            InputBuffer<T> buffer = new InputBuffer<T>(PluginProxy.Coordinator);
            inputBuffers.Add(bufferId, buffer);
            return buffer;
        }

        public InputBuffer<T> GetBuffer<T>(object bufferId)
        {
            if (inputBuffers.ContainsKey(bufferId))
                return (InputBuffer<T>)inputBuffers[bufferId];

            InputBuffer<T> buffer = new InputBuffer<T>(PluginProxy.Coordinator);
            inputBuffers.Add(bufferId, buffer);
            return buffer;
        }

        public void SetParameter(string paramName, object value)
        {
            PluginProxy.SetParameter(paramName, value);
        }

        public InputBuffer<BarData> GetBarBuffer(string bufferId)
        {
            return GetBuffer<BarData>(bufferId);
        }

        public InputBuffer<BarData> GetBarBuffer(object bufferId)
        {
            return GetBuffer<BarData>(bufferId);
        }

        public IReaonlyDataBuffer GetOutput(string outputName)
        {
            var outputProxy = PluginProxy.GetOutput(outputName);
            return (IReaonlyDataBuffer)outputProxy.Buffer;
        }

        public OutputBuffer<T> GetOutput<T>(string outputName)
        {
            var outputProxy = PluginProxy.GetOutput(outputName);
            return (OutputBuffer<T>)outputProxy.Buffer;
        }

        public void MapBarInput<TVal>(string inputName, object bufferId, Func<BarData, TVal> selector)
        {
            MapInput(inputName, bufferId, selector);
        }

        public void MapBarInput(string inputName, object bufferId)
        {
            MapInput<BarData, Api.Bar>(inputName, bufferId, b => new BarEntity(b));
        }

        public void MapInput<TSrc, TVal>(string inputName, object bufferId, Func<TSrc, TVal> selector)
        {
            var buffer = GetBuffer<TSrc>(bufferId);
            var input = PluginProxy.GetInput<TVal>(inputName);

            input.Buffer = new ProxyBuffer<TSrc, TVal>(buffer, selector);
        }

        public void MapInput<TSrc, TVal>(string inputName, object bufferId1, object bufferId2, Func<TSrc, TSrc, TVal> selector)
        {
            var buffer1 = GetBuffer<TSrc>(bufferId1);
            var buffer2 = GetBuffer<TSrc>(bufferId2);
            var input = PluginProxy.GetInput<TVal>(inputName);

            input.Buffer = new ProxyBuffer2<TSrc, TVal>(buffer1, buffer2, selector);
        }

        public void MapInput<T>(string inputName, object bufferId)
        {
            var buffer = GetBuffer<T>(bufferId);
            var input = PluginProxy.GetInput<T>(inputName);

            input.Buffer = buffer;
        }

        public void SetStopped()
        {
            isStopped = true;
        }

        private void OnPluginException(Exception ex, bool init)
        {
            Logger.OnError(ex);

            if (init)
                OnInitFailed(ex);

            OnException?.Invoke(ex);
        }

        private void OnPluginThread(SendOrPostCallback callback, object state)
        {
            OnAsyncAction(() => callback(state));
        }

        private Task OnPluginThreadAsync(Action action)
        {
            TaskCompletionSource<object> waithandler = new TaskCompletionSource<object>();
            OnAsyncAction(() =>
            {
                try
                {
                    action();
                    waithandler.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    waithandler.SetException(ex);
                }
            });
            return waithandler.Task;
        }

        #region Emulation

        internal void SetCustomTradeAdapter(TradeCommands adapter)
        {
            _commands = adapter;
        }

        internal double GetFinalEquity()
        {
            return Account.Equity;
        }

        #endregion

        #region IPluginContext

        FeedProvider IPluginContext.Feed => marketData;

        AccountDataProvider IPluginContext.AccountData
        {
            get
            {
                if (!isAccountInitialized)
                {
                    isAccountInitialized = true;
                    AccountDataRequested?.Invoke();
                }
                return Account;
            }
        }

        SymbolProvider IPluginContext.Symbols
        {
            get
            {
                if (!isSymbolsInitialized)
                {
                    isSymbolsInitialized = true;
                    SymbolDataRequested?.Invoke();
                    Symbols.MainSymbolCode = MainSymbol;
                }
                return Symbols;
            }
        }

        CurrencyList IPluginContext.Currencies
        {
            get
            {
                if (!isCurrenciesInitialized)
                {
                    isCurrenciesInitialized = true;
                    CurrencyDataRequested?.Invoke();
                }
                return Currencies;
            }
        }

        TradeCommands IPluginContext.TradeApi => _commands;
        IPluginMonitor IPluginContext.Logger => logAdapter;
        StatusApi IPluginContext.StatusApi => statusApi;
        EnvironmentInfo IPluginContext.Environment => this;
        IHelperApi IPluginContext.Helper => this;
        bool IPluginContext.IsStopped => isStopped;
        ITimerApi IPluginContext.TimerApi => TimerApi;
        TimeFrames IPluginContext.TimeFrame => TimeFrame.ToApiEnum();
        IndicatorProvider IPluginContext.Indicators => _indicators;

        void IPluginContext.OnExit()
        {
            Logger.OnExit();
            OnExit?.Invoke();
        }

        void IPluginContext.OnPluginThread(Action action)
        {
            if (AlgoPlugin.staticContext != null) // we are already on plugin thread!
                action();
            else
                OnPluginThreadAsync(action).Wait();
        }

        void IPluginContext.BeginOnPluginThread(Action action)
        {
            OnAsyncAction(action);
        }

        Task IPluginContext.OnPluginThreadAsync(Action action)
        {
            return OnPluginThreadAsync(action);
        }

        void IPluginContext.SetFeedBufferSize(int newSize)
        {
            OnInputResize?.Invoke(newSize);
        }

        double IPluginContext.DefaultOptimizationMetric => GetDefaultOptMetric();

        #endregion IPluginContext

        #region Invoke

        protected void OnBeforeInvoke()
        {
        }

        protected void OnAfterInvoke()
        {
            statusApi.Apply();
        }

        internal void InvokePluginMethod(Action<PluginBuilder, object> invokeAction, bool initMethod = false)
        {
            var ex = InvokeMethod<object>(invokeAction, null);
            if (ex != null)
                OnPluginException(ex, initMethod);
        }

        internal void InvokePluginMethod<T>(Action<PluginBuilder, T> invokeAction, T param, bool initMethod = false)
        {
            var ex = InvokeMethod(invokeAction, param);
            if (ex != null)
                OnPluginException(ex, initMethod);
        }

        protected virtual Exception InvokeMethod<T>(Action<PluginBuilder, T> invokeAction, T param)
        {
            Exception pluginException = null;

            OnBeforeInvoke();
            var oldContext = SynchronizationContext.Current;
            var oldStatic = AlgoPlugin.staticContext;
            AlgoPlugin.staticContext = this;
            SynchronizationContext.SetSynchronizationContext(syncContext);
            try
            {
                invokeAction(this, param);
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                pluginException = ex;
            }
            AlgoPlugin.staticContext = oldStatic;
            SynchronizationContext.SetSynchronizationContext(oldContext);
            OnAfterInvoke();

            return pluginException;
        }

        internal void InvokeCalculate(bool isUpdate)
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeCalculate(p), isUpdate, false);
        }

        internal void InvokeOnStart()
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeOnStart(), true);
            Logger.OnStart();
        }

        internal void InvokeOnStop()
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeOnStop(), false);
            Logger.OnStop();
        }

        internal void InvokeInit()
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeInit(), true);
            Logger.OnInitialized();
        }

        internal void InvokeOnQuote(Quote quote)
        {
            InvokePluginMethod((b, q) => b.PluginProxy.InvokeOnQuote(q), quote, false);
        }

        internal void InvokeOnModelTick()
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeOnModelTick(), false);
        }

        internal void InvokeAsyncAction(Action asyncAction)
        {
            InvokePluginMethod((b, p) => asyncAction(), false);
        }

        internal async Task InvokeAsyncStop()
        {
            Task result = null;
            InvokePluginMethod((b, p) => result = b.PluginProxy.InvokeAsyncStop(), false);
            try
            {
                await result;
            }
            catch (Exception ex)
            {
                OnPluginException(ex, false);
            }
        }

        internal void FireConnectedEvent()
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeConnectedEvent(), false);
            Logger.OnConnected();
        }

        internal void FireDisconnectedEvent()
        {
            InvokePluginMethod((b, p) => b.PluginProxy.InvokeDisconnectedEvent(), false);
            Logger.OnDisconnected();
        }

        internal void LogConnectionInfo(string connectionInfo)
        {
            Logger.OnConnectionInfo(connectionInfo);
        }

        internal double InvokeGetMetric(out Exception error)
        {
            double result = 0;
            error = InvokeMethod<object>((b, p) => result = b.PluginProxy.InvokeGetMetric(), null);
            return result;
        }

        #endregion

        internal void StartBatch()
        {
            PluginProxy.Coordinator.BeginBatch();
        }

        internal void StopBatch()
        {
            PluginProxy.Coordinator.EndBatch();
        }

        internal void IncreaseVirtualPosition()
        {
            PluginProxy.Coordinator.Extend();
        }

        internal void TruncateBuffers(int bySize)
        {
            PluginProxy.Coordinator.Truncate(bySize);
        }

        #region IHelperApi

        string IHelperApi.FormatPrice(double price, int digits)
        {
            return price.ToString("F" + digits);
        }

        string IHelperApi.FormatPrice(double price, Symbol symbolInfo)
        {
            return price.ToString("F" + symbolInfo.Digits);
        }

        double IHelperApi.RoundVolumeDown(double volume, Symbol symbolInfo)
        {
            double step = symbolInfo.TradeVolumeStep;
            double steps = System.Math.Truncate(volume / step);

            if (steps < symbolInfo.MinTradeVolume)
                return 0;

            return steps * step;
        }

        double IHelperApi.RoundVolumeUp(double volume, Symbol symbolInfo)
        {
            double step = symbolInfo.TradeVolumeStep;
            double steps = System.Math.Ceiling(volume / step);

            if (steps < symbolInfo.MinTradeVolume)
                return symbolInfo.MinTradeVolume;

            return steps * step;
        }

        #endregion

        private class MarketDataImpl : FeedProvider
        {
            private PluginBuilder builder;
            private BarSeriesProxy mainBars;

            public MarketDataImpl(PluginBuilder builder)
            {
                this.builder = builder;
                this.CustomCommds = new NullCustomFeedProvider();
            }

            public BarSeries Bars
            {
                get
                {
                    if (mainBars == null)
                    {
                        mainBars = new BarSeriesProxy();
                        if (!string.IsNullOrEmpty(builder.MainSymbol))
                        {
                            IDataBuffer mainBuffer;
                            if (builder.inputBuffers.TryGetValue(builder.MainBufferId, out mainBuffer))
                            {
                                if (mainBuffer is InputBuffer<BarData>)
                                    mainBars.Buffer = new ProxyBuffer<BarData, Api.Bar>(b => new BarEntity(b)) { SrcBuffer = (InputBuffer<BarData>)mainBuffer };
                                else if (mainBuffer is InputBuffer<QuoteInfo>)
                                    mainBars.Buffer = new QuoteToBarAdapter((InputBuffer<QuoteInfo>)mainBuffer);
                            }
                        }

                        if (mainBars.Buffer == null)
                            mainBars.Buffer = new EmptyBuffer<Bar>();
                    }

                    return mainBars;
                }
            }

            public CustomFeedProvider CustomCommds { get; set; }
        }

        private class StatusApiImpl : StatusApi
        {
            private StringBuilder statusBuilder = new StringBuilder();
            private bool hasChanges;

            public string Status { get; private set; }
            public Action<string> Updated { get; set; }

            public void Apply()
            {
                if (hasChanges)
                {
                    hasChanges = false;
                    Status = statusBuilder.ToString();
                    Updated?.Invoke(Status);
                    statusBuilder.Clear();
                }
            }

            void StatusApi.Flush()
            {
                Apply();
            }

            void StatusApi.Write(string str)
            {
                statusBuilder.Append(str);
                hasChanges = true;
            }

            void StatusApi.Write(string str, object[] strParams)
            {
                statusBuilder.AppendFormat(str, strParams);
                hasChanges = true;
            }

            void StatusApi.WriteLine(string str)
            {
                statusBuilder.Append(str).AppendLine();
                hasChanges = true;
            }

            void StatusApi.WriteLine(string str, object[] strParams)
            {
                statusBuilder.AppendFormat(str, strParams).AppendLine();
                hasChanges = true;
            }

            public void WriteLine()
            {
                statusBuilder.AppendLine();
                hasChanges = true;
            }

            public void Clear()
            {
                statusBuilder.Clear();
                hasChanges = true;
            }
        }
    }
}
