using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core.Realtime
{
    internal class SubsciptionProxy
    {
        private Dictionary<string, SymbolSubsciptionProxy> subscibers = new Dictionary<string, SymbolSubsciptionProxy>();
        private IRealtimeFeedProvider feed;
        private ActionBlock<IRealtimeUpdate> updateQueue;

        public SubsciptionProxy(IRealtimeFeedProvider feed)
        {
            this.feed = feed;
        }

        public void Add(IPluginSubscriber subscriber)
        {
            SymbolSubsciptionProxy proxy;
            if (!subscibers.TryGetValue(subscriber.SymbolCode, out proxy))
            {
                proxy = new SymbolSubsciptionProxy(updateQueue, subscriber);
                subscibers.Add(subscriber.SymbolCode, proxy);
                feed.Subscribe(proxy);
            }
            else
                proxy.Add(subscriber);

        }

        public void Remove(IPluginSubscriber subscriber)
        {
            SymbolSubsciptionProxy proxy;
            if (subscibers.TryGetValue(subscriber.SymbolCode, out proxy))
            {
                proxy.Remove(subscriber);
                if (proxy.Count == 0)
                    feed.Unsubscribe(proxy);
            }
            else
                throw new ArgumentException("This subscriber is not found in the subscribers list!");
        }

        private class SymbolSubsciptionProxy : ISymbolFeedSubscriber
        {
            private ActionBlock<IRealtimeUpdate> updateQueue;
            private IPluginSubscriber[] symbolSubscibers;

            public SymbolSubsciptionProxy(ActionBlock<IRealtimeUpdate> updateQueue, IPluginSubscriber first)
            {
                this.updateQueue = updateQueue;
                this.symbolSubscibers = new IPluginSubscriber[] { first };
                this.Depth = first.Depth;
            }

            public int Depth { get; set; }
            public int Count { get { return symbolSubscibers.Length; } }

            public event Action DepthChanged;

            public void Add(IPluginSubscriber subscriber)
            {
                IPluginSubscriber[] newList = new IPluginSubscriber[symbolSubscibers.Length + 1];

                int maxDepth = subscriber.Depth;
                for (int i = 0; i < symbolSubscibers.Length; i++)
                {
                    newList[i] = symbolSubscibers[i];
                    if (newList[i].Depth > maxDepth)
                        maxDepth = newList[i].Depth;
                }

                newList[symbolSubscibers.Length] = subscriber;
                if (Depth != maxDepth)
                {
                    Depth = maxDepth;
                    DepthChanged();
                }
            }

            public void Remove(IPluginSubscriber subscriber)
            {
                IPluginSubscriber[] newList = new IPluginSubscriber[symbolSubscibers.Length - 1];
                int j = 0;
                int maxDepth = 0;

                if (symbolSubscibers.Length == 1 && symbolSubscibers[0] == subscriber)
                    return;

                for (int i = 0; i < symbolSubscibers.Length; i++)
                {
                    if (symbolSubscibers[i] == subscriber)
                        continue;

                    newList[j++] = symbolSubscibers[i];
                    if (symbolSubscibers[i].Depth > maxDepth)
                        maxDepth = newList[i].Depth;
                }

                if (Depth != maxDepth)
                {
                    Depth = maxDepth;
                    DepthChanged();
                }
            }

            public void OnUpdate(QuoteEntity quote)
            {
                FeedUpdate update = new FeedUpdate() { SubscribersCopy = symbolSubscibers, NewQuote = quote };
                updateQueue.SendAsync(update).Wait();
            }
        }

        private class FeedUpdate : IRealtimeUpdate
        {
            public QuoteEntity NewQuote { get; set; }
            public IPluginSubscriber[] SubscribersCopy { get; set; }

            public void Apply()
            {
                foreach (var s in SubscribersCopy)
                    s.OnUpdate(NewQuote);
            }
        }
    }

    public interface ISymbolFeedSubscriber
    {
        int Depth { get; }
        event Action DepthChanged;
        void OnUpdate(QuoteEntity quote);
    }

    internal interface IPluginSubscriber
    {
        string SymbolCode { get; }
        int Depth { get; }
        void OnUpdate(QuoteEntity quote);
    }
}
