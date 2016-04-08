using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Awesome
{
    [TestClass]
    public class AwesomeTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, int periodFast, int periodSlow)
        {
            var test = new AwesomeTestCase(typeof (Indicators.Awesome.Awesome), symbol,
                PathHelper.QuotesPath(dir, symbol), PathHelper.AnswerPath(dir, symbol, "Awesome"), periodFast,
                periodSlow);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 5, 34);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 5, 34);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 5, 34);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 5, 34);
        }
    }
}