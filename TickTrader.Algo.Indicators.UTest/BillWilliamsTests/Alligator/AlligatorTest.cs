using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.Alligator
{
    [TestClass]
    public class AlligatorTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int jawsPeriod, int jawsShift,
            int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
        {
            var dir = PathHelper.MeasuresDir("Trend", "Alligator");
            var test = new AlligatorTestCase(typeof (BillWilliams.Alligator.Alligator), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{jawsPeriod}_{jawsShift}_{teethPeriod}_{teethShift}_{lipsPeriod}_{lipsShift}.bin",
                $"{dir}Alligator_{symbol}_{timeframe}_{jawsPeriod}_{jawsShift}_{teethPeriod}_{teethShift}_{lipsPeriod}_{lipsShift}",
                jawsPeriod, jawsShift, teethPeriod, teethShift, lipsPeriod, lipsShift);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_13_8_8_5_5_3()
        {
            TestMeasures("AUDJPY", "M30", 13, 8, 8, 5, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_20_15_15_10_10_5()
        {
            TestMeasures("AUDJPY", "M30", 20, 15, 15, 10, 10, 5);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_10_8_8_6_6_4()
        {
            TestMeasures("AUDNZD", "M15", 10, 8, 8, 6, 6, 4);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25_3_20_2_15_1()
        {
            TestMeasures("AUDNZD", "M15", 25, 3, 20, 2, 15, 1);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_13_10_10_7_7_4()
        {
            TestMeasures("EURUSD", "H1", 13, 10, 10, 7, 7, 4);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_14_12_11_9_8_6()
        {
            TestMeasures("EURUSD", "H1", 14, 12, 11, 9, 8, 6);
        }
    }
}
