using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.BillWilliamsTests.Fractals
{
    public class FractalsTestCase : SimpleTestCase
    {
        public FractalsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer("FractalsUp", 0);
            PutOutputToBuffer("FractalsDown", 1);
        }
    }
}
