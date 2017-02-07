using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.ParabolicSAR
{
    public class ParabolicSarTestCase : SimpleTestCase
    {
        public double Step { get; protected set; }
        public double Maximum { get; protected set; }

        public ParabolicSarTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, double step,
            double maximum) : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            Step = step;
            Maximum = maximum;
        }

        protected override void SetupParameters()
        {
            SetParameter("Step", Step);
            SetParameter("Maximum", Maximum);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Sar", 0);
        }
    }
}
