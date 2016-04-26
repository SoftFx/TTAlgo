using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.AverageTrueRange
{
    [TestClass]
    public class AverageTrueRangeTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "AverageTrueRange");
            var test = new AverageTrueRangeTestCase(typeof (Oscillators.AverageTrueRange.AverageTrueRange), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}ATR_{symbol}_{timeframe}_{period}", period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_14()
        {
            TestMeasures("AUDJPY", "M30", 14);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_25()
        {
            TestMeasures("AUDJPY", "M30", 25);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_20()
        {
            TestMeasures("AUDNZD", "M15", 20);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_40()
        {
            TestMeasures("AUDNZD", "M15", 40);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_10()
        {
            TestMeasures("EURUSD", "H1", 10);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5()
        {
            TestMeasures("EURUSD", "H1", 5);
        }

        [TestMethod]
        public void TestMeasuresAUDUSD_H4_14()
        {
            TestMeasures("AUDUSD", "H4", 14);
        }
    }
}
