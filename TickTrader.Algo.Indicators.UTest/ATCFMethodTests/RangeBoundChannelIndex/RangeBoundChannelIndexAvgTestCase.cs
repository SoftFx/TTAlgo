﻿using System;
using TickTrader.Algo.Indicators.UTest.TestCases;
using TickTrader.Algo.Indicators.UTest.Utility;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.UTest.ATCFMethodTests.RangeBoundChannelIndex
{
    public class RangeBoundChannelIndexAvgTestCase : DigitalIndicatorTestCase
    {
        public int Std { get; protected set; }
        public int CountBars { get; protected set; }

        public RangeBoundChannelIndexAvgTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int std, int countBars) : base(indicatorType, symbol, quotesPath, answerPath, 40)
        {
            Std = std;
            CountBars = countBars;
            Epsilon = 6e-9;
        }

        protected override void SetupParameters()
        {
            SetParameter("CountBars", CountBars);
            SetParameter("Std", Std);
            SetParameter("Frequency", CalcFrequency.EveryTick);
        }

        protected override void SetupInput()
        {
            BarInputHelper.MapPrice(Builder, Symbol, AppliedPrice.Close);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Rbci", 0);
            PutOutputToBuffer("UpperBound", 1);
            PutOutputToBuffer("LowerBound", 2);
            PutOutputToBuffer("UpperBound2", 3);
            PutOutputToBuffer("LowerBound2", 4);
        }
    }
}