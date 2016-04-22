using System;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.AverageDirectionalMovementIndex
{
    public class AverageDirectionalMovementIndexTestCase : PeriodPricesTestCase
    {
        public AverageDirectionalMovementIndexTestCase(Type indicatorType, string symbol, string quotesPath,
            string answerPath, int period) : base(indicatorType, symbol, quotesPath, answerPath, 24, period, 7)
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
            PutOutputToBuffer("Adx", 0);
            PutOutputToBuffer("PlusDmi", 1);
            PutOutputToBuffer("MinusDmi", 2);
        }
    }
}
