﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OtherTests.ZigZag
{
    [TestClass]
    public class ZigZagTest : TestBase
    {
        private void TestMeasures(string symbol, string timeframe, int depth, int deviation, int backstep, int digits)
        {
            var dir = PathHelper.MeasuresDir("Other", "ZigZag");
            var test = new ZigZagTestCase(typeof(Other.ZigZag.ZigZag), symbol,
                $"{dir}bids_{symbol}_{timeframe}_{depth}_{deviation}_{backstep}.bin",
                $"{dir}ZigZag_{symbol}_{timeframe}_{depth}_{deviation}_{backstep}",
                depth, deviation, backstep, digits);
            LaunchTestCase(test);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_12_5_3()
        {
            TestMeasures("AUDJPY", "M30", 12, 5, 3, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_32_14_5()
        {
            TestMeasures("AUDJPY", "M30", 32, 14, 5, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_15_8_4()
        {
            TestMeasures("AUDNZD", "M15", 15, 8, 4, 5);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_25_7_8()
        {
            TestMeasures("AUDNZD", "M15", 25, 7, 8, 5);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20_15_7()
        {
            TestMeasures("EURUSD", "H1", 20, 15, 7, 5);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_40_12_9()
        {
            TestMeasures("EURUSD", "H1", 40, 12, 9, 5);
        }

        [TestMethod]
        public void TestMeasuresAUDCAD_M5_12_5_3()
        {
            TestMeasures("AUDCAD", "M5", 12, 5, 3, 5);
        }
    }
}
