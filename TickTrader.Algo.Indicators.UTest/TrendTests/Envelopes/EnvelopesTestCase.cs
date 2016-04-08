using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.Envelopes
{
    public class EnvelopesTestCase : MethodsPricesTestCase
    {
        public double Deviation { get; protected set; }

        public EnvelopesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period,
            int shift, double deviation) : base(indicatorType, symbol, quotesPath, answerPath, 4, 7, 16, period, shift)
        {
            Deviation = deviation;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Deviation", Deviation);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("TopLine"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("BottomLine"));
        }
    }
}
