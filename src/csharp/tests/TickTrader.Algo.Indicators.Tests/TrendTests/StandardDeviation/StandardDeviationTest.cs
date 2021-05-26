using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.TrendTests.StandardDeviation
{
    [TestClass]
    public class StandardDeviationTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period, int shift)
        {
            var dir = PathHelper.MeasuresDir("Trend", "StandardDeviation");
            var test = new StandardDeviationTestCase(typeof (Trend.StandardDeviation.StandardDeviation), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}_{shift}.bin",
                $"{dir}StdDev_{symbol}_{timeframe}_{period}_{shift}", period, shift);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_15_5()
        {
            TestMeasures("AUDJPY", "M30", 15, 5);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_20_0()
        {
            TestMeasures("AUDJPY", "M30", 20, 0);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_15_minus20()
        {
            TestMeasures("EURUSD", "H1", 15, -20);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_30_15()
        {
            TestMeasures("EURUSD", "H1", 30, 15);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25_minus10()
        {
            TestMeasures("AUDNZD", "M15", 25, -10);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_40_5()
        {
            TestMeasures("AUDNZD", "M15", 40, 5);
        }
    }
}
