namespace TickTrader.Algo.Domain
{
    public static class SubscriptionDepth
    {
        // positive values correspond to realtime subscription with specified depth
        public const int AllBands = int.MaxValue;
        public const int RemoveSub = 0; // removes subscription
        // negative values are special
        public const int Ambient = int.MinValue; // default subscription defined by algo server
        public const int Tick_S0 = -1; // depth = 1, frequency = 0 
        public const int Tick_S1 = -2; // depth = 1, frequency = 1
    }


    public partial class FeedSubscriptionUpdate
    {
        public bool IsRemoveAction => Depth == 0;

        public bool IsUpsertAction => Depth != 0;


        public static FeedSubscriptionUpdate Upsert(string symbol, int depth)
        {
            return new FeedSubscriptionUpdate { Symbol = symbol, Depth = depth };
        }

        public static FeedSubscriptionUpdate Remove(string symbol)
        {
            return new FeedSubscriptionUpdate { Symbol = symbol, Depth = SubscriptionDepth.RemoveSub };
        }
    }
}
