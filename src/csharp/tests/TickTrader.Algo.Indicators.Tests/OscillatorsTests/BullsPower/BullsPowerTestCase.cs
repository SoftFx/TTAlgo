﻿using System;
using TickTrader.Algo.Indicators.Tests.TestCases;
using TickTrader.Algo.Indicators.Tests.Utility;

namespace TickTrader.Algo.Indicators.Tests.OscillatorsTests.BullsPower
{
    public class BullsPowerTestCase : PeriodPricesTestCase
    {
        public BullsPowerTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int period)
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
            PutOutputToBuffer("Bulls", 0);
        }
    }
}
