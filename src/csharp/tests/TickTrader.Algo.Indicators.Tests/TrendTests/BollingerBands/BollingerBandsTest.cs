using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.TrendTests.BollingerBands
{
    [TestClass]
    public class BollingerBandsTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period, int shift, double deviations)
        {
            var dir = PathHelper.MeasuresDir("Trend", "BollingerBands");
            var test = new BollingerBandsTestCase(typeof (Trend.BollingerBands.BollingerBands), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}_{shift}_{deviations:F3}.bin",
                $"{dir}Bands_{symbol}_{timeframe}_{period}_{shift}_{deviations:F3}", period, shift, deviations);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_20_0_2000()
        {
            TestMeasures("AUDJPY", "M30", 20, 0, 2.0);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_30_minus15_3678()
        {
            TestMeasures("AUDJPY", "M30", 30, -15, 3.678);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20_minus20_1500()
        {
            TestMeasures("EURUSD", "H1", 20, -20, 1.5);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40_10_0500()
        {
            TestMeasures("EURUSD", "H1", 40, 10, 0.5);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_15_0_1000()
        {
            TestMeasures("AUDNZD", "M15", 15, 0, 1.0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_20_minus30_2400()
        {
            TestMeasures("AUDNZD", "M15", 20, -30, 2.4);
        }
    }
}
