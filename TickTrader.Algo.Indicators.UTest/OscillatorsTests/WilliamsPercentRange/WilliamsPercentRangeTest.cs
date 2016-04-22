using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.WilliamsPercentRange
{
    [TestClass]
    public class WilliamsPercentRangeTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "WilliamsPercentRange");
            var test = new WilliamsPercentRangeTestCase(typeof (Oscillators.WilliamsPercentRange.WilliamsPercentRange),
                symbol, $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}WPR_{symbol}_{timeframe}_{period}",
                period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_14()
        {
            TestMeasures("AUDJPY", "M30", 14);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_30()
        {
            TestMeasures("AUDJPY", "M30", 30);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_12()
        {
            TestMeasures("AUDNZD", "M15", 12);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25()
        {
            TestMeasures("AUDNZD", "M15", 25);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_10()
        {
            TestMeasures("EURUSD", "H1", 10);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40()
        {
            TestMeasures("EURUSD", "H1", 40);
        }
    }
}
