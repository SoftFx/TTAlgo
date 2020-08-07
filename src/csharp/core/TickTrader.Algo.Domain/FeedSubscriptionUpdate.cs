namespace TickTrader.Algo.Domain
{
    public partial class FeedSubscriptionUpdate
    {
        public bool IsRemoveAction => Depth < 0;

        public bool IsUpsertAction => Depth >= 0;


        public static FeedSubscriptionUpdate Upsert(string symbol, int depth)
        {
            return new FeedSubscriptionUpdate { Symbol = symbol, Depth = depth };
        }

        public static FeedSubscriptionUpdate Remove(string symbol)
        {
            return new FeedSubscriptionUpdate { Symbol = symbol, Depth = -1 };
        }
    }
}
