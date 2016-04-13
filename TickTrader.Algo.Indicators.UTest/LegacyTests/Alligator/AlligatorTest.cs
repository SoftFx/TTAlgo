using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.LegacyTests.Utility;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Alligator
{
    [TestClass]
    public class AlligatorTest : TestBase
    {
        private void TestMeasures(string symbol, string dir, int inpJawsPeriod, int inpJawsShift, int inpTeethPeriod,
            int inpTeethShift, int inpLipsPeriod, int inpLipsShift)
        {
            var test = new AlligatorTestCase(typeof (Indicators.Alligator.Alligator), symbol,
                PathHelper.QuotesPath(dir, symbol), PathHelper.AnswerPath(dir, symbol, "Alligator"), inpJawsPeriod,
                inpJawsShift, inpTeethPeriod, inpTeethShift, inpLipsPeriod, inpLipsShift);
            test.InvokeFullBuildTest();
            //LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_OneDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresOneDayDir("EURUSD"), 13, 8, 8, 5, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_TwoDay()
        {
            TestMeasures("EURUSD", PathHelper.MeasuresTwoDayDir("EURUSD"), 13, 8, 8, 5, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_OneDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 13, 8, 8, 5, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresXAUUSD_TwoDay()
        {
            TestMeasures("XAUUSD", PathHelper.MeasuresOneDayDir("XAUUSD"), 13, 8, 8, 5, 5, 3);
        }
    }
}
