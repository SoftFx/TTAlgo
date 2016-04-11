using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.BollingerBands
{
    public class BollingerBandsTestCase : PricesTestCase
    {
        public double Deviations { get; protected set; }

        public BollingerBandsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int period, int shift, double deviations)
            : base(indicatorType, symbol, quotesPath, answerPath, 24, 7, period, shift)
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
            Epsilon = 14e-8;
            base.InvokeUpdateTest();
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Deviations", Deviations);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("MiddleLine"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("TopLine"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("BottomLine"));
        }
    }
}
