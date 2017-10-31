using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class QuoteDistributor
    {
        protected static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger("QuoteDistributor");

        private ClientCore _client;
        private List<Subscription> allSymbolSubscriptions = new List<Subscription>();
        private Dictionary<string, SubscriptionGroup> groups = new Dictionary<string, SubscriptionGroup>();
        private ActionBlock<Task> requestQueue;

        public QuoteDistributor(ClientCore client)
        {
            _client = client;
            _client.TickReceived += FeedProxy_Tick;
        }

        public IFeedSubscription SubscribeAll()
        {
            return new AllSymbolSubscription(this);
        }

        public IFeedSubscription Subscribe(string symbol, int depth = 1)
        {
            var subscription = new Subscription(this);
            subscription.Add(symbol, depth);
            return subscription;
        }

        public void AddSymbol(string symbol)
        {
            groups.Add(symbol, new SubscriptionGroup(symbol));
        }

        public void RemoveSymbol(string symbol)
        {
            groups.Remove(symbol);
        }

        public void Init()
        {
            foreach (var group in groups.Values)
                group.CurrentDepth = group.MaxDepth;

            requestQueue = new ActionBlock<Task>(t => t.RunSynchronously());
            EnqueuBatchSubscription();
        }

        public async Task Stop()
        {
            requestQueue.Complete();
            await requestQueue.Completion;
        }

        private void EnqueuBatchSubscription()
        {
            foreach (var group in groups.Values.GroupBy(s => s.MaxDepth))
            {
                var depth = group.Key;
                var symbols = group.Select(s => s.Symbol).ToArray();
                EnqueueSubscriptionRequest(depth, symbols);
            }
        }

        void FeedProxy_Tick(QuoteEntity e)
        {
            UpdateRate(e);
        }

        private void UpdateRate(QuoteEntity tick)
        {
            foreach (var subscription in allSymbolSubscriptions)
                subscription.OnNewQuote(tick);

            SubscriptionGroup modifier = groups.GetOrDefault(tick.Symbol);

            if (modifier != null)
            {
                foreach (var subscription in modifier.Subscriptions.Keys)
                {
                    if (!(subscription is AllSymbolSubscription))
                        subscription.OnNewQuote(tick);
                }
            }
        }

        private void AdjustSubscription(string symbol)
        {
            var modifier = groups.GetOrDefault(symbol);
            if (modifier != null)
            {
                var newDepth = modifier.MaxDepth;
                if (newDepth != modifier.CurrentDepth)
                {
                    modifier.CurrentDepth = newDepth;
                    EnqueueSubscriptionRequest(newDepth, symbol);
                }
            }
        }

        private SubscriptionGroup GetGroup(string symbol)
        {
            SubscriptionGroup group;
            groups.TryGetValue(symbol, out group);
            return group;
        }

        private Task EnqueueSubscriptionRequest(int depth, params string[] symbols)
        {
            if (requestQueue != null) // online
            {
                var subscribeTask = new Task(() =>
                {
                    _client.FeedProxy.SubscribeToQuotes(symbols, depth);
                    logger.Debug("Subscribed to " + string.Join(",", symbols));
                });
                requestQueue.Post(subscribeTask);
                return subscribeTask;
            }
            return Task.FromResult<object>(this);
        }

        private class Subscription : IFeedSubscription
        {
            protected QuoteDistributor parent;
            protected Dictionary<string, int> bySymbol = new Dictionary<string, int>();

            public event Action<QuoteEntity> NewQuote;

            public Subscription(QuoteDistributor parent)
            {
                this.parent = parent;
            }

            public virtual void OnNewQuote(QuoteEntity newQuote)
            {
                try
                {
                    NewQuote?.Invoke(newQuote);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "OnNewQuote() failed.");
                }
            }

            public virtual void Dispose()
            {
                foreach (var symbol in bySymbol.Keys)
                    parent.GetGroup(symbol).Subscriptions.Remove(this);
            }

            public virtual void Add(string symbol, int depth = 1)
            {
                AddModifier(symbol, depth);
            }

            public virtual void Remove(string symbol)
            {
                RemoveModifier(symbol);
            }

            protected void AddModifier(string symbol, int depth)
            {
                var modifier = parent.GetGroup(symbol);
                if (modifier != null)
                {
                    modifier.Add(this, depth);
                    parent.AdjustSubscription(symbol);
                }
                bySymbol[symbol] = depth;
            }

            protected void RemoveModifier(string symbol)
            {
                if (bySymbol.Remove(symbol))
                {
                    var modifier = parent.GetGroup(symbol);
                    if (modifier != null)
                    {
                        modifier.Subscriptions.Remove(this);
                        parent.AdjustSubscription(symbol);
                    }
                }
            }
        }

        private class AllSymbolSubscription : Subscription
        {
            public AllSymbolSubscription(QuoteDistributor parent) : base(parent)
            {
                parent.allSymbolSubscriptions.Add(this);
            }

            public override void Add(string symbol, int depth = 1)
            {
                if (depth == 1)
                    RemoveModifier(symbol); // subscribed for depth 1 by default
                else
                    AddModifier(symbol, depth);
            }

            public override void Dispose()
            {
                parent.allSymbolSubscriptions.Remove(this);
            }
        }

        private class SubscriptionGroup
        {
            public int CurrentDepth { get; set; }
            public Dictionary<Subscription, int> Subscriptions { get; private set; }
            public string Symbol { get; private set; }

            public SubscriptionGroup(string symbol)
            {
                this.Symbol = symbol;
                Subscriptions = new Dictionary<Subscription, int>();
            }

            public int MaxDepth
            {
                get
                {
                    int max = 1;

                    foreach (var value in Subscriptions.Values)
                    {
                        if (value == 0)
                            return 0;
                        if (value > max)
                            max = value;
                    }

                    return max;
                }
            }

            public void Add(Subscription subscription, int depth)
            {
                Subscriptions.Add(subscription, depth);
            }

            public void Remove(Subscription subscription)
            {
                Subscriptions.Remove(subscription);
            }
        }
    }

    public interface IFeedSubscription : IDisposable
    {
        void Add(string symbol, int depth = 1);
        void Remove(string symbol);
        event Action<QuoteEntity> NewQuote;
    }
}
