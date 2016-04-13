using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.RelativeStrengthIndex
{
    [TestClass]
    public class RelativeStrengthIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "RelativeStrengthIndex");
            var test =
                new RelativeStrengthIndexTestCase(typeof (Oscillators.RelativeStrengthIndex.RelativeStrengthIndex),
                    symbol, $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}RSI_{symbol}_{timeframe}_{period}",
                    period);
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
        public void TestMeasuresAUDNZD_M15_30()
        {
            TestMeasures("AUDNZD", "M15", 30);
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
        public void TestMeasuresEURUSD_H1_20()
        {
            TestMeasures("EURUSD", "H1", 20);
        }
    }
}
