using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;


namespace TickTrader.FeedStorage
{
    internal sealed class CustomSymbol : BaseSymbol
    {
        public override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Custom;


        public CustomSymbol(CustomData data, CustomFeedStorage.Handler storage) : base(data, storage)
        { }



        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _storage.GetRange(new FeedCacheKey(Name, timeFrame, priceType));
        }
    }
}
