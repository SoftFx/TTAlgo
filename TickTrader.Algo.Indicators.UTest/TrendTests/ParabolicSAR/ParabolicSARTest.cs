using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.ParabolicSAR
{
    [TestClass]
    public  class ParabolicSarTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, double step, double maximum)
        {
            var dir = PathHelper.MeasuresDir("Trend", "ParabolicSAR");
            var test = new ParabolicSarTestCase(typeof (Trend.ParabolicSAR.ParabolicSar), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{step:F3}_{maximum:F3}.bin",
                $"{dir}SAR_{symbol}_{timeframe}_{step:F3}_{maximum:F3}", step, maximum);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_0020_0200()
        {
            TestMeasures("AUDJPY", "M30", 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_0300_2000()
        {
            TestMeasures("AUDJPY", "M30", 0.3, 2.0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_0200_4000()
        {
            TestMeasures("AUDNZD", "M15", 0.2, 4.0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_1500_0015()
        {
            TestMeasures("AUDNZD", "M15", 1.5, 0.015);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_0010_0400()
        {
            TestMeasures("EURUSD", "H1", 0.01, 0.4);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_0800_5000()
        {
            TestMeasures("EURUSD", "H1", 0.8, 5.0);
        }
    }
}
