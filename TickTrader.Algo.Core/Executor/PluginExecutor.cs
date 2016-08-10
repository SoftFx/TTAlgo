using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class PluginExecutor : NoTimeoutByRefObject, IFeedStrategyContext, IInvokeStrategyContext, IPluginSetupTarget
    {
        public enum States { Idle, Running }

        private object _sync = new object();
        private IPluginFeedProvider feed;
        private IPluginLogger logger;
        private SubscriptionManager subscriptionManager;
        private FeedStrategy fStrategy;
        private InvokeStartegy iStrategy;
        private string mainSymbol;
        private PluginBuilder builder;
        private DateTime periodStart;
        private DateTime periodEnd;
        private Api.TimeFrames timeframe;
        private List<Action> setupActions = new List<Action>();
        private AlgoPluginDescriptor descriptor;
        private Dictionary<string, SubscriptionFixture> userSubscriptions = new Dictionary<string, SubscriptionFixture>();
        private Dictionary<string, OutputFixture> outputFixtures = new Dictionary<string, OutputFixture>();
        private Task stopTask;

        public PluginExecutor(string pluginId)
        {
            this.descriptor = AlgoPluginDescriptor.Get(pluginId);
            //if (builderFactory == null)
            //    throw new ArgumentNullException("builderFactory");

            //this.builderFactory = builderFactory;
        }

        #region Properties

        public bool IsRunning { get; private set; }

        public IPluginFeedProvider FeedProvider
        {
            get { return feed; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (value == null)
                        throw new InvalidOperationException("FeedProvider cannot be null!");

                    feed = value;
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

        public FeedStrategy FeedStrategy
        {
            get { return fStrategy; }
            set
            {
                lock (_sync)
                {
                    ThrowIfRunning();

                    if (value == null)
                        throw new InvalidOperationException("FeedStrategy cannot be null!");

                    fStrategy = value;
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

                stopTask = null;

                feed.FeedUpdated += Feed_FeedUpdated;
                subscriptionManager = new SubscriptionManager(feed);
                builder = new PluginBuilder(descriptor);
                builder.MainSymbol = MainSymbolCode;
                builder.Symbols.Init(feed.GetSymbolMetadata());
                if (logger != null)
                    builder.Logger = logger;
                builder.OnSubscribe = OnUserSubscribe;
                //builder.OnException = OnException;
                builder.OnExit = Abort;
                fStrategy.Start(this);
                setupActions.ForEach(a => a());
                BindAllOutputs();
                iStrategy.Start(this, fStrategy.BufferSize);

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
                    stopTask = DoStop();

                taskToWait = stopTask;
            }

            taskToWait.Wait();
        }

        private async Task DoStop()
        {
            fStrategy.Stop();
            feed.FeedUpdated -= Feed_FeedUpdated;
            await iStrategy.Stop();

            lock (_sync)
            {
                IsRunning = false;
                stopTask = null;
                IsRunningChanged(this);
            }
        }

        private void Abort()
        {
            lock (_sync)
            {
                iStrategy.Abort();
                if (stopTask == null)
                    stopTask = DoStop();
            }
        }

        #region Setup Methods

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
                throw new InvalidOperationException("Executor has been already started!");

            if (feed == null)
                throw new InvalidOperationException("Feed provider is not specified!");

            if (fStrategy == null)
                throw new InvalidOperationException("Feed strategy is not specified!");

            if (iStrategy == null)
                throw new InvalidOperationException("Invoke strategy is not specified!");

            if (string.IsNullOrEmpty(mainSymbol))
                throw new InvalidOperationException("Main symbol is not specified!");
        }

        private void ThrowIfRunning()
        {
            if (IsRunning)
                throw new InvalidOperationException("Executor parameters cannot be changed after start!");
        }

        private void Feed_FeedUpdated(FeedUpdate[] updates)
        {
            iStrategy.OnUpdate(updates).Wait();
        }

        private void OnUserSubscribe(string symbolCode, int depth)
        {
            SubscriptionFixture fixture;
            if (userSubscriptions.TryGetValue(symbolCode, out fixture))
                subscriptionManager.Remove(fixture);
            fixture = new SubscriptionFixture(this, symbolCode, depth);
            userSubscriptions[symbolCode] = fixture;
            subscriptionManager.Add(fixture);
        }

        private void OnException(Exception pluginError)
        {
            Abort();
        }

        private void BindAllOutputs()
        {
            foreach (var fixtureEntry in outputFixtures)
            {
                var outputBuffer = builder.GetOutput(fixtureEntry.Key);
                if (outputBuffer != null)
                    fixtureEntry.Value.BindTo(outputBuffer, FeedStrategy.TimeRef);
            }
        }

        #region IFeedStrategyContext

        PluginBuilder IFeedStrategyContext.Builder { get { return builder; } }

        IEnumerable<BarEntity> IFeedStrategyContext.QueryBars(string symbolCode, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return feed.CustomQueryBars(symbolCode, from, to, timeFrame);
        }

        IEnumerable<QuoteEntity> IFeedStrategyContext.QueryTicks(string symbolCode, DateTime from, DateTime to)
        {
            return feed.CustomQueryTicks(symbolCode, from, to, 1);
        }

        IEnumerable<QuoteEntityL2> IFeedStrategyContext.QueryTicksL2(string symbolCode, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        void IFeedStrategyContext.Add(IFeedFixture subscriber)
        {
            subscriptionManager.Add(subscriber);
        }

        void IFeedStrategyContext.Remove(IFeedFixture subscriber)
        {
            subscriptionManager.Remove(subscriber);
        }

        void IFeedStrategyContext.InvokeUpdateOnCustomSubscription(QuoteEntity update)
        {
            builder.InvokeUpdateNotification(update);
        }

        #endregion

        #region IInvokeStrategyContext

        IPluginInvoker IInvokeStrategyContext.Builder { get { return builder; } }

        BufferUpdateResults IInvokeStrategyContext.UpdateBuffers(FeedUpdate update)
        {
            return fStrategy.UpdateBuffers(update);
        }

        void IInvokeStrategyContext.InvokeFeedEvents(FeedUpdate update)
        {
            subscriptionManager.OnUpdateEvent(update.Quote);
        }

        #endregion
    } 
}
