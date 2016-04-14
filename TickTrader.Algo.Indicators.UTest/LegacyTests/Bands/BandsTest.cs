using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Bands
{
    [TestClass]
    public class BandsTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, int period, double shift, double deviations)
        {
            var test = new BandsTestCase(typeof (Indicators.Bands.Bands), symbol, PathHelper.QuotesPath(dir, symbol),
                PathHelper.AnswerPath(dir, symbol, "Bands"), period, shift, deviations);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 20, 0.0, 2.0);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 20, 0.0, 2.0);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 20, 0.0, 2.0);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 20, 0.0, 2.0);
        }
    }
}
