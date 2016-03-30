﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.MovingAverage
{
    [TestClass]
    public class MovingAverageTest
    {
        private void TestMeasures(string symbol, string timeframe, int period, int shift, double smoothFactor = 0.0)
        {
            var dir = @"..\..\TestsData\TrendTests\MovingAverage\";
            var test = new MovingAverageTestCase(typeof (Trend.MovingAverage.MovingAverage),
                $"{dir}bids_{symbol}_{timeframe}_{period}_{shift}.bin", $"{dir}MA_{symbol}_{timeframe}_{period}_{shift}",
                period, shift, smoothFactor);
            test.InvokeTest();
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_12_1()
        {
            TestMeasures("AUDJPY", "M30", 12, 1);
        }

        [TestMethod]
        public void TestMeasuresAUDJPY_M30_4_0()
        {
            TestMeasures("AUDJPY", "M30", 4, 0);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_24_3()
        {
            TestMeasures("AUDNZD", "M15", 24, 3);
        }

        [TestMethod]
        public void TestMeasuresAUDNZD_M15_6_0()
        {
            TestMeasures("AUDNZD", "M15", 6, 0);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_20_4()
        {
            TestMeasures("EURUSD", "H1", 20, 4);
        }

        [TestMethod]
        public void TestMeasuresEURUSD_H1_7_2()
        {
            TestMeasures("EURUSD", "H1", 7, 2);
        }
    }
}
