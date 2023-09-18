using ActorSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public class DownloadDataTests : TestsBase<OnlineStorageSettings>
    {
        internal const int DefaultDataCnt = 10000;
        internal const Feed.Types.Timeframe DefaultTimeframe = Feed.Types.Timeframe.M1;
        internal const Feed.Types.MarketSide DefaultSide = Feed.Types.MarketSide.Bid;

        private const int TestTimeout = 10000;


        internal static readonly int[] DataCounts = new int[] { 1, 10, 10000, 100000 };

        internal static readonly Feed.Types.Timeframe[] BarTimeframes = new Feed.Types.Timeframe[]
        {
            Feed.Types.Timeframe.S1,
            Feed.Types.Timeframe.S10,
            Feed.Types.Timeframe.M1,
            Feed.Types.Timeframe.M5,
            Feed.Types.Timeframe.M15,
            Feed.Types.Timeframe.M30,
            Feed.Types.Timeframe.H1,
            Feed.Types.Timeframe.H4,
            Feed.Types.Timeframe.D,
            Feed.Types.Timeframe.W,
            Feed.Types.Timeframe.MN,
        };

        internal static readonly Feed.Types.Timeframe[] TickTimeframes = new Feed.Types.Timeframe[]
        {
            Feed.Types.Timeframe.Ticks,
            Feed.Types.Timeframe.TicksLevel2,
        };


        internal override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Online;

        internal string MainName => _feed.DefaultSymbol.Name;

        internal ISymbolData MainSymbol => _catalog.OnlineCollection[MainName];


        public static IEnumerable<object[]> CountAndDirectionsCmb
        {
            get
            {
                foreach (var cnt in DataCounts)
                {
                    yield return new object[] { cnt, false };
                    yield return new object[] { cnt, true };
                }
            }
        }

        public static IEnumerable<object[]> BarTimeframeDirectionsCmb
        {
            get
            {
                foreach (var frame in BarTimeframes)
                {
                    yield return new object[] { frame, false };
                    yield return new object[] { frame, true };
                }
            }
        }

        public static IEnumerable<object[]> TickTimeframeDirectionsCmb
        {
            get
            {
                foreach (var frame in TickTimeframes)
                {
                    yield return new object[] { frame, false };
                    yield return new object[] { frame, true };
                }
            }
        }


        public override async Task InitializeAsync()
        {
            await _catalog.ConnectClient(_settings);
        }


        [Theory(Timeout = TestTimeout)]
        [MemberData(nameof(CountAndDirectionsCmb))]
        public async Task Download_Bars(int count, bool reverse)
        {
            _feed.GenerateBarsFeed(MainName, DefaultTimeframe, count);

            var receivedBars = await AssertLoadData((from, to) => MainSymbol.DownloadBarSeriesToStorage(DefaultTimeframe, DefaultSide, from, to),
                                                    (from, to) => MainSymbol.GetBarStream(DefaultTimeframe, DefaultSide, from, to, reverse), count);

            var originValues = _feed.BarFeed[MainName][DefaultTimeframe];

            if (reverse)
                originValues.Reverse();

            AssertList(originValues, receivedBars);
        }

        [Theory(Timeout = TestTimeout)]
        [MemberData(nameof(CountAndDirectionsCmb))]
        public async Task Download_Ticks(int count, bool reverse)
        {
            const Feed.Types.Timeframe timeframe = Feed.Types.Timeframe.Ticks;

            _feed.GenerateTicksFeed(MainName, timeframe, count);

            var receivedTicks = await AssertLoadData((from, to) => MainSymbol.DownloadTickSeriesToStorage(timeframe, from, to),
                                                     (from, to) => MainSymbol.GetTickStream(timeframe, from, to, reverse), count);

            var originValues = _feed.TickFeed[MainName][timeframe];

            if (reverse)
                originValues.Reverse();

            AssertList(originValues, receivedTicks);
        }

        [Theory(Timeout = TestTimeout)]
        [MemberData(nameof(BarTimeframeDirectionsCmb))]
        public async Task Get_BarStream(Feed.Types.Timeframe timeframe, bool reverse)
        {
            _feed.GenerateBarsFeed(MainName, timeframe, DefaultDataCnt);

            var receivedBars = await AssertLoadData((from, to) => MainSymbol.DownloadBarSeriesToStorage(timeframe, DefaultSide, from, to),
                                                    (from, to) => MainSymbol.GetBarStream(timeframe, DefaultSide, from, to, reverse), DefaultDataCnt);

            var originValues = _feed.BarFeed[MainName][timeframe];

            if (reverse)
                originValues.Reverse();

            AssertList(originValues, receivedBars);
        }

        [Theory(Timeout = TestTimeout)]
        [MemberData(nameof(TickTimeframeDirectionsCmb))]
        public async Task Get_TickStream(Feed.Types.Timeframe timeframe, bool reverse)
        {
            _feed.GenerateTicksFeed(MainName, timeframe, DefaultDataCnt);

            var receivedBars = await AssertLoadData((from, to) => MainSymbol.DownloadTickSeriesToStorage(timeframe, from, to),
                                                    (from, to) => MainSymbol.GetTickStream(timeframe, from, to, reverse), DefaultDataCnt);

            var originValues = _feed.TickFeed[MainName][timeframe];

            if (reverse)
                originValues.Reverse();

            AssertList(originValues, receivedBars);
        }

        [Fact(Timeout = TestTimeout)]
        [Trait("Category", "Negative")]
        public async Task Get_Nonexisted_Stream_1()
        {
            var stream = await MainSymbol.GetBarStream(DefaultTimeframe, DefaultSide, DateTime.MinValue, DateTime.MaxValue);

            Assert.Null(stream);
        }

        [Fact(Timeout = TestTimeout)]
        [Trait("Category", "Negative")]
        public async Task Get_Nonexisted_Stream_2()
        {
            _feed.GenerateBarsFeed(MainName, DefaultTimeframe, 10);

            var stream = await MainSymbol.GetTickStream(Feed.Types.Timeframe.Ticks, DateTime.MinValue, DateTime.MaxValue);

            Assert.Null(stream);
        }

        [Fact(Timeout = TestTimeout)]
        [Trait("Category", "Negative")]
        public async Task Get_Nonexisted_Stream_3()
        {
            _feed.GenerateBarsFeed(MainName, DefaultTimeframe, 0);

            var stream = await MainSymbol.GetBarStream(DefaultTimeframe, DefaultSide, DateTime.MinValue, DateTime.MaxValue);

            Assert.Null(stream);
        }

        [Fact(Timeout = TestTimeout)]
        [Trait("Category", "Negative")]
        public async Task Get_Nonexisted_Stream_4()
        {
            _feed.GenerateBarsFeed(MainName, DefaultTimeframe, 10);

            var stream = await MainSymbol.GetBarStream(DefaultTimeframe, Feed.Types.MarketSide.Ask, DateTime.MinValue, DateTime.MaxValue);

            Assert.Null(stream);
        }

        [Fact(Timeout = TestTimeout)]
        [Trait("Category", "Negative")]
        public async Task Get_Nonexisted_Stream_5()
        {
            _feed.GenerateBarsFeed(MainName, DefaultTimeframe, 10);

            var stream = await MainSymbol.GetBarStream(Feed.Types.Timeframe.H1, DefaultSide, DateTime.MinValue, DateTime.MaxValue);

            Assert.Null(stream);
        }


        private async Task<List<T>> AssertLoadData<T>(Func<DateTime, DateTime, Task<ActorChannel<ISliceInfo>>> downloadStreamFactory,
                                                      Func<DateTime, DateTime, Task<IEnumerable<T>>> storageStreamFactory,
                                                      int expectedCount)
        {
            var from = DateTime.MinValue;
            var to = DateTime.MaxValue;

            var receivedCount = 0;
            var receivedData = new List<T>(expectedCount);
            var downloadStream = await downloadStreamFactory(from, to);

            while (await downloadStream.ReadNext())
                receivedCount += downloadStream.Current.Count;

            var stream = await storageStreamFactory(from, to);

            foreach (var data in stream)
                receivedData.Add(data);

            Assert.Equal(expectedCount, receivedCount);
            Assert.Equal(expectedCount, receivedData.Count);
            Assert.Equal(1, MainSymbol.Series.Count);

            return receivedData;
        }

        private static void AssertList(List<BarData> expectedList, List<BarData> actualList)
        {
            Assert.Equal(expectedList.Count, actualList.Count);

            for (int i = 0; i < expectedList.Count; ++i)
            {
                var origin = expectedList[i];
                var actual = actualList[i];

                Assert.Equal(origin.Open, actual.Open);
                Assert.Equal(origin.Close, actual.Close);
                Assert.Equal(origin.High, actual.High);
                Assert.Equal(origin.Low, actual.Low);

                Assert.Equal(origin.OpenTime, actual.OpenTime);
                Assert.Equal(origin.OpenTimeRaw, actual.OpenTimeRaw);

                Assert.Equal(origin.CloseTime, actual.CloseTime);
                Assert.Equal(origin.CloseTimeRaw, actual.CloseTimeRaw);
            }
        }

        private static void AssertList(List<QuoteInfo> expectedList, List<QuoteInfo> actualList)
        {
            Assert.Equal(expectedList.Count, actualList.Count);

            for (int i = 0; i < expectedList.Count; ++i)
            {
                var origin = expectedList[i];
                var actual = actualList[i];

                Assert.Equal(origin.TimeUtc, actual.TimeUtc);
                Assert.Equal(origin.Time, actual.Time);
                Assert.Equal(origin.Timestamp, actual.Timestamp);

                Assert.Equal(origin.Symbol, actual.Symbol);

                Assert.Equal(origin.Ask, actual.Ask);
                Assert.Equal(origin.Bid, actual.Bid);
                Assert.Equal(origin.HasAsk, actual.HasAsk);
                Assert.Equal(origin.HasBid, actual.HasBid);

                AssertSpan(origin.L2Data.Bids, actual.L2Data.Bids);
                AssertSpan(origin.L2Data.Asks, actual.L2Data.Asks);
            }
        }

        private static void AssertSpan(ReadOnlySpan<QuoteBand> origin, ReadOnlySpan<QuoteBand> actual)
        {
            Assert.Equal(origin.Length, actual.Length);

            for (int i = 0; i < origin.Length; ++i)
            {
                Assert.Equal(origin[i].Amount, actual[i].Amount);
                Assert.Equal(origin[i].Price, actual[i].Price);
            }
        }
    }
}