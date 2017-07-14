using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class PluginExecutor : CrossDomainObject, IFixtureContext, IPluginSetupTarget, DiagnosticInfo
    {
        public enum States { Idle, Running, Stopping }

        private object _sync = new object();
        private LogFixture pluginLogger;
        private IPluginMetadata metadata;
        private IPluginFeedProvider feedProvider;
        private ITradeHistoryProvider tradeHistoryProvider;
        private FeedStrategy fStrategy;
        private FeedBufferStrategy bStrategy;
        private SubscriptionManager dispenser;
        private InvokeStartegy iStrategy;
        private CalculatorFixture calcFixture;
        private TradingFixture accFixture;
        private StatusFixture statusFixture;
        private string mainSymbol;
        private PluginBuilder builder;
        private Api.TimeFrames timeframe;
        private List<Action> setupActions = new List<Action>();
        private AlgoPluginDescriptor descriptor;
        private Dictionary<string, OutputFixture> outputFixtures = new Dictionary<string, OutputFixture>();
        private Task stopTask;
        private string workingFolder;
        private string botWorkingFolder;
        private string _botInstanceId;
        private bool _isolated;
        private States state;

        public PluginExecutor(string pluginId)
        {
            this.descriptor = AlgoPluginDescriptor.Get(pluginId);
            this.accFixture = new TradingFixture(this);
            this.statusFixture = new StatusFixture(this);
            calcFixture = new CalculatorFixture(this);
            dispenser = new SubscriptionManager(this);
            //if (builderFactory == null)
            //    throw new ArgumentNullException("builderFactory");

            //this.builderFactory = builderFactory;
        }

        #region Properties

        public bool IsRunning { get; private set; }

        public IAccountInfoProvider AccInfoProvider
        {
            get { return accFixture.DataProvider; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    accFixture.DataProvider = value;
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
                    tradeHistoryProvider = value?? throw new InvalidOperationException("TradeHistoryProvider cannot be null!");
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

                    if (value == null)
                        throw new InvalidOperationException("InvokeStrategy cannot be null!");

                    iStrategy = value;
                }
            }
        }

        public string MainSymbolCode
        {
            get { return mainSymbol; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (string.IsNullOrEmpty(value))
                        throw new InvalidOperationException("MainSymbolCode cannot be null or empty string!");

                    mainSymbol = value;
                }
            }
        }

        public TimeFrames TimeFrame
        {
            get { return timeframe; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    timeframe = value;
                }
            }
        }

        public ITradeExecutor TradeExecutor
        {
            get { return accFixture.Executor; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    accFixture.Executor = value;
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
            get { return _botInstanceId; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    _botInstanceId = value;
                }
            }
        }

        public bool Isolated 
        {
            get { return _isolated; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    _isolated = value;
                }
            }
        }

        public event Action<PluginExecutor> IsRunningChanged = delegate { };
        public event Action<Exception> OnRuntimeError = delegate { };

        #endregion

        public void Start()
        {
            lock (_sync)
            {
                System.Diagnostics.Debug.WriteLine("EXECUTOR START!");

                Validate();

                // Setup builder

                builder = new PluginBuilder(descriptor);
                builder.MainSymbol = MainSymbolCode;
                InitMetadata();
                InitWorkingFolder();
                builder.TradeApi = accFixture;
                builder.TradeHistoryProvider = tradeHistoryProvider;
                builder.Id = _botInstanceId;
                builder.Isolated = _isolated;
                builder.Diagnostics = this;
                builder.Logger = pluginLogger ?? Null.Logger;
                builder.OnAsyncAction = OnAsyncAction;
                builder.OnExit = OnExit;
                //builder.OnException = OnException;

                // Setup strategy

                iStrategy.Init(builder, OnInternalException, OnRuntimeException, fStrategy);
                fStrategy.Init(this, bStrategy, ApplyNewRate);
                fStrategy.SetSubscribed(MainSymbolCode, 1);   // Default subscribe
                setupActions.ForEach(a => a());
                BindAllOutputs();
                iStrategy.EnqueueTradeUpdate(b => b.InvokeInit()); // enqueue init

                // Start

                pluginLogger?.Start();
                statusFixture.Start();
                accFixture.Start();
                calcFixture.Start();
                fStrategy.Start(); // enqueue build action

                iStrategy.EnqueueTradeUpdate(b => b.InvokeOnStart());

                iStrategy.Start(); // Must be last action! It starts queue processing.

                // Update state

                ChangeState(States.Running);
            }
        }

        public void Stop()
        {
            Task taskToWait = null;

            lock (_sync)
            {
                System.Diagnostics.Debug.WriteLine("EXECUTOR STOP!");

                if (state == States.Idle)
                    return;
                else if (state != States.Stopping)
                {
                    ChangeState(States.Stopping);
                    stopTask = DoStop(false);
                }   

                taskToWait = stopTask;
            }

            taskToWait.Wait();
        }

        public void HandleReconnect()
        {
            lock (_sync)
            {
                if (state == States.Running)
                {
                    iStrategy.EnqueueTradeUpdate(b =>
                    {
                        calcFixture.Stop();
                        accFixture.Restart();
                        calcFixture.Start();
                        builder.Account.FireResetEvent();
                    });
                }
            }
        }

        private async Task DoStop(bool qucik)
        {
            try
            {
                builder.SetStopped();

                await iStrategy.Stop(qucik).ConfigureAwait(false);
                if (pluginLogger != null)
                    await pluginLogger.Stop();

                System.Diagnostics.Debug.WriteLine("EXECUTOR STOPPED STRATEGY!");

                fStrategy.Stop();
                calcFixture.Stop();
                accFixture.Stop();
                statusFixture.Stop();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }

            lock (_sync) ChangeState(States.Idle);
        }

        private void OnExit()
        {
            lock (_sync)
            {
                System.Diagnostics.Debug.WriteLine("EXECUTOR EXIT!");

                if (state != States.Running)
                    return;

                ChangeState(States.Stopping);
                stopTask = DoStop(true);
            }
        }

        #region Setup Methods

        public T GetFeedStrategy<T>()
            where T: FeedStrategy
        {
            return (T)fStrategy;
        }

        public BarStrategy InitBarStrategy(IPluginFeedProvider feed, BarPriceType mainPirceTipe, List<BarEntity> mainSeries = null)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                ThrowIfAlreadyHasFStrategy();
                this.feedProvider = feed;
                var strategy = new BarStrategy(mainPirceTipe);
                this.fStrategy = strategy;
                return strategy;
            }
        }

        public QuoteStrategy InitQuoteStrategy(IPluginFeedProvider feed)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                ThrowIfAlreadyHasFStrategy();
                this.feedProvider = feed;
                var strategy = new QuoteStrategy();
                this.fStrategy = strategy;
                return strategy;
            }
        }

        public void InitSlidingBuffering(int size)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                //ThrowIfAlreadyHasBufferStrategy();
                this.bStrategy = new SlidingBufferStrategy(size);
            }
        }

        public void InitTimeSpanBuffering(DateTime from, DateTime to)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                //ThrowIfAlreadyHasBufferStrategy();
                this.bStrategy = new TimeSpanStrategy(from, to);
            }
        }

        public void SetParameter(string name, object value)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                setupActions.Add(() => builder.SetParameter(name, value));
            }
        }

        public OutputFixture<T> GetOutput<T>(string id)
        {
            OutputFixture fixture;
            if (!outputFixtures.TryGetValue(id, out fixture))
            {
                fixture = new OutputFixture<T>();
                outputFixtures.Add(id, fixture);
            }
            return (OutputFixture<T>)fixture;
        }

        public LogFixture InitLogging()
        {
            lock (_sync)
            {
                ThrowIfRunning();
                if (pluginLogger == null)
                    pluginLogger = new LogFixture(this);
                return pluginLogger;
            }
        }

        #endregion

        private void ApplyNewRate(QuoteEntity quote)
        {
            calcFixture.UpdateRate(quote);
        }

        private void Validate()
        {
            if (state == States.Running)
                throw new InvalidOperationException("Executor has been already started!");
            else if (state == States.Stopping)
                throw new InvalidOperationException("Executor has not been stopped yet!");

            //if (feed == null)
            //    throw new InvalidOperationException("Feed provider is not specified!");

            if (fStrategy == null)
                throw new ExecutorException("Feed strategy is not set!");

            if (bStrategy == null)
                throw new ExecutorException("Buffering strategy is not set!");

            if (iStrategy == null)
                throw new ExecutorException("Invoke strategy is not set!");

            if (string.IsNullOrEmpty(mainSymbol))
                throw new ExecutorException("Main symbol is not specified!");
        }

        private void ThrowIfRunning()
        {
            if (state != States.Idle)
                throw new InvalidOperationException("Executor parameters cannot be changed after start!");
        }

        private void ThrowIfAlreadyHasFStrategy()
        {
            if (fStrategy != null)
                throw new InvalidOperationException("Feed has beed already initialized!");
        }

        //private void ThrowIfAlreadyHasBufferStrategy()
        //{
        //    if (bStrategy != null)
        //        throw new InvalidOperationException("Buffering strategy has beed already initialized!");
        //}

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

        //private void Feed_FeedUpdated(FeedUpdate[] updates)
        //{
        //    //iStrategy.OnUpdate(updates).Wait();
        //}

        private void OnException(Exception pluginError)
        {
            OnExit();
        }

        private void OnInternalException(Exception ex)
        {
            //OnRuntimeError?.Invoke(ex);
        }

        private void OnRuntimeException(Exception ex)
        {
            OnRuntimeError?.Invoke(ex);
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
                var outputBuffer = builder.GetOutput(fixtureEntry.Key);
                if (outputBuffer != null)
                    fixtureEntry.Value.BindTo(outputBuffer, fStrategy.MainBuffer);
            }
        }

        private void InitMetadata()
        {
            if (metadata != null)
            {
                var symbolInfoList = metadata.GetSymbolMetadata();
                var currencies = metadata.GetCurrencyMetadata().ToDictionary(c => c.Name);
                foreach (var smb in symbolInfoList)
                    builder.Symbols.Add(smb, currencies);
                foreach (var currency in currencies.Values)
                    builder.Currencies.Add(currency);
            }
        }

        private void InitWorkingFolder()
        {
            builder.DataFolder = !string.IsNullOrEmpty(workingFolder) ? workingFolder : System.IO.Directory.GetCurrentDirectory();
            builder.BotDataFolder = !string.IsNullOrEmpty(botWorkingFolder) ? botWorkingFolder : System.IO.Directory.GetCurrentDirectory();
        }

        #region IFixtureContext

        IPluginFeedProvider IFixtureContext.FeedProvider => feedProvider;
        SubscriptionManager IFixtureContext.Dispenser => dispenser;
        FeedBufferStrategy IFixtureContext.BufferingStrategy => fStrategy.BufferingStrategy;
        string IFixtureContext.MainSymbolCode => mainSymbol;
        TimeFrames IFixtureContext.TimeFrame => timeframe;
        PluginBuilder IFixtureContext.Builder => builder;
        IPluginLogger IFixtureContext.Logger => pluginLogger;

        void IFixtureContext.EnqueueTradeUpdate(Action<PluginBuilder> action)
        {
            iStrategy.EnqueueTradeUpdate(action);
        }

        void IFixtureContext.EnqueueQuote(QuoteEntity update)
        {
            iStrategy.EnqueueQuote(update);
        }

        public void EnqueueTradeEvent(Action<PluginBuilder> action)
        {
            iStrategy.EnqueueTradeEvent(action);
        }

        public void ProcessNextOrderUpdate()
        {
            iStrategy.ProcessNextTrade();
        }

        void IFixtureContext.OnInternalException(Exception ex)
        {
            OnInternalException(ex);
        }

        //void IFixtureContext.Subscribe(IRateSubscription subscriber)
        //{
        //    fStrategy.RateDispenser.Add(subscriber);
        //}

        //void IFixtureContext.Unsubscribe(IRateSubscription subscriber)
        //{
        //    fStrategy.RateDispenser.Remove(subscriber);
        //}

        //void IFixtureContext.Subscribe(IAllRatesSubscription subscriber)
        //{
        //    fStrategy.RateDispenser.Add(subscriber);
        //}

        //void IFixtureContext.Unsubscribe(IAllRatesSubscription subscriber)
        //{
        //    fStrategy.RateDispenser.Remove(subscriber);
        //}

        //void IFixtureContext.AddSetupAction(Action setupAction)
        //{
        //    setupActions.Add(setupAction);
        //}

        //IEnumerable<BarEntity> IFeedStrategyContext.QueryBars(string symbolCode, DateTime from, DateTime to, TimeFrames timeFrame)
        //{
        //    return feed.CustomQueryBars(symbolCode, from, to, timeFrame);
        //}

        //IEnumerable<QuoteEntity> IFeedStrategyContext.QueryTicks(string symbolCode, DateTime from, DateTime to)
        //{
        //    return feed.CustomQueryTicks(symbolCode, from, to, 1);
        //}

        //IEnumerable<QuoteEntityL2> IFeedStrategyContext.QueryTicksL2(string symbolCode, DateTime from, DateTime to)
        //{
        //    throw new NotImplementedException();
        //}

        //void IFeedStrategyContext.Add(IFeedFixture subscriber)
        //{
        //    subscriptionManager.Add(subscriber);
        //}

        //void IFeedStrategyContext.Remove(IFeedFixture subscriber)
        //{
        //    subscriptionManager.Remove(subscriber);
        //}

        //void IFeedStrategyContext.InvokeUpdateOnCustomSubscription(QuoteEntity update)
        //{
        //    builder.InvokeUpdateNotification(update);
        //}

        #endregion

        #region DiagnosticInfo

        int DiagnosticInfo.FeedQueueSize { get { return iStrategy.FeedQueueSize; } }

        #endregion

        //#region IInvokeStrategyContext

        //IPluginInvoker IInvokeStrategyContext.Builder { get { return builder; } }

        //BufferUpdateResults IInvokeStrategyContext.UpdateBuffers(FeedUpdate update)
        //{
        //    return fStrategy.UpdateBuffers(update);
        //}

        //void IInvokeStrategyContext.InvokeFeedEvents(FeedUpdate update)
        //{
        //    subscriptionManager.OnUpdateEvent(update.Quote);
        //}

        //#endregion
    }
}
