using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.TrendTests.AverageDirectionalMovementIndex
{
    [TestClass]
    public class AverageDirectionalMovementIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int period)
        {
            var dir = PathHelper.MeasuresDir("Trend", "AverageDirectionalMovementIndex");
            var test =
                new AverageDirectionalMovementIndexTestCase(
                    typeof (Trend.AverageDirectionalMovementIndex.AverageDirectionalMovementIndex), symbol,
                    $"{dir}bids_{symbol}_{timeframe}_{period}.bin", $"{dir}ADX_{symbol}_{timeframe}_{period}", period);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_14()
        {
            TestMeasures("AUDJPY", "M30", 14);
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
        public void TestMeasuresEURUSD_H1_5()
        {
            TestMeasures("EURUSD", "H1", 5);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_30()
        {
            TestMeasures("EURUSD", "H1", 30);
        }

        [TestMethod]
        public void TestMeasuresCADCHF_D1_14()
        {
            TestMeasures("CADCHF", "D1", 14);
        }
    }
}
