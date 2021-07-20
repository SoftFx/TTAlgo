namespace TickTrader.Algo.Core
{
    public interface IAccountProxy
    {
        string Id { get; }

        IAccountInfoProvider AccInfoProvider { get; }

        ITradeHistoryProvider TradeHistoryProvider { get; }

        IPluginMetadata Metadata { get; }

        IFeedProvider Feed { get; }

        IFeedHistoryProvider FeedHistory { get; }

        ITradeExecutor TradeExecutor { get; }
    }


    public class LocalAccountProxy : IAccountProxy
    {
        public string Id { get; }

        public IAccountInfoProvider AccInfoProvider { get; set; }

        public ITradeHistoryProvider TradeHistoryProvider { get; set; }

        public IPluginMetadata Metadata { get; set; }

        public IFeedProvider Feed { get; set; }

        public IFeedHistoryProvider FeedHistory { get; set; }

        public ITradeExecutor TradeExecutor { get; set; }


        public LocalAccountProxy(string id)
        {
            Id = id;
        }
    }
}
