using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.StochasticOscillator
{
    [TestClass]
    public class StochasticOscillatorTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int kPeriod, int slowing, int dPeriod)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "StochasticOscillator");
            var test = new StochasticOscillatorTestCase(typeof (Oscillators.StochasticOscillator.StochasticOscillator), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{kPeriod}_{slowing}_{dPeriod}.bin",
                $"{dir}Stochastic_{symbol}_{timeframe}_{kPeriod}_{slowing}_{dPeriod}", kPeriod, slowing, dPeriod);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_5_3_3()
        {
            TestMeasures("AUDJPY", "M30", 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10_1_20()
        {
            TestMeasures("AUDJPY", "M30", 10, 1, 20);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_10_3_5()
        {
            TestMeasures("AUDNZD", "M15", 10, 3, 5);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_20_1_10()
        {
            TestMeasures("AUDNZD", "M15", 20, 1, 10);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_15_3_15()
        {
            TestMeasures("EURUSD", "H1", 15, 3, 15);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20_5_15()
        {
            TestMeasures("EURUSD", "H1", 20, 5, 15);
        }

        [TestMethod]
        public void TestMeasuresAUDCHF_M30_5_3_3()
        {
            TestMeasures("AUDCHF", "M30", 5, 3, 3);
        }
    }
}
