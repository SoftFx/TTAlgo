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
    public class PluginExecutor : NoTimeoutByRefObject, IFeedStrategyContext, IInvokeStrategyContext
    {
        private object _sync = new object();
        private bool isInitialized;
        private bool isStarted;
        //private Func<PluginBuilder> builderFactory;
        private IPluginFeedProvider feed;
        //private SubscriptionManager subscriptionManager;
        private FeedStrategy fStrategy;
        private InvokeStartegy iStrategy;
        private string mainSymbol;
        private PluginBuilder builder;
        private DateTime periodStart;
        private DateTime periodEnd;
        private Api.TimeFrames timeframe;
        private List<Action> setupActions = new List<Action>();
        private AlgoPluginDescriptor descriptor;

        public PluginExecutor(string pluginId)
        {
            this.descriptor = AlgoPluginDescriptor.Get(pluginId);
            //if (builderFactory == null)
            //    throw new ArgumentNullException("builderFactory");

            //this.builderFactory = builderFactory;
        }

        #region Properties

        public IPluginFeedProvider FeedProvider
        {
            get { return feed; }
            set
            {
                lock (_sync)
                {
                    ThrowIfInitialized();

                    if (value == null)
                        throw new InvalidOperationException("FeedProvider cannot be null!");

                    feed = value;
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
                    ThrowIfInitialized();

                    if (value == null)
                        throw new InvalidOperationException("FeedStrategy cannot be null!");

                    fStrategy = value;
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
                    ThrowIfInitialized();

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
                    ThrowIfInitialized();

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
                    ThrowIfInitialized();
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
                    ThrowIfInitialized();
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
                    ThrowIfInitialized();
                    timeframe = value;
                }
            }
        }

        #endregion

        public void Start()
        {
            lock (_sync)
            {
                Validate();

                isInitialized = true;
                isStarted = true;

                feed.FeedUpdated += Feed_FeedUpdated;
                //subscriptionManager = new SubscriptionManager(feed);
                builder = new PluginBuilder(descriptor);
                builder.MainSymbol = MainSymbolCode;
                builder.Symbols.Init(feed.GetSymbolMetadata());
                fStrategy.Start(this);
                setupActions.ForEach(a => a());
                iStrategy.Start(this, fStrategy.BufferSize);
            }
        }

        public void Stop(bool cancelPendingUpdates = true)
        {
            lock (_sync)
            {
                if (!isStarted)
                    return;

                isStarted = false;
                fStrategy.Stop();
                feed.FeedUpdated -= Feed_FeedUpdated;
                iStrategy.Stop(cancelPendingUpdates).Wait();
            }
        }

        #region Setup Methods

        public void MapBarInput<TVal>(string inputName, string symbolCode, Func<BarEntity, TVal> selector)
        {
            setupActions.Add(() => fStrategy.MapInput(inputName, symbolCode, selector));
        }

        public void MapBarInput(string inputName, string symbolCode)
        {
            setupActions.Add(() => fStrategy.MapInput<BarEntity, Api.Bar>(inputName, symbolCode, b => b));
        }

        public void SetParameter(string name, object value)
        {
            setupActions.Add(() => builder.SetParameter(name, value));
        }

        #endregion

        private void Validate()
        {
            if (isStarted)
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

        private void ThrowIfInitialized()
        {
            if (isInitialized)
                throw new InvalidOperationException("Executor parameters cannot be changed after start!");
        }

        private void Feed_FeedUpdated(FeedUpdate[] updates)
        {
            iStrategy.OnUpdate(updates).Wait();
        }

        #region IFeedStrategyContext

        PluginBuilder IFeedStrategyContext.Builder { get { return builder; } }

        IEnumerable<BarEntity> IFeedStrategyContext.QueryBars(string symbolCode, DateTime from, DateTime to, TimeFrames timeFrame)
        {
            return feed.QueryBars(symbolCode, from, to, timeFrame);
        }

        void IFeedStrategyContext.Subscribe(string symbolCode, int depth)
        {
            feed.Subscribe(symbolCode, depth);
        }

        void IFeedStrategyContext.Unsubscribe(string symbolCode)
        {
            feed.Unsubscribe(symbolCode);
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
        }

        #endregion
    } 
}
