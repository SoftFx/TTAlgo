using IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Tests
{
    [TestClass]
    public class FeedCacheKeyTests
    {
        [TestMethod]
        public void Build_WithBarFrame()
        {
            var key = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.D, SymbolConfig.Types.SymbolOrigin.Online, Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key.Symbol, "EURUSD");
            Assert.AreEqual(key.TimeFrame, Feed.Types.Timeframe.D);
            Assert.AreEqual(key.Origin, SymbolConfig.Types.SymbolOrigin.Online);
            Assert.IsNotNull(key.MarketSide);
            Assert.AreEqual(key.MarketSide, Feed.Types.MarketSide.Bid);
        }

        [TestMethod]
        public void Build_WithTickFrame()
        {
            var key = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Ask);

            Assert.AreEqual(key.Symbol, "EURUSD");
            Assert.AreEqual(key.TimeFrame, Feed.Types.Timeframe.Ticks);
            Assert.AreEqual(key.Origin, SymbolConfig.Types.SymbolOrigin.Custom);
            Assert.IsNull(key.MarketSide);
        }

        [TestMethod]
        public void Build_WithTickFrame2()
        {
            var key = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.TicksLevel2);

            Assert.AreEqual(key.Symbol, "EURUSD");
            Assert.AreEqual(key.TimeFrame, Feed.Types.Timeframe.TicksLevel2);
            Assert.AreEqual(key.Origin, SymbolConfig.Types.SymbolOrigin.Online);
            Assert.IsNull(key.MarketSide);
        }

        [TestMethod]
        public void CodeString_WithBarFrame()
        {
            var key = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.M1, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key.FullInfo, "EURUSD_Online_M1_Bid");
        }

        [TestMethod]
        public void CodeString_WithTickFrame()
        {
            var key = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, SymbolConfig.Types.SymbolOrigin.Custom);

            Assert.AreEqual(key.FullInfo, "EURUSD_Custom_MN");
        }

        [TestMethod]
        public void Succ_Equals()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key1, key2);
        }

        [TestMethod]
        public void Succ_Equals2()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Online, Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Online, Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key1, key2);
        }

        [TestMethod]
        public void Succ_Equals3()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Ask);

            Assert.AreEqual(key1, key2);
            Assert.IsNull(key1.MarketSide);
            Assert.IsNull(key2.MarketSide);
        }

        [TestMethod]
        public void Succ_Equals4()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("eurusd", Feed.Types.Timeframe.Ticks, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key1, key2);
        }

        [TestMethod]
        public void Fail_Equals()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.H1, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.H1, priceType: Feed.Types.MarketSide.Ask);

            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        public void Fail_Equals2()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.H1, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURGBP", Feed.Types.Timeframe.H1, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        public void Fail_Equals3()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.H1, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        public void Fail_Equals4()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.H1, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks);

            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        public void Fail_Equals5()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Custom, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        public void Equal_HashCode()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
        }

        [TestMethod]
        public void Equal_HashCode2()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("eurusd", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
        }

        [TestMethod]
        public void NotEqual_HashCode()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Ask);

            Assert.AreNotEqual(key1.GetHashCode(), key2.GetHashCode());
        }

        [TestMethod]
        public void NotEqual_HashCode2()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.M1, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreNotEqual(key1.GetHashCode(), key2.GetHashCode());
        }

        [TestMethod]
        public void NotEqual_HashCode3()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("AUDUSD", Feed.Types.Timeframe.MN, priceType: Feed.Types.MarketSide.Bid);

            Assert.AreNotEqual(key1.GetHashCode(), key2.GetHashCode());
        }

        [TestMethod]
        public void NotEqual_HashCode4()
        {
            var key1 = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.MN, SymbolConfig.Types.SymbolOrigin.Online, Feed.Types.MarketSide.Bid);
            var key2 = new FeedCacheKey("eurusd", Feed.Types.Timeframe.MN, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Bid);

            Assert.AreNotEqual(key1.GetHashCode(), key2.GetHashCode());
        }

        [TestMethod]
        public void Succ_TryParse()
        {
            var expect = new FeedCacheKey("EURUSD", Feed.Types.Timeframe.M1, priceType: Feed.Types.MarketSide.Ask);
            var result = FeedCacheKey.TryParse($"EURUSD_Online_M1_Ask", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse2()
        {
            var expect = new FeedCacheKey("eurusd", Feed.Types.Timeframe.MN, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Bid);
            var result = FeedCacheKey.TryParse($"eurusd_custom_mn_bid", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse3()
        {
            var expect = new FeedCacheKey("a", Feed.Types.Timeframe.Ticks, priceType: Feed.Types.MarketSide.Bid);
            var result = FeedCacheKey.TryParse($"a_Online_Ticks", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse4()
        {
            var expect = new FeedCacheKey("EURUSD_test", Feed.Types.Timeframe.M1, priceType: Feed.Types.MarketSide.Ask);
            var result = FeedCacheKey.TryParse($"EURUSD_test_Online_M1_Ask", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse5()
        {
            var expect = new FeedCacheKey("eurusd_1_2_", Feed.Types.Timeframe.TicksLevel2, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Bid);
            var result = FeedCacheKey.TryParse($"eurusd_1_2__custom_tickslevel2", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse6()
        {
            var expect = new FeedCacheKey("eurusd_сustom", Feed.Types.Timeframe.Ticks, SymbolConfig.Types.SymbolOrigin.Custom, Feed.Types.MarketSide.Bid);
            var result = FeedCacheKey.TryParse($"eurusd_сustom_custom_ticks", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse7()
        {
            var expect = new FeedCacheKey("eurusd_1_2_custom_", Feed.Types.Timeframe.M1, SymbolConfig.Types.SymbolOrigin.Online, Feed.Types.MarketSide.Bid);
            var result = FeedCacheKey.TryParse($"EURUSD_1_2_CUSTOM__ONLINE_M1_BID", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Succ_TryParse8()
        {
            var expect = new FeedCacheKey("eurusd_token_online_custom", Feed.Types.Timeframe.MN, SymbolConfig.Types.SymbolOrigin.Token, Feed.Types.MarketSide.Ask);
            var result = FeedCacheKey.TryParse($"eurusd_token_online_custom_token_mN_aSk", out var actual);

            Assert.IsTrue(result);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Fail_TryParse()
        {
            var result = FeedCacheKey.TryParse(string.Empty, out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse2()
        {
            var result = FeedCacheKey.TryParse(null, out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse3()
        {
            var result = FeedCacheKey.TryParse("EURUSD_", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse4()
        {
            var result = FeedCacheKey.TryParse("EURUSD__", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse5()
        {
            var result = FeedCacheKey.TryParse("EURUSD__Ask", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse6()
        {
            var result = FeedCacheKey.TryParse("______", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse7()
        {
            var result = FeedCacheKey.TryParse("EURUSDM1Ask", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse8()
        {
            var result = FeedCacheKey.TryParse("EURUSD@M1@Ask", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse9()
        {
            var result = FeedCacheKey.TryParse("a_a_a_a_a", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse10()
        {
            var result = FeedCacheKey.TryParse("EURUSD_M4_Ask", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse11()
        {
            var result = FeedCacheKey.TryParse("EURUSD_M1_Avr", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse12()
        {
            var result = FeedCacheKey.TryParse("__M1_Ask", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse13()
        {
            var result = FeedCacheKey.TryParse("EURUSD_t_Online", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse14()
        {
            var result = FeedCacheKey.TryParse("EURUSD_t_Online_Ask", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse15()
        {
            var result = FeedCacheKey.TryParse("EURUSD_t_Online_MN_Bid_assas", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse16()
        {
            var result = FeedCacheKey.TryParse("EURUSD_Online_a_MN_Bid", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse17()
        {
            var result = FeedCacheKey.TryParse("EURUSD_Online__MN_Bid", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse18()
        {
            var result = FeedCacheKey.TryParse("EURUSD_Online_ticks_Bid", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse19()
        {
            var result = FeedCacheKey.TryParse($"EURUSD_Online_Ticks_", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void Fail_TryParse20()
        {
            var result = FeedCacheKey.TryParse($"_Online_Ticks", out var key);

            Assert.IsFalse(result);
            Assert.IsNull(key);
        }
    }
}
