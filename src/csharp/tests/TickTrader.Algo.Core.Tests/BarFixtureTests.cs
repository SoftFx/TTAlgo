using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TickTrader.Algo.Domain;
using Google.Protobuf.WellKnownTypes;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Core.Tests
{
    [TestClass]
    public class BarFixtureTests
    {
        [TestMethod]
        public void BarFixture_UpdateTest()
        {
            var eurusdBars = new List<BarData>()
                .Add("2017-01-16 18:23", 1.123)
                .Add("2017-01-16 18:25", 1.128)
                .Add("2017-01-16 18:27", 1.133)
                .Add("2017-01-16 18:29", 1.137);

            var context = CreateBarBasedContext();
            BarSeriesFixture mainFixture = new BarSeriesFixture("EURUSD", context, eurusdBars);

            mainFixture.UpdateRate("2017-01-16 18:29:44", 1.144);

            Assert.AreEqual(4, mainFixture.Count);
            
            mainFixture.UpdateRate("2017-01-16 18:30:38", 1.149);
            mainFixture.UpdateRate("2017-01-16 18:30:47", 1.107);
            mainFixture.UpdateRate("2017-01-16 18:30:55", 1.201);

            Assert.AreEqual(5, mainFixture.Count);

            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:29"), mainFixture.Buffer[3].OpenTime);
            Assert.AreEqual(1.137, mainFixture.Buffer[3].Open);
            Assert.AreEqual(1.144, mainFixture.Buffer[3].Close);

            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:30"), mainFixture.Buffer[4].OpenTime);
            Assert.AreEqual(1.149, mainFixture.Buffer[4].Open);
            Assert.AreEqual(1.201, mainFixture.Buffer[4].Close);
            Assert.AreEqual(1.201, mainFixture.Buffer[4].High);
            Assert.AreEqual(1.107, mainFixture.Buffer[4].Low);
        }

        [TestMethod]
        public void BarFixture_Sync_InitTest()
        {
            var eurusdBars = new List<BarData>()
                .Add("2017-01-16 18:23", 1.123)
                .Add("2017-01-16 18:25", 1.128)
                .Add("2017-01-16 18:27", 1.133)
                .Add("2017-01-16 18:29", 1.137);

            var eurcadBars = new List<BarData>()
                .Add("2017-01-16 18:24", 1.391)
                .Add("2017-01-16 18:25", 1.390)
                .Add("2017-01-16 18:27", 1.395)
                .Add("2017-01-16 18:30", 1.393);

            var context = CreateBarBasedContext();
            BarSeriesFixture mainFixture = new BarSeriesFixture("EURUSD", context, eurusdBars);
            BarSeriesFixture secondFixture = new BarSeriesFixture("EURCAD", context, eurcadBars, mainFixture);
            secondFixture.SyncByTime();

            Assert.AreEqual(4, mainFixture.Count);

            Assert.AreEqual(4, secondFixture.Count);
            Assert.AreEqual(1.391, secondFixture.Buffer[0].Open);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:25"), secondFixture.Buffer[1].OpenTime);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:27"), secondFixture.Buffer[2].OpenTime);
            Assert.AreEqual(1.395, secondFixture.Buffer[3].Open);
        }

        [TestMethod]
        public void BarFixture_Sync_UpdateTest()
        {
            var eurusdBars = new List<BarData>()
                .Add("2017-01-16 18:23", 1.123)
                .Add("2017-01-16 18:25", 1.128)
                .Add("2017-01-16 18:27", 1.133)
                .Add("2017-01-16 18:29", 1.137);

            var eurcadBars = new List<BarData>()
                .Add("2017-01-16 18:24", 1.391)
                .Add("2017-01-16 18:25", 1.390)
                .Add("2017-01-16 18:27", 1.395)
                .Add("2017-01-16 18:30", 1.393);

            var context = CreateBarBasedContext();
            BarSeriesFixture mainFixture = new BarSeriesFixture("EURUSD", context, eurusdBars);
            BarSeriesFixture secondFixture = new BarSeriesFixture("EURCAD", context, eurcadBars, mainFixture);
            secondFixture.SyncByTime();

            mainFixture.UpdateRate("2017-01-16 18:30:44", 1.144);
            
            Assert.AreEqual(5, mainFixture.Count);
            Assert.AreEqual(5, secondFixture.Count);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:30"), mainFixture.Buffer[4].OpenTime);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:30"), secondFixture.Buffer[4].OpenTime);
        }

        [TestMethod]
        public void BarFixture_Sync_UpdateEmptyBufferTest()
        {
            var eurusdBars = new List<BarData>();
            var eurcadBars = new List<BarData>();
            var context = CreateBarBasedContext();
            BarSeriesFixture mainFixture = new BarSeriesFixture("EURUSD", context, eurusdBars);
            BarSeriesFixture secondFixture = new BarSeriesFixture("EURCAD", context, eurcadBars, mainFixture);
            secondFixture.SyncByTime();

            secondFixture.UpdateRate("2017-01-16 18:30:11", 1.078);
            secondFixture.UpdateRate("2017-01-16 18:30:14", 1.079);
            mainFixture.UpdateRate("2017-01-16 18:30:44", 1.144);
            mainFixture.UpdateRate("2017-01-16 18:30:48", 1.149);
            mainFixture.UpdateRate("2017-01-16 18:31:10", 1.100);
            mainFixture.UpdateRate("2017-01-16 18:32:56", 1.113);
            secondFixture.UpdateRate("2017-01-16 18:31:36", 1.078);
            secondFixture.UpdateRate("2017-01-16 18:31:43", 1.076);
            secondFixture.UpdateRate("2017-01-16 18:31:48", 1.080);
            secondFixture.UpdateRate("2017-01-16 18:32:19", 1.101);
            secondFixture.UpdateRate("2017-01-16 18:33:29", 1.099);

            Assert.AreEqual(3, mainFixture.Count);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:30"), mainFixture.Buffer[0].OpenTime);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:31"), mainFixture.Buffer[1].OpenTime);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:32"), mainFixture.Buffer[2].OpenTime);

            Assert.AreEqual(3, secondFixture.Count);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:30"), secondFixture.Buffer[0].OpenTime);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:31"), secondFixture.Buffer[1].OpenTime);
            Assert.AreEqual(TimestampHelper.ParseLocalDateTime("2017-01-16 18:32"), secondFixture.Buffer[2].OpenTime);
        }

        private static IFixtureContext CreateBarBasedContext()
        {
            var context = new MockFixtureContext(DateTime.MinValue.ToUniversalTime().ToTimestamp(), DateTime.MaxValue.ToUniversalTime().ToTimestamp())
            {
                MainSymbolCode = "EURUSD",
                TimeFrame = Feed.Types.Timeframe.M1,
                ModelTimeFrame = Feed.Types.Timeframe.Ticks,
            };

            return context;
        }
    }
}

