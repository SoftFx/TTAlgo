using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class QuoteDistributor
    {
        protected static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger("QuoteDistributor");

        //private ClientModel _client;
        private List<Subscription> allSymbolSubscriptions = new List<Subscription>();
        private Dictionary<string, SubscriptionGroup> groups = new Dictionary<string, SubscriptionGroup>();
        //private ActionBlock<SubscriptionTask> requestQueue;
        //private CancellationTokenSource cancelAllrequests;
        private IQuoteDistributorSource _src;

        internal QuoteDistributor(IQuoteDistributorSource src)
        {
            _src = src;
            //_client = client;
            //_client.TickReceived += FeedProxy_Tick;
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

        public IEnumerable<Tuple<int, string>> GetAllSubscriptions(IEnumerable<string> allSymbols)
        {
            foreach (var smb in allSymbols)
            {
                var group = groups.GetOrDefault(smb);
                if (group == null)
                    yield return new Tuple<int, string>(1, smb);
                else
                    yield return new Tuple<int, string>(group.Depth, smb);
            }
        }

        //public async Task Init()
        //{
        //    foreach (var group in groups.Values)
        //        group.CurrentDepth = group.MaxDepth;

        //    StartQueue();
        //    await DoBatchSubscription();
        //    await GetQuoteSnapshot();
        //}

        //public async Task Stop()
        //{
        //    await StopQueue();
        //}

        //private async Task GetQuoteSnapshot()
        //{
        //    try
        //    {
        //        foreach (var group in groups.Values.GroupBy(s => s.MaxDepth))
        //        {
        //            var depth = group.Key;
        //            var symbols = group.Select(s => s.Symbol).ToArray();
        //            EnqueueSubscriptionRequest(depth, symbols);

        //            //var quotes = await _client.FeedProxy.GetQuoteSnapshot(symbols, depth);
        //            //quotes.Foreach(UpdateRate);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Failed to get snapshot! " + ex.Message);
        //    }
        //}

        //private void StartQueue()
        //{
        //    cancelAllrequests = new CancellationTokenSource();
        //    var queueOptions = new ExecutionDataflowBlockOptions { CancellationToken = cancelAllrequests.Token };

        //    requestQueue = new ActionBlock<SubscriptionTask>(InvokeSubscribeAsync);
        //}

        //private async Task StopQueue()
        //{
        //    if (requestQueue != null)
        //    {
        //        cancelAllrequests.Cancel();
        //        requestQueue.Complete();
        //        await requestQueue.Completion;
        //        cancelAllrequests = null;
        //        requestQueue = null;
        //    }
        //}

        //private async Task DoBatchSubscription()
        //{
        //    foreach (var group in groups.Values.GroupBy(s => s.MaxDepth))
        //    {
        //        var depth = group.Key;
        //        var symbols = group.Select(s => s.Symbol).ToArray();
        //        EnqueueSubscriptionRequest(depth, symbols);

        //        await InvokeSubscribeAsync(new SubscriptionNotification(symbols, depth));
        //    }
        //}


        internal void UpdateRate(QuoteEntity tick)
        {
            foreach (var subscription in allSymbolSubscriptions)
                subscription.OnNewQuote(tick);

            SubscriptionGroup group = groups.GetOrDefault(tick.Symbol);

            if (group != null)
            {
                foreach (var subscription in group.Subscriptions.Keys)
                {
                    if (!(subscription is AllSymbolSubscription))
                        subscription.OnNewQuote(tick);
                }
            }
        }

        private void AdjustSubscription(string symbol)
        {
            var group = groups.GetOrDefault(symbol);
            if (group != null)
            {
                var oldDepth = group.Depth;
                var newDepth = group.GetMaxDepth();
                if (newDepth != oldDepth)
                {
                    group.Depth = newDepth;
                    _src.ModifySubscription(symbol, newDepth);
                    //EnqueueSubscriptionRequest(newDepth, symbol);
                }
            }
        }

        private SubscriptionGroup GetGroup(string symbol)
        {
            SubscriptionGroup group;
            if (!groups.TryGetValue(symbol, out group))
            {
                group = new SubscriptionGroup(symbol);
                groups.Add(symbol, group);
            }
            return group;
        }

        private void RemoveIfEmpty(SubscriptionGroup group)
        {
            if (group.Subscriptions.Count == 0)
                groups.Remove(group.Symbol);
        }

        //private void EnqueueSubscriptionRequest(int depth, params string[] symbols)
        //{
        //    if (requestQueue != null) // online
        //        requestQueue.Post(new SubscriptionNotification(symbols, depth));
        //}

        //private async Task InvokeSubscribeAsync(SubscriptionTask task)
        //{
        //    try
        //    {
        //        //await _client.FeedProxy.SubscribeToQuotes(task.Symbols, task.Depth);
        //        logger.Debug("Subscribed to " + string.Join(",", task.Symbols));
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Failed to subscribe! " + ex.Message);
        //    }
        //}

        private struct SubscriptionNotification
        {
            public SubscriptionNotification(string[] symbols, int depth)
            {
                Symbols = symbols;
                Depth = depth;
            }

            public string[] Symbols { get; }
            public int Depth { get; }
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
                    NewQuote?.Invoke(TruncateQuote(newQuote));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "OnNewQuote() failed.");
                }
            }

            public virtual void Dispose()
            {
                foreach (var symbol in bySymbol.Keys)
                {
                    var group = parent.GetGroup(symbol);
                    group.Subscriptions.Remove(this);
                    parent.RemoveIfEmpty(group);
                }
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
                var group = parent.GetGroup(symbol);
                if (group != null)
                {
                    group.Add(this, depth);
                    parent.AdjustSubscription(symbol);
                }
                bySymbol[symbol] = depth;
            }

            protected void RemoveModifier(string symbol)
            {
                if (bySymbol.Remove(symbol))
                {
                    var group = parent.GetGroup(symbol);
                    if (group != null)
                    {
                        group.Subscriptions.Remove(this);
                        parent.RemoveIfEmpty(group);
                        parent.AdjustSubscription(symbol);
                    }
                }
            }

            protected QuoteEntity TruncateQuote(QuoteEntity quote)
            {
                if (bySymbol.TryGetValue(quote.Symbol, out var depth) && depth == 0)
                {
                    return quote;
                }
                depth = depth < 1 ? 1 : depth;
                return new QuoteEntity(quote.Symbol, quote.CreatingTime, quote.BidList.Take(depth).ToArray(), quote.AskList.Take(depth).ToArray());
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
                base.Dispose();
                parent.allSymbolSubscriptions.Remove(this);
            }
        }

        private class SubscriptionGroup
        {
            public int Depth { get; set; }
            public Dictionary<Subscription, int> Subscriptions { get; private set; }
            public string Symbol { get; private set; }

            public SubscriptionGroup(string symbol)
            {
                this.Symbol = symbol;
                Subscriptions = new Dictionary<Subscription, int>();
                Depth = 1;
            }

            public int GetMaxDepth()
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

            public void Add(Subscription subscription, int depth)
            {
                Subscriptions[subscription] = depth;
            }

            public void Remove(Subscription subscription)
            {
                Subscriptions.Remove(subscription);
            }
        }
    }

    internal interface IQuoteDistributorSource
    {
        void ModifySubscription(string symbol, int depth);
    }

    public interface IFeedSubscription : IDisposable
    {
        void Add(string symbol, int depth = 1);
        void Remove(string symbol);
        event Action<QuoteEntity> NewQuote;
    }
}
