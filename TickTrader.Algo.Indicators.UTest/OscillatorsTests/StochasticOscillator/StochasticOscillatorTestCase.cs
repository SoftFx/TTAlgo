using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.StochasticOscillator
{
    public class StochasticOscillatorTestCase : MethodsTestCase
    {
        public int KPeriod { get; set; }
        public int Slowing { get; set; }
        public int DPeriod { get; set; }

        public StochasticOscillatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int kPeriod, int slowing, int dPeriod) : base(indicatorType, symbol, quotesPath, answerPath, 16, 4)
        {
            KPeriod = kPeriod;
            Slowing = slowing;
            DPeriod = dPeriod;
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Stoch", 0);
            PutOutputToBuffer("Signal", 1);
        }
    }
}
