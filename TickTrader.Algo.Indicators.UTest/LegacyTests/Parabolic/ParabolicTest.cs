using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Parabolic
{
    [TestClass]
    public class ParabolicTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, double inpSarStep, double inpSarMaximum)
        {
            var test = new ParabolicTestCase(typeof(Indicators.Parabolic.Parabolic), symbol,
                PathHelper.QuotesPath(dir, symbol), PathHelper.AnswerPath(dir, symbol, "Parabolic"), inpSarStep, inpSarMaximum);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 0.02, 0.2);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 0.02, 0.2);
        }
    }
}
