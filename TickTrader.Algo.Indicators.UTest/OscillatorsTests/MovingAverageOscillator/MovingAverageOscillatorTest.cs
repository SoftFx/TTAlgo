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
        public void TestMeasuresAUDJPY_M30_20_10_40()
        {
            TestMeasures("AUDJPY", "M30", 20, 10, 40);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_10_25_10()
        {
            TestMeasures("AUDNZD", "M15", 10, 25, 10);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_20_40_14()
        {
            TestMeasures("AUDNZD", "M15", 20, 40, 14);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_10_20_7()
        {
            TestMeasures("EURUSD", "H1", 10, 20, 7);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_35_15_25()
        {
            TestMeasures("EURUSD", "H1", 35, 15, 25);
        }
    }
}
