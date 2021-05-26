using System;
using TickTrader.Algo.Indicators.Tests.TestCases;
using TickTrader.Algo.Indicators.Tests.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Tests.ATCFMethodTests.RangeBoundChannelIndex
{
    public class RangeBoundChannelIndexBBandsTestCase : DigitalIndicatorTestCase
    {
        public int DeviationPeriod { get; protected set; }
        public double DeviationCoeff { get; protected set; }

        public RangeBoundChannelIndexBBandsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
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
            PutOutputToBuffer("Plus2Sigma", 1);
            PutOutputToBuffer("PlusSigma", 2);
            PutOutputToBuffer("Middle", 3);
            PutOutputToBuffer("MinusSigma", 4);
            PutOutputToBuffer("Minus2Sigma", 5);
        }
    }
}
