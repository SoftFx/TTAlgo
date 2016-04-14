using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.ZigZag
{
    [TestClass]
    public class ZigZagTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, int inpDepth, int inpDeviation, int inpBackstep)
        {
            var test = new ZigZagTestCase(typeof (Indicators.ZigZag.ZigZag), symbol, PathHelper.QuotesPath(dir, symbol),
                PathHelper.AnswerPath(dir, symbol, "ZigZag"), inpDepth, inpDeviation, inpBackstep);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 12, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 12, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 12, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 12, 5, 3);
        }
    }
}
