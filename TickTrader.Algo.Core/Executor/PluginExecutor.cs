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
    public class PluginExecutor : CrossDomainObject, IFixtureContext, IPluginSetupTarget
    {
        public enum States { Idle, Running }

        private object _sync = new object();
        private IPluginLogger logger;
        private IPluginMetadata metadata;
        private FeedStrategy fStrategy;
        private InvokeStartegy iStrategy;
        private AccDataFixture accFixture;
        private StatusFixture statusFixture;
        private string mainSymbol;
        private PluginBuilder builder;
        private DateTime periodStart;
        private DateTime periodEnd;
        private Api.TimeFrames timeframe;
        private ITradeApi tradeApi;
        private List<Action> setupActions = new List<Action>();
        private AlgoPluginDescriptor descriptor;
        private Dictionary<string, OutputFixture> outputFixtures = new Dictionary<string, OutputFixture>();
        private Task stopTask;
        private string workingFolder;
        private bool isExited;

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

        public IPluginLogger Logger
        {
            get { return logger; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (value == null)
                        throw new InvalidOperationException("Logger cannot be null!");

                    logger = value;
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

        public DateTime TimePeriodStart
        {
            get { return periodStart; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    periodStart = value;
                }
            }
        }

        public DateTime TimePeriodEnd
        {
            get { return periodEnd; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();
                    periodEnd = value;
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

        public event Action<PluginExecutor> IsRunningChanged = delegate { };

        #endregion

        public void Start()
        {
            lock (_sync)
            {
                if (IsRunning)
                    throw new InvalidOperationException("Executor has been already started!");

                Validate();

                //isInitialized = true;
                isExited = false;

                stopTask = null;

                // Setup builder
                builder = new PluginBuilder(descriptor);
                builder.MainSymbol = MainSymbolCode;
                InitMetadata();
                InitWorkingFolder();
                builder.TradeApi = tradeApi;
                if (logger != null)
                    builder.Logger = logger;
                builder.OnAsyncAction = OnAsyncAction;
                builder.OnSubscribe = fStrategy.OnUserSubscribe;
                builder.OnUnsubscribe = fStrategy.OnUserUnsubscribe;
                //builder.OnException = OnException;
                builder.OnExit = OnExit;
                statusFixture.Start();
                fStrategy.Init(this);
                fStrategy.OnUserSubscribe(MainSymbolCode, 1);   // Default subscribe
                setupActions.ForEach(a => a());
                BindAllOutputs();
                iStrategy.Init(builder, OnInternalException, OnRuntimeException, OnFeedUpdate);

                // Start
                accFixture.Start();
                iStrategy.Start();
                iStrategy.Enqueue(b => b.InvokeInit());
                fStrategy.Start();
                iStrategy.Enqueue(b => b.InvokeOnStart());

                IsRunning = true;
                IsRunningChanged(this);
            }
        }

        public void Stop()
        {
            Task taskToWait = null;

            lock (_sync)
            {
                if (!IsRunning)
                    return;

                if (stopTask == null)
                    stopTask = iStrategy.Stop(b => DoStop(true));

                taskToWait = stopTask;
            }

            taskToWait.Wait();
        }

        private void DoStop(bool invokeOnStop)
        {
            if (invokeOnStop)
                builder.InvokeOnStop();

            fStrategy.Stop();
            accFixture.Stop();
            statusFixture.Stop();

            lock (_sync)
            {
                IsRunning = false;
                stopTask = null;
                IsRunningChanged(this);
            }
        }

        private void OnExit()
        {
            lock (_sync)
            {
                if (isExited)
                    return;

                isExited = true;
                if (stopTask == null)
                    stopTask = iStrategy.Stop(b => DoStop(false));
            }
        }

        #region Setup Methods

        public void InitBarStartegy(IBarBasedFeed feed)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                fStrategy = new BarStrategy(feed);
            }
        }

        public void InitQuoteStartegy(IQuoteBasedFeed feed)
        {
            lock (_sync)
            {
                ThrowIfRunning();
                fStrategy = new QuoteStrategy(feed);
            }
        }

        public void MapBarInput(string id, string symbolCode, Func<BarEntity, double> selector)
        {
            MapBarInput<double>(id, symbolCode, selector);
        }

        public void MapBarInput<TVal>(string inputName, string symbolCode, Func<BarEntity, TVal> selector)
        {
            setupActions.Add(() => fStrategy.MapInput(inputName, symbolCode, selector));
        }

        public void MapBarInput(string inputName, string symbolCode)
        {
            setupActions.Add(() => fStrategy.MapInput<BarEntity, Api.Bar>(inputName, symbolCode, b => b));
        }

        public void MapInput<TEntity, TData>(string inputName, string symbolCode, Func<TEntity, TData> selector)
        {
            setupActions.Add(() => fStrategy.MapInput<TEntity, TData>(inputName, symbolCode, selector));
        }

        public void SetParameter(string name, object value)
        {
            setupActions.Add(() => builder.SetParameter(name, value));
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

        #endregion

        private void Validate()
        {
            if (IsRunning)
                throw new ExecutorException("Executor has been already started!");

            //if (feed == null)
            //    throw new InvalidOperationException("Feed provider is not specified!");

            if (fStrategy == null)
                throw new ExecutorException("Feed strategy is not specified!");

            if (iStrategy == null)
                throw new ExecutorException("Invoke strategy is not specified!");

            if (string.IsNullOrEmpty(mainSymbol))
                throw new ExecutorException("Main symbol is not specified!");
        }

        private void ThrowIfRunning()
        {
            if (IsRunning)
                throw new InvalidOperationException("Executor parameters cannot be changed after start!");
        }

        //private void Feed_FeedUpdated(FeedUpdate[] updates)
        //{
        //    //iStrategy.OnUpdate(updates).Wait();
        //}

        private void OnException(Exception pluginError)
        {
            OnExit();
        }

        private void OnInternalException(ExecutorException ex)
        {
            logger.OnError(ex);
        }

        private void OnRuntimeException(Exception ex)
        {
            logger.OnError(ex);
        }

        private void OnAsyncAction(SendOrPostCallback callback, object state)
        {
            iStrategy.EnqueueCustomAction(b => callback(state));
        }

        private void OnFeedUpdate(FeedUpdate update)
        {
            fStrategy.ApplyUpdate(update);
        }

        private void BindAllOutputs()
        {
            foreach (var fixtureEntry in outputFixtures)
            {
                var outputBuffer = builder.GetOutput(fixtureEntry.Key);
                if (outputBuffer != null)
                    fixtureEntry.Value.BindTo(outputBuffer, fStrategy.TimeRef);
            }
        }

        private void InitMetadata()
        {
            if (metadata != null)
            {
                var symbolInfoList = metadata.GetSymbolMetadata();
                foreach (var smb in symbolInfoList)
                    builder.Symbols.Add(smb);
            }
        }

        private void InitWorkingFolder()
        {
            if (!string.IsNullOrEmpty(workingFolder))
                builder.DataFolder = workingFolder;
            else
                builder.DataFolder = System.IO.Directory.GetCurrentDirectory();
        }

        #region IFeedStrategyContext

        PluginBuilder IFixtureContext.Builder { get { return builder; } }

        void IFixtureContext.Enqueue(Action<PluginBuilder> action)
        {
            iStrategy.Enqueue(action);
        }

        void IFixtureContext.Enqueue(FeedUpdate update)
        {
            iStrategy.Enqueue(update);
        }

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
