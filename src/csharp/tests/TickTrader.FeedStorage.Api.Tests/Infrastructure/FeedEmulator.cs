using ActorSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestEnviroment;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api.Tests
{
    internal sealed class FeedEmulator : IClientFeedProvider
    {
        public Dictionary<string, Dictionary<Feed.Types.Timeframe, List<BarData>>> BarFeed { get; }

        public Dictionary<string, Dictionary<Feed.Types.Timeframe, List<QuoteInfo>>> TickFeed { get; }


        public bool IsAvailable { get; private set; } = true;

        public ISymbolInfo DefaultSymbol { get; }

        public List<SymbolInfo> Symbols { get; }


        private FeedEmulator()
        {
            Symbols = new List<SymbolInfo>();
            BarFeed = new Dictionary<string, Dictionary<Feed.Types.Timeframe, List<BarData>>>();
            TickFeed = new Dictionary<string, Dictionary<Feed.Types.Timeframe, List<QuoteInfo>>>();

            DefaultSymbol = (SymbolInfo)SymbolFactory.BuildSymbol("EUR", "USD");
        }


        internal static FeedEmulator BuildDefaultEmulator()
        {
            var emulator = new FeedEmulator();

            emulator.Symbols.Add((SymbolInfo)emulator.DefaultSymbol);

            return emulator;
        }


        internal ISymbolInfo GetRandomSymbol()
        {
            var smb = (SymbolInfo)SymbolFactory.BuildSymbol(RandomGenerator.GetRandomString(3), RandomGenerator.GetRandomString(3));

            return GetUpdatedSymbol(smb);
        }

        internal ISymbolInfo GetUpdatedSymbol(ISymbolInfo smb)
        {
            return GetUpdatedSymbol(((SymbolInfo)smb).DeepCopy());
        }

        internal void GenerateBarsFeed(string symbol, Feed.Types.Timeframe timeframe, int count)
        {
            if (!BarFeed.TryGetValue(symbol, out var frameFeed))
                frameFeed = new Dictionary<Feed.Types.Timeframe, List<BarData>>();

            if (!frameFeed.TryGetValue(timeframe, out var feed))
                feed = new List<BarData>(count);

            var open = DateTime.MinValue.ToUniversalTime();

            if (timeframe == Feed.Types.Timeframe.W) // because for Forex week starts with Sunday
                open = open.AddDays(6);

            var close = open.AddTimeframe(timeframe);

            while (feed.Count < count)
            {
                var bar = RandomGenerator.GetBarData(open, close);

                open = close;
                close = open.AddTimeframe(timeframe);
                feed.Add(bar);
            }

            frameFeed.Add(timeframe, feed);
            BarFeed.Add(symbol, frameFeed);
        }


        public void DownloadBars(BlockingChannel<BarData> stream, string symbol, DateTime fromD, DateTime toD, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
        {
            Task.Run(() =>
            {
                var from = fromD.Ticks;
                var to = toD.Ticks;

                foreach (var bar in BarFeed[symbol][timeframe])
                    if (from <= bar.OpenTimeRaw && bar.CloseTimeRaw <= to)
                        stream.Write(bar);

                stream.Close();
            });
        }

        public void DownloadQuotes(BlockingChannel<QuoteInfo> stream, string symbol, DateTime from, DateTime to, bool includeLevel2)
        {
            throw new NotImplementedException();
        }

        public Task<(DateTime?, DateTime?)> GetAvailableSymbolRange(string symbol, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            throw new NotImplementedException();
        }

        private static ISymbolInfo GetUpdatedSymbol(SymbolInfo smb)
        {
            smb.MinTradeVolume = RandomGenerator.GetRandomInt(1, 10);
            smb.MaxTradeVolume = RandomGenerator.GetRandomInt(11, 100);
            smb.LotSize = RandomGenerator.GetDouble();
            smb.Margin.Factor = RandomGenerator.GetDouble();
            smb.Swap.SizeLong = RandomGenerator.GetDouble();
            smb.Swap.SizeShort = RandomGenerator.GetDouble();
            smb.Description = RandomGenerator.GetRandomString(20);

            return smb;
        }
    }
}
