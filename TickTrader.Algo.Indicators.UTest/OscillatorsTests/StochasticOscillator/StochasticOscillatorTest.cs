using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.StochasticOscillator
{
    [TestClass]
    public class StochasticOscillatorTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int kPeriod, int slowing, int dPeriod)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "StochasticOscillator");
            var test = new StochasticOscillatorTestCase(typeof (Oscillators.StochasticOscillator.StochasticOscillator), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{kPeriod}_{slowing}_{dPeriod}.bin",
                $"{dir}Stoch_{symbol}_{timeframe}_{kPeriod}_{slowing}_{dPeriod}", kPeriod, slowing, dPeriod);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_5_3_3()
        {
            TestMeasures("AUDJPY", "M30", 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_20()
        {
            TestMeasures("AUDJPY", "M30", 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_15()
        {
            TestMeasures("AUDNZD", "M15", 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_40()
        {
            TestMeasures("AUDNZD", "M15", 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_10()
        {
            TestMeasures("EURUSD", "H1", 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_25()
        {
            TestMeasures("EURUSD", "H1", 5, 3, 3);
        }
    }
}
