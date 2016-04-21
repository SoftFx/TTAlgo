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
        public void TestMeasuresAUDJPY_M30_002_02()
        {
            TestMeasures("AUDJPY", "M30", 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresUSDCHF_M30_002_02()
        {
            TestMeasures("USDCHF", "M30", 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_002_02()
        {
            TestMeasures("AUDNZD", "M15", 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresEURNZD_M15_002_02()
        {
            TestMeasures("EURNZD", "M15", 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_002_02()
        {
            TestMeasures("EURUSD", "H1", 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresAUDCAD_H1_002_02()
        {
            TestMeasures("AUDCAD", "H1", 0.02, 0.2);
        }
    }
}
