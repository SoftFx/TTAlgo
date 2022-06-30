namespace TickTrader.Algo.Domain
{
    public static class SubscriptionDepth
    {
        // positive values correspond to realtime subscription with specified depth
        public const int MaxDepth = int.MaxValue; // all bands available
        public const int RemoveSub = 0; // removes subscription
        // negative values are special
        public const int Ambient = int.MinValue; // default subscription defined by algo server
        public const int Tick_S0 = -1; // depth = 1, frequency = 0 
        public const int Tick_S1 = -2; // depth = 1, frequency = 1
    }


    public partial class FeedSubscriptionUpdate
    {
        public const string AllSymbolsAlias = "";


        public bool IsRemoveAction => Depth == 0;

        public bool IsUpsertAction => Depth != 0;

        public bool IsAllSymbols => Symbol == AllSymbolsAlias;


        public static FeedSubscriptionUpdate Upsert(string symbol, int depth) => new FeedSubscriptionUpdate { Symbol = symbol, Depth = depth };

        public static FeedSubscriptionUpdate Remove(string symbol) => new FeedSubscriptionUpdate { Symbol = symbol, Depth = SubscriptionDepth.RemoveSub };

        public static FeedSubscriptionUpdate UpsertAll(int depth = SubscriptionDepth.Ambient) => new FeedSubscriptionUpdate { Symbol = AllSymbolsAlias, Depth = depth };

        public static FeedSubscriptionUpdate ResetAll() => new FeedSubscriptionUpdate { Symbol = AllSymbolsAlias, Depth = SubscriptionDepth.Ambient };
    }
}
