using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.ForceIndex
{
    [TestClass]
    public class ForceIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "ForceIndex");
            var test = new ForceIndexTestCase(typeof(Oscillators.ForceIndex.ForceIndex), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}Force_{symbol}_{timeframe}_{period}", period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_13()
        {
            TestMeasures("AUDJPY", "M30", 13);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_30()
        {
            TestMeasures("AUDJPY", "M30", 30);
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
        public void TestMeasuresEURUSD_H1_25()
        {
            TestMeasures("EURUSD", "H1", 25);
        }
    }
}
