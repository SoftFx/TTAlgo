using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.FeedStorage.StorageBase;

namespace TickTrader.FeedStorage
{
    internal sealed class OnlineSymbol : BaseSymbol
    {
        private new readonly OnlineFeedStorage.Handler _storage;


        public override bool IsDownloadAvailable => _storage.FeedProxy.IsAvailable;

        public override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Online;


        public OnlineSymbol(ISymbolInfo symbol, OnlineFeedStorage.Handler storage) : base(symbol, storage)
        {
            _storage = storage;
        }


        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _storage.FeedProxy.GetAvailableSymbolRange(Name, timeFrame, priceType ?? Feed.Types.MarketSide.Bid);
        }

        public override Task<ActorChannel<ISliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            return _storage?.DownloadSeriesToStorage(GetKey(timeframe, marketSide), from, to);
        }

        public override Task<ActorChannel<ISliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            return _storage?.DownloadSeriesToStorage(GetKey(timeframe), from, to);
        }
    }
}