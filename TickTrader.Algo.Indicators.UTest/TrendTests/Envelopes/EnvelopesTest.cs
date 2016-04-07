using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.Envelopes
{
    [TestClass]
    public class EnvelopesTest
    {
        private void TestMeasures(string symbol, string timeframe, int period, int shift, double deviation)
        {
            var dir = @"..\..\TestsData\TrendTests\Envelopes\";
            var test = new EnvelopesTestCase(typeof(Trend.Envelopes.Envelopes),
                $"{dir}bids_{symbol}_{timeframe}_{period}_{shift}_{deviation:F3}.bin", $"{dir}Envelopes_{symbol}_{timeframe}_{period}_{shift}_{deviation:F3}",
                period, shift, deviation);
            test.InvokeTest();
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_14_0_0100()
        {
            TestMeasures("AUDJPY", "M30", 14, 0, 0.1);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_10_1_0200()
        {
            TestMeasures("AUDJPY", "M30", 10, 1, 0.2);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_10_minus3_0600()
        {
            TestMeasures("AUDNZD", "M15", 10, -3, 0.6);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_5_minus10_0300()
        {
            TestMeasures("AUDNZD", "M15", 5, -10, 0.3);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20_3_0005()
        {
            TestMeasures("EURUSD", "H1", 20, 3, 0.005);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_25_minus6_0100()
        {
            TestMeasures("EURUSD", "H1", 25, -6, 0.1);
        }
    }
}
