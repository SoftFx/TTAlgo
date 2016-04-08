using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.HeikenAshi
{
    public class HeikenAshiTestCase : LegacyTestCase
    {
        public HeikenAshiTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 32)
        {
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtLowHighBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtHighLowBuffer"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("ExtOpenBuffer"));
            AnswerBuffer[3] = new List<double>(Builder.GetOutput<double>("ExtCloseBuffer"));
        }
    }
}
