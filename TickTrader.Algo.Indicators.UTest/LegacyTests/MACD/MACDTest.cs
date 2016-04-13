using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.MACD
{
    [TestClass]
    public class MacdTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, int inpFastEma, int inpSlowEma, int inpSignalSma)
        {
            var test = new MacdTestCase(typeof (Indicators.MACD.MACD), symbol, PathHelper.QuotesPath(dir, symbol),
                PathHelper.AnswerPath(dir, symbol, "MACD"), inpFastEma, inpSlowEma, inpSignalSma);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 12, 26, 9);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 12, 26, 9);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 12, 26, 9);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 12, 26, 9);
        }
    }
}
