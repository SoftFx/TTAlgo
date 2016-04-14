using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Ichimoku
{
    [TestClass]
    public class IchimokuTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, int inpTenkan, int inpKijun, int inpSenkou)
        {
            var test = new IchimokuTestCase(typeof (Indicators.Ichimoku.Ichimoku), symbol,
                PathHelper.QuotesPath(dir, symbol), PathHelper.AnswerPath(dir, symbol, "Ichimoku"), inpTenkan, inpKijun,
                inpSenkou);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 9, 26, 52);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 9, 26, 52);
        }
    }
}
