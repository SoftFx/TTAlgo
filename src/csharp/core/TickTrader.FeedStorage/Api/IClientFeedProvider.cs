using ActorSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface IClientFeedProvider
    {
        bool IsAvailable { get; }

        List<SymbolInfo> Symbols { get; }


        Task<(DateTime?, DateTime?)> GetAvailableSymbolRange(string symbol, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);


        void DownloadBars(BlockingChannel<BarData> stream, string symbol, DateTime from, DateTime to, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe);

        void DownloadQuotes(BlockingChannel<QuoteInfo> stream, string symbol, DateTime from, DateTime to, bool includeLevel2);
    }
}
