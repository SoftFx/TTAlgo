using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.RangeBoundChannelIndex
{
    [TestClass]
    public class RangeBoundChannelIndexTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int deviationPeriod, double deviationCoeff)
        {
            var dir = PathHelper.MeasuresDir("ATCFMethod", "RangeBoundChannelIndex");
            var test =
                new RangeBoundChannelIndexTestCase(typeof (ATCFMethod.RangeBoundChannelIndex.RangeBoundChannelIndex),
                    symbol, $"{dir}bids_{symbol}_{timeframe}_{deviationPeriod}_{deviationCoeff:F2}.bin",
                    $"{dir}RBCI_{symbol}_{timeframe}_{deviationPeriod}_{deviationCoeff:F2}", deviationPeriod, deviationCoeff);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_100_2_50()
        {
            TestMeasures("EURUSD", "H1", 100, 2.5);
        }

        [TestMethod]
        public void TestMeasuresGBPUSD_H1_87_1_42()
        {
            TestMeasures("GBPUSD", "H1", 87, 1.42);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_H1_1_2_00()
        {
            TestMeasures("AUDJPY", "H1", 1, 2.0);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M5_100_0_00()
        {
            TestMeasures("AUDJPY", "M5", 100, 0.0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M1_50_23_00()
        {
            TestMeasures("AUDNZD", "M1", 50, 23.0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_66_66_00()
        {
            TestMeasures("AUDNZD", "M15", 66, 66.0);
        }

        [TestMethod]
        public void TestMeasuresBTCUSD_M1_61_17_00()
        {
            TestMeasures("BTCUSD", "M1", 61, 17.0);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_M5_153_4_20()
        {
            TestMeasures("EURUSD", "M5", 153, 4.2);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_M30_200_3_00()
        {
            TestMeasures("EURUSD", "M30", 200, 3.0);
        }

        [TestMethod]
        public void TestMeasuresGBPCHF_M1_33_1_01()
        {
            TestMeasures("GBPCHF", "M1", 33, 1.01);
        }

        [TestMethod]
        public void TestMeasuresGBPCHF_M5_77_0_33()
        {
            TestMeasures("GBPCHF", "M5", 77, 0.33);
        }

        [TestMethod]
        public void TestMeasuresUSDCAD_H1_99_0_00()
        {
            TestMeasures("USDCAD", "H1", 99, 0.00);
        }

        [TestMethod]
        public void TestMeasuresUSDCAD_M15_21_1_00()
        {
            TestMeasures("USDCAD", "M15", 21, 1.00);
        }
    }
}
