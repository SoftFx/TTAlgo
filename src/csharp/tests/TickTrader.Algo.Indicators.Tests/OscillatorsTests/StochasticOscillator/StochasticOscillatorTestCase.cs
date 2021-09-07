using System;
using TickTrader.Algo.Indicators.Tests.TestCases;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.StochasticOscillator
{
    public class StochasticOscillatorTestCase : MethodsPricesTestCase
    {
        public int KPeriod { get; set; }
        public int Slowing { get; set; }
        public int DPeriod { get; set; }

        public StochasticOscillatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int kPeriod, int slowing, int dPeriod) : base(indicatorType, symbol, quotesPath, answerPath, 16, 4, 2)
        {
            KPeriod = kPeriod;
            Slowing = slowing;
            DPeriod = dPeriod;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("TargetPrice", TargetPrice);
            SetParameter("KPeriod", KPeriod);
            SetParameter("Slowing", Slowing);
            SetParameter("DPeriod", DPeriod);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapBars(Builder, Symbol);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Stoch", 0);
            PutOutputToBuffer("Signal", 1);
        }
    }
}
