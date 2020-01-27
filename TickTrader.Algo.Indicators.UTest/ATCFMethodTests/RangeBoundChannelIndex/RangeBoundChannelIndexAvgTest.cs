using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.RangeBoundChannelIndex
{
    [TestClass]
    public class RangeBoundChannelIndexAvgTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int std, int countBars)
        {
            var dir = PathHelper.MeasuresDir("ATCFMethod", "RangeBoundChannelIndexAvg");
            var test =
                new RangeBoundChannelIndexAvgTestCase(typeof(ATCFMethod.RangeBoundChannelIndex.RangeBoundChannelIndex),
                    symbol, $"{dir}bids_{symbol}_{timeframe}_{std}_{countBars}.bin",
                    $"{dir}RBCI_{symbol}_{timeframe}_{std}_{countBars}", std, countBars);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_18_300()
        {
            TestMeasures("AUDJPY", "M30", 18, 300);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_18_1250()
        {
            TestMeasures("AUDJPY", "M30", 18, 1250);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_18_2500()
        {
            TestMeasures("AUDJPY", "M30", 18, 2500);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10_300()
        {
            TestMeasures("AUDJPY", "M30", 10, 300);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10_1250()
        {
            TestMeasures("AUDJPY", "M30", 10, 1250);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10_2500()
        {
            TestMeasures("AUDJPY", "M30", 10, 2500);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25_300()
        {
            TestMeasures("AUDNZD", "M15", 25, 300);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25_1250()
        {
            TestMeasures("AUDNZD", "M15", 25, 1250);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25_2500()
        {
            TestMeasures("AUDNZD", "M15", 25, 2500);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_35_300()
        {
            TestMeasures("AUDNZD", "M15", 35, 300);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_35_1250()
        {
            TestMeasures("AUDNZD", "M15", 35, 1250);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_35_2500()
        {
            TestMeasures("AUDNZD", "M15", 35, 2500);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5_300()
        {
            TestMeasures("EURUSD", "H1", 5, 300);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5_1250()
        {
            TestMeasures("EURUSD", "H1", 5, 1250);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_5_2500()
        {
            TestMeasures("EURUSD", "H1", 5, 2500);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40_300()
        {
            TestMeasures("EURUSD", "H1", 40, 300);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40_1250()
        {
            TestMeasures("EURUSD", "H1", 40, 1250);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40_2500()
        {
            TestMeasures("EURUSD", "H1", 40, 2500);
        }
    }
}
