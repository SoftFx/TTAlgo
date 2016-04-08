using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Bands
{
    public class BandsTestCase : LegacyTestCase
    {
        public int Period { get; set; }
        public double Shift { get; set; }
        public double Deviations { get; set; }

        public BandsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period,
            double shift, double deviations) : base(indicatorType, symbol, quotesPath, answerPath, 24)
        {
            Period = period;
            Shift = shift;
            Deviations = deviations;
        }

        protected override void SetupInput()
        {
            Builder.MapBarInput("Close", Symbol, entity => entity.Close);
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("Period", Period);
            Builder.SetParameter("Shift", Shift);
            Builder.SetParameter("Deviations", Deviations);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtMovingBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtUpperBuffer"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("ExtLowerBuffer"));
        }
    }
}
