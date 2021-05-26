using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.BullsPower
{
    [TestClass]
    public class BullsPowerTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "BullsPower");
            var test = new BullsPowerTestCase(typeof(Oscillators.BullsPower.BullsPower), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}Bulls_{symbol}_{timeframe}_{period}", period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_13()
        {
            TestMeasures("AUDJPY", "M30", 13);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_20()
        {
            TestMeasures("AUDJPY", "M30", 20);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_15()
        {
            TestMeasures("AUDNZD", "M15", 15);
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
        public void TestMeasuresEURUSD_H1_25()
        {
            TestMeasures("EURUSD", "H1", 25);
        }
    }
}
