using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.MovingAverageOscillator
{
    [TestClass]
    public class MovingAverageOscillatorTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int fastEma, int slowEma, int macdSma)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "MovingAverageOscillator");
            var test =
                new MovingAverageOscillatorTestCase(
                    typeof (Oscillators.MovingAverageOscillator.MovingAverageOscillator), symbol,
                    $"{dir}bids_{symbol}_{timeframe}_{fastEma}_{slowEma}_{macdSma}.bin",
                    $"{dir}OsMA_{symbol}_{timeframe}_{fastEma}_{slowEma}_{macdSma}", fastEma, slowEma, macdSma);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_12_26_9()
        {
            TestMeasures("AUDJPY", "M30", 12, 26, 9);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_20_0_0()
        {
            TestMeasures("AUDJPY", "M30", 20, 0, 0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_15_0_0()
        {
            TestMeasures("AUDNZD", "M15", 15, 0, 0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_40_0_0()
        {
            TestMeasures("AUDNZD", "M15", 40, 0, 0);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_10_0_0()
        {
            TestMeasures("EURUSD", "H1", 10, 0, 0);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_25_0_0()
        {
            TestMeasures("EURUSD", "H1", 25, 0, 0);
        }
    }
}
