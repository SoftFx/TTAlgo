using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Bands
{
    public class BandsTestCase : LegacyTestCase
    {
        public int Period { get; set; }
        public double Shift { get; set; }
        public double Deviations { get; set; }

        public BandsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period,
            double shift, double deviations) : base(indicatorType, symbol, quotesPath, answerPath, 24, 20)
        {
            Period = period;
            Shift = shift;
            Deviations = deviations;
        }

        protected override void SetupInput()
        {
            Builder.MapBarInput("Close", Symbol, entity => entity.Close);
        }

        protected override void SetupParameters()
        {
            SetParameter("Period", Period);
            SetParameter("Shift", Shift);
            SetParameter("Deviations", Deviations);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtMovingBuffer", 0);
            PutOutputToBuffer("ExtUpperBuffer", 1);
            PutOutputToBuffer("ExtLowerBuffer", 2);
        }
    }
}
