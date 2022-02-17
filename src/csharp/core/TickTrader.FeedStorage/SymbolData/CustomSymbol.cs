using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;


namespace TickTrader.FeedStorage
{
    internal sealed class CustomSymbol : BaseSymbol
    {
        public override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Custom;


        public CustomSymbol(CustomInfo data, CustomFeedStorage.Handler storage) : base(data, storage)
        { }



        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _storage.GetRange(new FeedCacheKey(Name, timeFrame, priceType));
        }

        public override Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public override Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
    }
}
