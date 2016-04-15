using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.AcceleratorOscillator
{
    [TestClass]
    public class AcceleratorOscillatorTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe)
        {
            var dir = PathHelper.MeasuresDir("BillWilliams", "AcceleratorOscillator");
            var test = new AcceleratorOscillatorTestCase(typeof(BillWilliams.AcceleratorOscillator.AcceleratorOscillator), symbol,
                $"{dir}bids_{symbol}_{timeframe}.bin", $"{dir}AC_{symbol}_{timeframe}");
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30()
        {
            TestMeasures("AUDJPY", "M30");
        }

        [TestMethod]
        public void TestMeasuresUSDCHF_M30()
        {
            TestMeasures("USDCHF", "M30");
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15()
        {
            TestMeasures("AUDNZD", "M15");
        }

        [TestMethod]
        public void TestMeasuresEURNZD_M15()
        {
            TestMeasures("EURNZD", "M15");
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1()
        {
            TestMeasures("EURUSD", "H1");
        }

        [TestMethod]
        public void TestMeasuresAUDCAD_H1()
        {
            TestMeasures("AUDCAD", "H1");
        }
    }
}
