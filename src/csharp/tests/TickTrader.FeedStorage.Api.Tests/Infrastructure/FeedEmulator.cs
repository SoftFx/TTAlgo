using ActorSharp;
using Google.Protobuf.WellKnownTypes;
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
        public bool IsAvailable { get; private set; } = true;

        public ISymbolInfo DefaultSymbol { get; }

        public List<SymbolInfo> Symbols { get; }


        private FeedEmulator()
        {
            Symbols = new List<SymbolInfo>();
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
            var smb = (SymbolInfo)SymbolFactory.BuildSymbol(RandomGenerator.GetRandomString(3) , RandomGenerator.GetRandomString(3));

            return GetUpdatedSymbol(smb);
        }

        internal ISymbolInfo GetUpdatedSymbol(ISymbolInfo smb)
        {
            return GetUpdatedSymbol(((SymbolInfo)smb).DeepCopy());
        }


        public void DownloadBars(BlockingChannel<BarData> stream, string symbol, Timestamp from, Timestamp to, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
        {
            throw new NotImplementedException();
        }

        public void DownloadQuotes(BlockingChannel<QuoteInfo> stream, string symbol, Timestamp from, Timestamp to, bool includeLevel2)
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
