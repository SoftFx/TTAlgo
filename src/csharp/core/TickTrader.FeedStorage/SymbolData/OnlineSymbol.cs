using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;


namespace TickTrader.FeedStorage
{
    internal sealed class OnlineSymbol : BaseSymbol
    {
        private readonly FeedProvider.Handler _provider;


        public override bool IsDownloadAvailable => _provider.FeedProxy.IsAvailable;

        public override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Online;


        public OnlineSymbol(ISymbolInfo symbol, FeedProvider.Handler provider) : base(symbol, provider.Cache)
        {
            _provider = provider;
        }


        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _provider.FeedProxy.GetAvailableSymbolRange(Name, timeFrame, priceType ?? Feed.Types.MarketSide.Bid);
        }

        public override Task<ActorSharp.ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            return _provider?.DownloadBarSeriesToStorage(Name, timeframe, marketSide, from, to);
        }

        public override Task<ActorSharp.ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            return _provider.DownloadTickSeriesToStorage(Name, timeframe, from, to);
        }
    }
}
