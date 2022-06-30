using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class PluginExecutorCore : IFixtureContext, IPluginSetupTarget, DiagnosticInfo
    {
        public enum States { Idle, Running, Stopping }

        private object _sync = new object();
        private LogFixture _pluginLoggerFixture;
        private IPluginLogger _pluginLogger = Null.Logger;
        private IPluginMetadata metadata;
        private IFeedProvider _feedProvider;
        private IFeedHistoryProvider _feedHistorySrc;
        private ITradeHistoryProvider tradeHistoryProvider;
        private FeedStrategy _fStrategy;
        private FeedBufferStrategy _bStrategy;
        private readonly SubscriptionFixtureManager _dispenser;
        private InvokeStartegy iStrategy;
        private readonly CalculatorFixture _calcFixture;
        private readonly MarketStateFixture _marketFixture;
        private IExecutorFixture accFixture;
        private IExecutorFixture _timerFixture;
        private readonly StatusFixture _statusFixture;
        private IAccountInfoProvider _externalAccData;
        private ITradeExecutor _externalTradeApi;
        private string _mainSymbol;
        private Func<PluginMetadata, PluginBuilder> _builderFactory = m => new PluginBuilder(m);
        private PluginBuilder _builder;
        private Feed.Types.Timeframe _timeframe;
        private Feed.Types.Timeframe _modelTimeframe;
        private List<Action> setupActions = new List<Action>();
        private readonly PluginMetadata _metadata;
        private Dictionary<string, IOutputFixture> outputFixtures = new Dictionary<string, IOutputFixture>();
        private Task stopTask;
        private string workingFolder;
        private string botWorkingFolder;
        private string _instanceId;
        private PluginPermissions _permissions;
        private States state;
        private Func<IFixtureContext, IExecutorFixture> _tradeFixtureFactory = c => new TradingFixture(c);
        private bool _enableUpdateMarshaling = true;

        private bool InRunningState => state == States.Running;

        public PluginExecutorCore(PluginKey pluginKey)
        {
            _metadata = PackageMetadataCache.GetPlugin(pluginKey);
            _statusFixture = new StatusFixture(this);
            _calcFixture = new CalculatorFixture(this);
            _marketFixture = new MarketStateFixture(this);
            _dispenser = new SubscriptionFixtureManager(this, _marketFixture);
            _timerFixture = new TimerFixture(this);
            //if (builderFactory == null)
            //    throw new ArgumentNullException("builderFactory");

            //this.builderFactory = builderFactory;
        }

        #region Properties

        public bool IsRunning { get; private set; }

        public IAccountInfoProvider AccInfoProvider
        {
            get => _externalAccData;
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    _externalAccData = value;
                }
            }
        }

        public ITradeHistoryProvider TradeHistoryProvider
        {
            get { return tradeHistoryProvider; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    tradeHistoryProvider = value ?? throw new InvalidOperationException("TradeHistoryProvider cannot be null!");
                }
            }
        }

        public InvokeStartegy InvokeStrategy
        {
            get { return iStrategy; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    iStrategy = value ?? throw new InvalidOperationException("InvokeStrategy cannot be null!");
                }
            }
        }

        public IFeedProvider Feed
        {
            get => _feedProvider;
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    _feedProvider = value ?? throw new InvalidOperationException("Feed cannot be null!");
                }
            }
        }

        public IFeedHistoryProvider FeedHistory
        {
            get => _feedHistorySrc;
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    _feedHistorySrc = value ?? throw new InvalidOperationException("FeedHistory cannot be null!");
                }
            }
        }

        public string MainSymbolCode
        {
            get { return _mainSymbol; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (string.IsNullOrEmpty(value))
                        throw new InvalidOperationException("MainSymbolCode cannot be null or empty string!");

                    _mainSymbol = value;
                }
            }
        }

        public Feed.Types.Timeframe TimeFrame
        {
            get { return _timeframe; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    _timeframe = value;
                }
            }
        }

        public Feed.Types.Timeframe ModelTimeFrame
        {
            get { return _modelTimeframe; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    _modelTimeframe = value;
                }
            }
        }

        public ITradeExecutor TradeExecutor
        {
            get => _externalTradeApi;
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    _externalTradeApi = value;
                }
            }
        }

        public IPluginMetadata Metadata
        {
            get { return metadata; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (value == null)
                        throw new InvalidOperationException("Metadata cannot be null!");

                    metadata = value;
                }
            }
        }

        public string WorkingFolder
        {
            get { return workingFolder; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (string.IsNullOrEmpty(value))
                        throw new InvalidOperationException("Working folder cannot be null or empty string!");

                    workingFolder = value;
                }
            }
        }

        public string BotWorkingFolder
        {
            get { return botWorkingFolder; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (string.IsNullOrEmpty(value))
                        throw new InvalidOperationException("Bot working folder cannot be null or empty string!");

                    botWorkingFolder = value;
                }
            }
        }

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    _instanceId = value;
                }
            }
        }

        public PluginPermissions Permissions
        {
            get { return _permissions; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    _permissions = value;
                }
            }
        }

        public event Action<PluginExecutorCore> IsRunningChanged = delegate { };
        public event Action<ExecutorInternalError> OnInternalError = delegate { };

        #endregion

        public void Start()
        {
            lock (_sync)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("EXECUTOR START!");

                    Validate();

                    if (_enableUpdateMarshaling)
                        StartUpdateMarshalling();

                    accFixture = _tradeFixtureFactory(this);

                    // Setup builder

                    _builder = _builderFactory(_metadata);
                    _builder.MainSymbol = MainSymbolCode;
                    _builder.TimeFrame = TimeFrame;
                    InitMetadata();
                    InitWorkingFolder();
                    //builder.TradeApi = accFixture;
                    _builder.Calculator = _calcFixture;
                    _builder.TradeHistoryProvider = new TradeHistoryAdapter(tradeHistoryProvider, _builder.Symbols);
                    _builder.InstanceId = _instanceId;
                    //_builder.Isolated = _permissions.Isolated;
                    _builder.Permissions = _permissions;
                    _builder.Diagnostics = this;
                    _builder.Logger = _pluginLogger;
                    _builder.OnAsyncAction = OnAsyncAction;
                    _builder.OnExit = () => OnExitRequest?.Invoke(this);
                    _builder.OnInitFailed = OnInitFailed;
                    _builder.OnInputResize = OnInputResize;
                    //builder.OnException = OnException;

                    // Setup strategy

                    accFixture.Start();

                    _marketFixture.Init();

                    iStrategy.Init(this, OnRuntimeException, _fStrategy);
                    _fStrategy.Init(this, _bStrategy, _marketFixture);
                    setupActions.ForEach(a => a());
                    BindAllOutputs();

                    iStrategy.EnqueueCustomInvoke(b =>
                    {
                        // Update state

                        ChangeState(States.Running);
                    });

                    iStrategy.EnqueueCustomInvoke(b =>
                    {
                        if (InRunningState)
                            b.InvokeInit();
                    });

                    // Start
                    _pluginLoggerFixture?.Start(); // should be after builder initializing
                    _statusFixture.Start();
                    _timerFixture.Start();
                    _fStrategy.Start(); // enqueue build action
                    _calcFixture.Start();

                    if (_metadata.Descriptor.SetupMainSymbol)
                    {
                        _dispenser.SetUserSubscription(MainSymbolCode, 1); // Default subscription
                    }

                    iStrategy.EnqueueCustomInvoke(b =>
                    {
                        if (InRunningState)
                            b.InvokeOnStart();
                    });

                    iStrategy.Start(); // Must be last action! It starts queue processing.
                }
                catch (AlgoMetadataException ex)
                {
                    _pluginLogger?.OnError(ex);
                    throw new Exception(ex.Message); // save formatted message
                }
                catch (AlgoException ex)
                {
                    _pluginLogger?.OnError(ex);
                    throw new Exception(ex.Message);
                }
                catch (Exception ex)
                {
                    _pluginLogger?.OnError(ex.InnerException ?? ex);
                    throw new Exception($"Exception of type {ex.GetType().Name} has been thrown: {ex}");
                }
            }
        }

        public Task Stop()
        {
            System.Diagnostics.Debug.WriteLine("EXECUTOR STOP!");

            return StopInternal(false);
        }

        public Task Exit()
        {
            System.Diagnostics.Debug.WriteLine("EXECUTOR EXIT!");

            return StopInternal(false);
        }

        private Task StopInternal(bool quick)
        {
            lock (_sync)
            {
                if (state == States.Idle)
                    return Task.CompletedTask;
                else if (state != States.Stopping)
                {
                    ChangeState(States.Stopping);
                    stopTask = DoStop(quick);
                }

                return stopTask;
            }
        }

        public void Abort()
        {
            lock (_sync)
            {
                System.Diagnostics.Debug.WriteLine("EXECUTOR ABORT!");

                if (state == States.Stopping)
                {
                    iStrategy.Abort();
                }
            }
        }

        public void HandleDisconnect()
        {
            lock (_sync)
            {
                if (state == States.Running)
                {
                    (accFixture as TradingFixture)?.ConnectionLost();

                    iStrategy.EnqueueCustomInvoke(b =>
                    {
                        _builder.FireDisconnectedEvent();
                    });
                }
            }
        }

        public void HandleReconnect()
        {
            lock (_sync)
            {
                if (state == States.Running)
                {
                    iStrategy.Reinit();
                    iStrategy.EnqueueCustomInvoke(b =>
                    {
                        _calcFixture.PreRestart();
                        accFixture.PreRestart();

                        _calcFixture.Stop();
                        accFixture.Stop();

                        MergeMetadata();
                        _marketFixture.Init();

                        accFixture.Start();
                        _calcFixture.Start();

                        _calcFixture.PostRestart();
                        accFixture.PostRestart();

                        _builder.Account.FireResetEvent();
                        _builder.FireConnectedEvent();
                    });
                }
            }
        }

        public void WriteConnectionInfo(string connectionInfo)
        {
            lock (_sync)
            {
                if (state == States.Running)
                    iStrategy.EnqueueCustomInvoke(b => _builder.LogConnectionInfo(connectionInfo));
            }
        }

        private async Task DoStop(bool quick)
        {
            try
            {
                _builder.SetStopped();

                await iStrategy.Stop(quick).ConfigureAwait(false);
                if (_pluginLoggerFixture != null)
                    await _pluginLoggerFixture.Stop();

                System.Diagnostics.Debug.WriteLine("EXECUTOR STOPPED STRATEGY!");

                _fStrategy.Stop();
                accFixture.Stop();
                _calcFixture.Stop();
                _statusFixture.Stop();
                _timerFixture.Stop();

                //builder.PluginProxy.Coordinator.Clear();
                //_builder.PluginProxy.Dispose();
                accFixture.Dispose();
                //_fStrategy.Dispose();
            }
            catch (Exception ex)
            {
                OnInternalException(ex, false);
            }

            try
            {
                if (_enableUpdateMarshaling)
                    StopUpdateMarshalling();
            }
            catch { }


            lock (_sync) ChangeState(States.Idle);
        }

        private void OnInitFailed(Exception ex)
        {
            OnInternalException(ex, true);
        }

        #region Setup Methods

        public T GetFeedStrategy<T>()
            where T : FeedStrategy
        {
            return (T)_fStrategy;
        }

        public BarStrategy InitBarStrategy(Feed.Types.MarketSide marketSide)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                ThrowIfAlreadyHasFStrategy();
                var strategy = new BarStrategy(marketSide);
                this._fStrategy = strategy;
                return strategy;
            }
        }

        public QuoteStrategy InitQuoteStrategy()
        {
            lock (_sync)
            {
                ThrowIfRunning();
                ThrowIfAlreadyHasFStrategy();
                var strategy = new QuoteStrategy();
                this._fStrategy = strategy;
                return strategy;
            }
        }

        public void InitSlidingBuffering(int size)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                //ThrowIfAlreadyHasBufferStrategy();
                this._bStrategy = new SlidingBufferStrategy(size);
            }
        }

        public void InitTimeSpanBuffering(DateTime from, DateTime to)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                //ThrowIfAlreadyHasBufferStrategy();
                this._bStrategy = new TimeSpanStrategy(from, to);
            }
        }

        public void SetParameter(string name, object value)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                setupActions.Add(() => _builder.SetParameter(name, value));
            }
        }

        void IPluginSetupTarget.SetupOutput<T>(string id, bool enabled)
        {
            outputFixtures[id] = new OutputFixtureFactory<T>(id).Create(this, enabled);
        }

        public OutputFixture<T> GetOutput<T>(string id)
        {
            IOutputFixture fixture;
            if (!outputFixtures.TryGetValue(id, out fixture))
            {
                fixture = new OutputFixture<T>();
                outputFixtures.Add(id, fixture);
            }
            return (OutputFixture<T>)fixture;
        }

        public LogFixture InitLogging(string logDir)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                if (_pluginLoggerFixture == null)
                    _pluginLoggerFixture = new LogFixture(this, GetDescriptor().IsTradeBot, logDir);
                _pluginLogger = _pluginLoggerFixture;
                return _pluginLoggerFixture;
            }
        }

        public void ApplyConfig(ExecutorConfig config, PluginConfig pluginConfig, IAccountProxy account)
        {
            WorkingFolder = config.WorkingDirectory;
            BotWorkingFolder = config.WorkingDirectory;

            if (config.IsLoggingEnabled)
                InitLogging(config.LogDirectory);

            InstanceId = pluginConfig.InstanceId;
            MainSymbolCode = pluginConfig.MainSymbol.Name;
            TimeFrame = pluginConfig.Timeframe;
            ModelTimeFrame = pluginConfig.ModelTimeframe;

            Permissions = pluginConfig.Permissions;

            if (config.FeedStrategyConfig.TryUnpack<BarStratefyConfig>(out var barStrategyConfig))
                InitBarStrategy(barStrategyConfig.MarketSide);
            else if (config.FeedStrategyConfig.TryUnpack<QuoteStrategyConfig>(out var quoteStrategyConfig))
                InitQuoteStrategy();

            if (config.BufferStrategyConfig.TryUnpack<SlidingBufferStrategyConfig>(out var slidingbufferConfig))
                InitSlidingBuffering(slidingbufferConfig.Size);
            else if (config.BufferStrategyConfig.TryUnpack<TimeSpanStrategyConfig>(out var timeSpanConfig))
                InitTimeSpanBuffering(timeSpanConfig.From.ToDateTime(), timeSpanConfig.To.ToDateTime());

            if (config.InvokeStrategyConfig.TryUnpack<PriorityInvokeStrategyConfig>(out var priorityStrategyConfig))
                InvokeStrategy = new PriorityInvokeStartegy();

            AccInfoProvider = account.AccInfoProvider;
            Metadata = account.Metadata;
            TradeHistoryProvider = account.TradeHistoryProvider;
            Feed = account.Feed;
            FeedHistory = account.FeedHistory;
            TradeExecutor = account.TradeExecutor;

            PluginConfigLoader.ApplyConfig(this, pluginConfig, MainSymbolCode, workingFolder);
        }

        #region Emulator Support

        internal void InitEmulation(InvokeStartegy iStrategy, FeedStrategy fStrategy, IFeedProvider feedProvider, IFeedHistoryProvider feedHistoryProvider,
            Func<IFixtureContext, IExecutorFixture> tradeFixtureFactory, IPluginLogger pluginLogger, IExecutorFixture timerFixture,
            Func<PluginMetadata, PluginBuilder> builderFactory)
        {
            InvokeStrategy = iStrategy;
            if (fStrategy != null)
            {
                _fStrategy = fStrategy;
                _feedProvider = feedProvider;
                _feedHistorySrc = feedHistoryProvider;
            }
            _tradeFixtureFactory = tradeFixtureFactory;
            _timerFixture = timerFixture;
            _builderFactory = builderFactory;
            _pluginLogger = pluginLogger;
            _enableUpdateMarshaling = false;
        }

        internal PluginBuilder GetBuilder() => _builder;
        internal PluginDescriptor GetDescriptor() => _metadata.Descriptor;
        internal IExecutorFixture GetTradeFixute() => accFixture;
        internal CalculatorFixture GetCalcFixture() => _calcFixture;

        internal void EmulateStop()
        {
            if (state == States.Running)
            {
                ChangeState(States.Stopping);
                stopTask = DoStop(false);
            }
        }

        internal void WaitStop()
        {
            stopTask?.Wait();
        }

        #endregion

        #endregion

        private void Validate()
        {
            if (state == States.Running)
                throw new InvalidOperationException("Executor has been already started!");
            else if (state == States.Stopping)
                throw new InvalidOperationException("Executor has not been stopped yet!");

            //if (feed == null)
            //    throw new InvalidOperationException("Feed provider is not specified!");

            if (_fStrategy == null)
                throw new ExecutorException("Feed strategy is not set!");

            if (_bStrategy == null)
                throw new ExecutorException("Buffering strategy is not set!");

            if (iStrategy == null)
                throw new ExecutorException("Invoke strategy is not set!");

            if (string.IsNullOrEmpty(_mainSymbol))
                throw new ExecutorException("Main symbol is not specified!");
        }

        private void ThrowIfRunning()
        {
            if (state != States.Idle)
                throw new InvalidOperationException("Executor parameters cannot be changed after start!");
        }

        private void ThrowIfAlreadyHasFStrategy()
        {
            if (_fStrategy != null)
                throw new InvalidOperationException("Feed has beed already initialized!");
        }

        private void ChangeState(States newState)
        {
            state = newState;

            if (newState == States.Idle)
            {
                IsRunning = false;
                IsRunningChanged(this);
            }
            else if (newState == States.Running)
            {
                IsRunning = true;
                IsRunningChanged(this);
            }
        }

        private void OnInternalException(Exception ex, bool isFatal)
        {
            OnInternalError?.Invoke(new ExecutorInternalError(ex, isFatal));
        }

        private void OnRuntimeException(Exception ex)
        {
            OnInternalException(ex, false);
        }

        private void OnInputResize(int newSize)
        {
            string error;
            _fStrategy.BufferingStrategy.OnUserSetBufferSize(newSize, out error);
            if (error != null)
                _pluginLogger.OnError(error);
        }

        private void OnAsyncAction(Action asyncAction)
        {
            lock (_sync)
            {
                if (state != States.Idle)
                    iStrategy.EnqueueCustomInvoke(b => b.InvokeAsyncAction(asyncAction));
            }
        }

        private void BindAllOutputs()
        {
            foreach (var fixtureEntry in outputFixtures)
            {
                var outputBuffer = _builder.GetOutput(fixtureEntry.Key);
                if (outputBuffer != null)
                    fixtureEntry.Value.BindTo(outputBuffer, _fStrategy.MainBuffer);
            }
        }

        private void InitMetadata()
        {
            if (metadata != null)
            {
                var symbolInfoList = metadata.GetSymbolMetadata();
                var currencies = metadata.GetCurrencyMetadata();

                _builder.Currencies.Init(currencies);
                _builder.Symbols.Init(symbolInfoList);
            }
        }

        private void MergeMetadata()
        {
            if (metadata != null)
            {
                var symbolInfoList = metadata.GetSymbolMetadata();
                var currencies = metadata.GetCurrencyMetadata();

                _builder.Currencies.Merge(currencies);
                _builder.Symbols.Merge(symbolInfoList);
            }
        }

        private void InitWorkingFolder()
        {
            _builder.DataFolder = !string.IsNullOrEmpty(workingFolder) ? workingFolder : System.IO.Directory.GetCurrentDirectory();
            _builder.BotDataFolder = !string.IsNullOrEmpty(botWorkingFolder) ? botWorkingFolder : System.IO.Directory.GetCurrentDirectory();
        }

        #region Update Marshaling

        private BunchingBlock<object> _channel;

        public bool IsGlobalMarshalingEnabled { get; set; }
        public bool IsBunchingRequired { get; set; }
        internal Action<IReadOnlyList<object>> MarshalUpdates { get; set; }
        internal Action<object> MarshalUpdate { get; set; }
        internal Action<object> OnUpdate { get; set; }
        public Action<IMessage> OnNotification { get; set; }
        public Action<PluginExecutorCore> OnExitRequest { get; set; }

        internal void StartUpdateMarshalling()
        {
            if (IsBunchingRequired)
            {
                _channel = new BunchingBlock<object>(MarshalUpdates, 30, 60);
                OnUpdate = _channel.EnqueueNoTrhow;
            }
            else
                OnUpdate = MarshalUpdate;
        }

        internal void StopUpdateMarshalling()
        {
            if (_channel != null)
            {
                _channel.Complete();
                _channel.Completion.Wait();
            }
        }

        #endregion

        #region IFixtureContext

        IFeedProvider IFixtureContext.FeedProvider => _feedProvider;
        SubscriptionFixtureManager IFixtureContext.Dispenser => _dispenser;
        FeedBufferStrategy IFixtureContext.BufferingStrategy => _fStrategy.BufferingStrategy;
        string IFixtureContext.MainSymbolCode => _mainSymbol;
        AlgoMarketState IFixtureContext.MarketData => _marketFixture.Market;
        Feed.Types.Timeframe IFixtureContext.TimeFrame => _timeframe;
        Feed.Types.Timeframe IFixtureContext.ModelTimeFrame => _modelTimeframe;
        PluginBuilder IFixtureContext.Builder => _builder;
        FeedStrategy IFixtureContext.FeedStrategy => _fStrategy;
        PluginLoggerAdapter IFixtureContext.Logger => _builder.LogAdapter;
        bool IFixtureContext.IsGlobalUpdateMarshalingEnabled => IsGlobalMarshalingEnabled;

        void IFixtureContext.EnqueueTradeUpdate(Action<PluginBuilder> action)
        {
            iStrategy.EnqueueTradeUpdate(action);
        }

        void IFixtureContext.EnqueueCustomInvoke(Action<PluginBuilder> action)
        {
            iStrategy.EnqueueCustomInvoke(action);
        }

        void IFixtureContext.EnqueueQuote(QuoteInfo update)
        {
            iStrategy.EnqueueQuote(update);
        }

        public void EnqueueEvent(Action<PluginBuilder> action)
        {
            iStrategy.EnqueueEvent(action);
        }

        public void EnqueueCustomInvoke(Action<PluginBuilder> action)
        {
            iStrategy.EnqueueCustomInvoke(action);
        }

        public void ProcessNextOrderUpdate()
        {
            iStrategy.ProcessNextTrade();
        }

        void IFixtureContext.OnInternalException(Exception ex)
        {
            OnInternalException(ex, false);
        }

        void IFixtureContext.SendExtUpdate(object update) => OnUpdate(update);

        void IFixtureContext.SendNotification(IMessage message) => OnNotification(message);

        #endregion

        #region DiagnosticInfo

        int DiagnosticInfo.FeedQueueSize { get { return iStrategy.FeedQueueSize; } }

        #endregion
    }
}
