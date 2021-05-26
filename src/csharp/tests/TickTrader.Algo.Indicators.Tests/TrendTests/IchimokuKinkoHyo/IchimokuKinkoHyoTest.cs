using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.TrendTests.IchimokuKinkoHyo
{
    [TestClass]
    public class IchimokuKinkoHyoTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int tenkanSen, int kijunSen, int senkouSpanB)
        {
            var dir = PathHelper.MeasuresDir("Trend", "IchimokuKinkoHyo");
            var test = new IchimokuKinkoHyoTestCase(typeof (Trend.IchimokuKinkoHyo.IchimokuKinkoHyo), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{tenkanSen}_{kijunSen}_{senkouSpanB}.bin",
                $"{dir}Ichimoku_{symbol}_{timeframe}_{tenkanSen}_{kijunSen}_{senkouSpanB}", tenkanSen, kijunSen,
                senkouSpanB);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_9_26_52()
        {
            TestMeasures("AUDJPY", "M30", 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_18_13_40()
        {
            TestMeasures("AUDJPY", "M30", 18, 13, 40);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_40_40_40()
        {
            TestMeasures("AUDNZD", "M15", 40, 40, 40);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_7_24_47()
        {
            TestMeasures("AUDNZD", "M15", 7, 24, 47);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_45_57_110()
        {
            TestMeasures("EURUSD", "H1", 45, 57, 110);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5_15_10()
        {
            TestMeasures("EURUSD", "H1", 5, 15, 10);
        }

        [TestMethod]
        public void TestMeasuresCADCHF_D1_9_26_52()
        {
            TestMeasures("CADCHF", "D1", 9, 26, 52);
        }
    }
}
