using System;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.OscillatorsTests.BearsPower
{
    public class BearsPowerTestCase : PeriodPricesTestCase
    {
        public BearsPowerTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
            : base(indicatorType, symbol, quotesPath, answerPath, 8, period, 7)
        {
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("TargetPrice", TargetPrice);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapBars(Builder, Symbol);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Bears", 0);
        }
    }
}
