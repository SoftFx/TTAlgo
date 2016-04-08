using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.ZigZag
{
    public class ZigZagTestCase : LegacyTestCase
    {
        public int InpDepth { get; set; }
        public int InpDeviation { get; set; }
        public int InpBackstep { get; set; }

        public ZigZagTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpDepth,
            int inpDeviation, int inpBackstep) : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            InpDepth = inpDepth;
            InpDeviation = inpDeviation;
            InpBackstep = inpBackstep;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpDepth", InpDepth);
            Builder.SetParameter("InpDeviation", InpDeviation);
            Builder.SetParameter("InpBackstep", InpBackstep);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtZigzagBuffer"));
        }
    }
}
