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


        public DownloadDataTests() : base()
        {
            _catalog.ConnectClient(_settings).Wait();
        }


        [Theory]
        [MemberData(nameof(CountAndDirectionsCmb))]
        public async Task DownloadBars(int count, bool reverse)
        {
            _feed.GenerateBarsFeed(MainName, DefaultTimeframe, count);

            var receivedBars = await AssertLoadData((from, to) => MainSymbol.DownloadBarSeriesToStorage(DefaultTimeframe, DefaultSide, from, to),
                                                    (from, to) => MainSymbol.GetBarStream(DefaultTimeframe, DefaultSide, from, to, reverse), count);

            var originValues = _feed.BarFeed[MainName][DefaultTimeframe];

            if (reverse)
                originValues.Reverse();

            AssertList(originValues, receivedBars);
        }

        [Theory]
        [MemberData(nameof(BarTimeframeDirectionsCmb))]
        public async Task GetBarStream(Feed.Types.Timeframe timeframe, bool reverse)
        {
            _feed.GenerateBarsFeed(MainName, timeframe, DefaultDataCnt);

            var receivedBars = await AssertLoadData((from, to) => MainSymbol.DownloadBarSeriesToStorage(timeframe, DefaultSide, from, to),
                                                    (from, to) => MainSymbol.GetBarStream(timeframe, DefaultSide, from, to, reverse), DefaultDataCnt);

            var originValues = _feed.BarFeed[MainName][timeframe];

            if (reverse)
                originValues.Reverse();

            AssertList(originValues, receivedBars);
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
    }
}