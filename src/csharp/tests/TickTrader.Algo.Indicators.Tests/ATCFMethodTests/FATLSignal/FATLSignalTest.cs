using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.ATCFMethodTests.FATLSignal
{
    [TestClass]
    public class FatlSignalTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int digits)
        {
            var dir = PathHelper.MeasuresDir("ATCFMethod", "FATLSignal");
            var test = new FatlSignalTestCase(typeof (ATCFMethod.FATLSignal.FatlSignal), symbol,
                $"{dir}bids_{symbol}_{timeframe}.bin", $"{dir}FATLs_{symbol}_{timeframe}", digits);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30()
        {
            TestMeasures("AUDJPY", "M30", 3);
        }

        [TestMethod]
        public void TestMeasuresUSDCHF_M30()
        {
            TestMeasures("USDCHF", "M30", 5);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15()
        {
            TestMeasures("AUDNZD", "M15", 5);
        }

        [TestMethod]
        public void TestMeasuresEURNZD_M15()
        {
            TestMeasures("EURNZD", "M15", 5);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1()
        {
            TestMeasures("EURUSD", "H1", 5);
        }

        [TestMethod]
        public void TestMeasuresAUDCAD_H1()
        {
            TestMeasures("AUDCAD", "H1", 5);
        }
    }
}
