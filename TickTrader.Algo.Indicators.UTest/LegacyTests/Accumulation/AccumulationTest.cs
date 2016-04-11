using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Accumulation
{
    [TestClass]
    public class AccumulationTest : TestBase
    {
        private void TestMeasures(string symbol, string dir)
        {
            var test = new AccumulationTestCase(typeof (Indicators.Accumulation.Accumulation), symbol,
                PathHelper.QuotesPath(dir, symbol), PathHelper.AnswerPath(dir, symbol, "Accumulator"));
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"));
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"));
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"));
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"));
        }
    }
}
