using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.ZigZag
{
    public class ZigZagTestCase : LegacyTestCase
    {
        public int InpDepth { get; set; }
        public int InpDeviation { get; set; }
        public int InpBackstep { get; set; }

        public ZigZagTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpDepth,
            int inpDeviation, int inpBackstep) : base(indicatorType, symbol, quotesPath, answerPath, 8, 100, 100)
        {
            InpDepth = inpDepth;
            InpDeviation = inpDeviation;
            InpBackstep = inpBackstep;
        }

        protected override void SetupParameters()
        {
            SetParameter("Depth", InpDepth);
            SetParameter("Deviation", InpDeviation);
            SetParameter("Backstep", InpBackstep);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Zigzag", 0);
        }
    }
}
