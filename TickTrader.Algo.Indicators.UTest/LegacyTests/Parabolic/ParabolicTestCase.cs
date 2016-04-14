using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Parabolic
{
    public class ParabolicTestCase : LegacyTestCase
    {
        public double InpSarStep { get; set; }
        public double InpSarMaximum { get; set; }

        public ParabolicTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            double inpSarStep, double inpSarMaximum) : base(indicatorType, symbol, quotesPath, answerPath, 8, 50)
        {
            InpSarStep = inpSarStep;
            InpSarMaximum = inpSarMaximum;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("InpSARStep", InpSarStep);
            SetBuilderParameter("InpSARMaximum", InpSarMaximum);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtSARBuffer", 0);
        }
    }
}
