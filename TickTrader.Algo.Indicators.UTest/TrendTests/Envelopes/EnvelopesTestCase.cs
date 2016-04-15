using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.Envelopes
{
    public class EnvelopesTestCase : PeriodShiftMethodsPricesTestCase
    {
        public double Deviation { get; protected set; }

        public EnvelopesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period,
            int shift, double deviation) : base(indicatorType, symbol, quotesPath, answerPath, 16, period, shift, 4, 7)
        {
            Deviation = deviation;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("Deviation", Deviation);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("TopLine", 0);
            PutOutputToBuffer("BottomLine", 1);
        }
    }
}
