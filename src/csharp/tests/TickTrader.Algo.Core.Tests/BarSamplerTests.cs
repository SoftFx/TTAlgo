﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Tests
{
    [TestClass]
    public class BarSamplerTests
    {
        [TestMethod]
        public void BarSampler_W()
        {
            var sampler = BarSampler.Get(Feed.Types.Timeframe.W);

            var b1 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-25 16:45:31.277"));
            var b2 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2016-12-26 00:00"));
            var b3 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-15 12:23"));
            var b4 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-14 12:23"));
            var b5 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-09 18:11"));

            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-22 00:00"), b1.Open);
            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-29 00:00"), b1.Close);

            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2016-12-25 00:00"), b2.Open);
            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-01 00:00"), b2.Close);

            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-15 00:00"), b3.Open);
            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-22 00:00"), b3.Close);

            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-08 00:00"), b4.Open);
            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-15 00:00"), b4.Close);

            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-08 00:00"), b5.Open);
            Assert.AreEqual(UtcTicksHelper.ParseUtcDateTime("2017-01-15 00:00"), b5.Close);
        }

        [TestMethod]
        public void BarSampler_M1()
        {
            var sampler = BarSampler.Get(Feed.Types.Timeframe.M1);

            var b1 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-16 18:25:31.277"));
            var b2 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-16 19:00"));
            var b3 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-01-16 18:59:59.999"));
            var b4 = sampler.GetBar(UtcTicksHelper.ParseLocalDateTime("2017-12-31 23:59:59.999"));

            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-01-16 18:25"), b1.Open);
            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-01-16 18:26"), b1.Close);

            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-01-16 19:00"), b2.Open);
            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-01-16 19:01"), b2.Close);

            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-01-16 18:59"), b3.Open);
            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-01-16 19:00"), b3.Close);

            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2017-12-31 23:59"), b4.Open);
            Assert.AreEqual(UtcTicksHelper.ParseLocalDateTime("2018-01-01 00:00"), b4.Close);
        }
    }
}
