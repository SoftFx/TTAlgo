using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Infrastructure
{
    public interface IFeedSubscription
    {
        //void Modify(FeedSubscriptionUpdate update);
        void Modify(List<FeedSubscriptionUpdate> updates);
        //void SubscribeForAll();
        void CancelAll();
    }

    [Serializable]
    public struct FeedSubscriptionUpdate
    {
        public FeedSubscriptionUpdate(string symbol, int depth)
        {
            Symbol = symbol;
            Depth = depth;
        }

        public static FeedSubscriptionUpdate Upsert(string symbol, int depth)
        {
            return new FeedSubscriptionUpdate(symbol, depth);
        }

        public static FeedSubscriptionUpdate Remove(string symbol)
        {
            return new FeedSubscriptionUpdate(symbol, -1);
        }

        public string Symbol { get; }
        public int Depth { get; }
        public bool IsRemoveAction => Depth < 0;
        public bool IsUpsertAction => Depth >= 0;
    }
}
