using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.RangeBoundChannelIndex
{
    [TestClass]
    public class RangeBoundChannelIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int std)
        {
            var dir = PathHelper.MeasuresDir("ATCFMethod", "RangeBoundChannelIndex");
            var test = new RangeBoundChannelIndexTestCase(
                typeof (ATCFMethod.RangeBoundChannelIndex.RangeBoundChannelIndex), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{std}.bin", $"{dir}RBCI_{symbol}_{timeframe}_{std}", std);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_18()
        {
            TestMeasures("AUDJPY", "M30", 18);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10()
        {
            TestMeasures("AUDJPY", "M30", 10);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25()
        {
            TestMeasures("AUDNZD", "M15", 25);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_35()
        {
            TestMeasures("AUDNZD", "M15", 35);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5()
        {
            TestMeasures("EURUSD", "H1", 5);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40()
        {
            TestMeasures("EURUSD", "H1", 40);
        }
    }
}
