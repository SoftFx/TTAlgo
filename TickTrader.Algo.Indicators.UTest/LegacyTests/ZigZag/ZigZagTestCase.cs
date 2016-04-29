﻿using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.ZigZag
{
    public class ZigZagTestCase : LegacyTestCase
    {
        public int Depth { get; set; }
        public int Deviation { get; set; }
        public int Backstep { get; set; }

        public ZigZagTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int depth,
            int deviation, int backstep) : base(indicatorType, symbol, quotesPath, answerPath, 8, 100, 100)
        {
            Depth = depth;
            Deviation = deviation;
            Backstep = backstep;
        }

        protected override void SetupParameters()
        {
            SetParameter("Depth", Depth);
            SetParameter("Deviation", Deviation);
            SetParameter("Backstep", Backstep);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Zigzag", 0);
        }
    }
}
