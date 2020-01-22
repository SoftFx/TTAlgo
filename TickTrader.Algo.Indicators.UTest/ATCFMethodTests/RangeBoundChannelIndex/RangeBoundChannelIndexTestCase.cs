using System;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.RangeBoundChannelIndex
{
    public class RangeBoundChannelIndexTestCase : DigitalIndicatorTestCase
    {
        public int DeviationPeriod { get; protected set; }
        public double DeviationCoeff { get; protected set; }

        public RangeBoundChannelIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int deviationPeriod, double deviationCoeff) : base(indicatorType, symbol, quotesPath, answerPath, 48)
        {
            DeviationPeriod = deviationPeriod;
            DeviationCoeff = deviationCoeff;
            Epsilon = 6e-9;
        }

        protected override void SetupParameters()
        {
            SetParameter("DeviationPeriod", DeviationPeriod);
            SetParameter("DeviationCoeff", DeviationCoeff);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Rbci", 0);
            PutOutputToBuffer("UpperBound2", 1);
            PutOutputToBuffer("UpperBound", 2);
            PutOutputToBuffer("Middle", 3);
            PutOutputToBuffer("LowerBound", 4);
            PutOutputToBuffer("LowerBound2", 5);
        }
    }
}
