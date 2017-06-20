using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        private FeedStrategy fStrategy;
        private FeedBufferStrategy bStrategy;
        private InvokeStartegy iStrategy;
        private AccDataFixture accFixture;
        private StatusFixture statusFixture;
        private string mainSymbol;
        private PluginBuilder builder;
        private Api.TimeFrames timeframe;
        private ITradeApi tradeApi;
        private List<Action> setupActions = new List<Action>();
        private AlgoPluginDescriptor descriptor;
        private Dictionary<string, OutputFixture> outputFixtures = new Dictionary<string, OutputFixture>();
        private Task stopTask;
        private string workingFolder;
        private string botWorkingFolder;
        private States state;

        public PluginExecutor(string pluginId)
        {
            this.descriptor = AlgoPluginDescriptor.Get(pluginId);
            this.accFixture = new AccDataFixture(this);
            this.statusFixture = new StatusFixture(this);
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

        public Api.TimeFrames TimeFrame
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

        public ITradeApi TradeApi
        {
            get { return tradeApi; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    tradeApi = value;
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
                builder.TradeApi = tradeApi;
                builder.Diagnostics = this;
                builder.Logger = pluginLogger ?? Null.Logger;
                builder.OnAsyncAction = OnAsyncAction;
                builder.OnSubscribe = fStrategy.OnUserSubscribe;
                builder.OnUnsubscribe = fStrategy.OnUserUnsubscribe;
                builder.OnExit = OnExit;
                //builder.OnException = OnException;

                // Setup strategy

                iStrategy.Init(builder, OnInternalException, OnRuntimeException, OnFeedUpdate);
                fStrategy.Init(this, bStrategy);
                fStrategy.OnUserSubscribe(MainSymbolCode, 1);   // Default subscribe
                setupActions.ForEach(a => a());
                BindAllOutputs();
                iStrategy.Enqueue(b => b.InvokeInit()); // enqueue init

                // Start

                pluginLogger?.Start();
                statusFixture.Start();
                accFixture.Start();
                fStrategy.Start(); // enqueue build action

                iStrategy.Enqueue(b => b.InvokeOnStart());

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
                var strategy = new BarStrategy(feed, mainPirceTipe);
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
                var strategy = new QuoteStrategy(feed);
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
                    iStrategy.EnqueueCustomAction(b => b.InvokeAsyncAction(asyncAction));
            }
        }

        private void OnFeedUpdate(RateUpdate update)
        {
            fStrategy.ApplyUpdate(update);
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

        #region IFeedStrategyContext

        PluginBuilder IFixtureContext.Builder => builder;
        IPluginLogger IFixtureContext.Logger => pluginLogger;

        void IFixtureContext.Enqueue(Action<PluginBuilder> action)
        {
            iStrategy.Enqueue(action);
        }

        void IFixtureContext.Enqueue(QuoteEntity update)
        {
            iStrategy.Enqueue(update);
        }

        void IFixtureContext.OnInternalException(Exception ex)
        {
            OnInternalException(ex);
        }

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
