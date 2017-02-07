using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.Momentum
{
    [TestClass]
    public class MomentumTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "Momentum");
            var test = new MomentumTestCase(typeof (Oscillators.Momentum.Momentum), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}Momentum_{symbol}_{timeframe}_{period}", period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_14()
        {
            TestMeasures("AUDJPY", "M30", 14);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_35()
        {
            TestMeasures("AUDJPY", "M30", 35);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25()
        {
            TestMeasures("AUDNZD", "M15", 25);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_10()
        {
            TestMeasures("AUDNZD", "M15", 10);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20()
        {
            TestMeasures("EURUSD", "H1", 20);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5()
        {
            TestMeasures("EURUSD", "H1", 5);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_H4_14()
        {
            TestMeasures("AUDNZD", "H4", 14);
        }
    }
}
