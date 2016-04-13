using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.MACD
{
    [TestClass]
    public class MacdTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int fastEma, int slowEma, int macdSma)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "MACD");
            var test = new MacdTestCase(typeof (Oscillators.MACD.Macd), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{fastEma}_{slowEma}_{macdSma}.bin",
                $"{dir}MACD_{symbol}_{timeframe}_{fastEma}_{slowEma}_{macdSma}", fastEma, slowEma, macdSma);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_12_26_9()
        {
            TestMeasures("AUDJPY", "M30", 12, 26, 9);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10_25_10()
        {
            TestMeasures("AUDJPY", "M30", 10, 25, 10);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_20_10_40()
        {
            TestMeasures("AUDNZD", "M15", 20, 10, 40);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_35_15_25()
        {
            TestMeasures("AUDNZD", "M15", 35, 15, 25);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_10_20_7()
        {
            TestMeasures("EURUSD", "H1", 10, 20, 7);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20_40_14()
        {
            TestMeasures("EURUSD", "H1", 20, 40, 14);
        }
    }
}
