using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.VolumesTests.MoneyFlowIndex
{
    [TestClass]
    public class MoneyFlowIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Volumes", "MoneyFlowIndex");
            var test = new MoneyFlowIndexTestCase(typeof (Indicators.Volumes.MoneyFlowIndex.MoneyFlowIndex), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}MFI_{symbol}_{timeframe}_{period}", period);
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
        public void TestMeasuresAUDNZD_M15_11()
        {
            TestMeasures("AUDNZD", "M15", 11);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_30()
        {
            TestMeasures("AUDNZD", "M15", 30);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_7()
        {
            TestMeasures("EURUSD", "H1", 7);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_36()
        {
            TestMeasures("EURUSD", "H1", 36);
        }

        [TestMethod]
        public void TestMeasuresCADCHF_D1_14()
        {
            TestMeasures("CADCHF", "D1", 14);
        }
    }
}
