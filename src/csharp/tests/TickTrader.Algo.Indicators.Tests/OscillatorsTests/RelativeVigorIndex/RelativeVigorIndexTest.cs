using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.RelativeVigorIndex
{
    [TestClass]
    public class RelativeVigorIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Oscillators", "RelativeVigorIndex");
            var test = new RelativeVigorIndexTestCase(typeof (Oscillators.RelativeVigorIndex.RelativeVigorIndex), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}RVI_{symbol}_{timeframe}_{period}", period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10()
        {
            TestMeasures("AUDJPY", "M30", 10);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_15()
        {
            TestMeasures("AUDJPY", "M30", 15);
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
        public void TestMeasuresEURUSD_H1_30()
        {
            TestMeasures("EURUSD", "H1", 30);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5()
        {
            TestMeasures("EURUSD", "H1", 5);
        }

        [TestMethod]
        public void TestMeasuresCADCHF_H1_10()
        {
            TestMeasures("CADCHF", "H1", 10);
        }
    }
}
