using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.IchimokuKinkoHyo
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
        public void TestMeasuresUSDCHF_M30_9_26_52()
        {
            TestMeasures("USDCHF", "M30", 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_9_26_52()
        {
            TestMeasures("AUDNZD", "M15", 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresEURNZD_M15_9_26_52()
        {
            TestMeasures("EURNZD", "M15", 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_9_26_52()
        {
            TestMeasures("EURUSD", "H1", 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresAUDCAD_H1_9_26_52()
        {
            TestMeasures("AUDCAD", "H1", 9, 26, 52);
        }
    }
}
