using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.BollingerBands
{
    public class BollingerBandsTestCase : PeriodShiftPricesTestCase
    {
        public double Deviations { get; protected set; }

        public BollingerBandsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period, int shift, double deviations)
            : base(indicatorType, symbol, quotesPath, answerPath, 24, period, shift, 7)
        {
            Deviations = deviations;
        }

        public override void InvokeFullBuildTest()
        {
            Epsilon = 23e-10;
            base.InvokeFullBuildTest();
        }

        public override void InvokeUpdateTest()
        {
            Epsilon = 45e-10;
            base.InvokeUpdateTest();
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("Deviations", Deviations);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("MiddleLine", 0);
            PutOutputToBuffer("TopLine", 1);
            PutOutputToBuffer("BottomLine", 2);
        }
    }
}
