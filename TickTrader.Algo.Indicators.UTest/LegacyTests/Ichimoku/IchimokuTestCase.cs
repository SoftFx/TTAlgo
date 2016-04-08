using System;
using System.Collections.Generic;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Ichimoku
{
    public class IchimokuTestCase : LegacyTestCase
    {
        public int InpTenkan { get; set; }
        public int InpKijun { get; set; }
        public int InpSenkou { get; set; }

        public IchimokuTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpTenkan,
            int inpKijun, int inpSenkou) : base(indicatorType, symbol, quotesPath, answerPath, 56)
        {
            InpTenkan = inpTenkan;
            InpKijun = inpKijun;
            InpSenkou = inpSenkou;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            Builder.SetParameter("InpTenkan", InpTenkan);
            Builder.SetParameter("InpKijun", InpKijun);
            Builder.SetParameter("InpSenkou", InpSenkou);
        }

        protected override void GetOutput()
        {
            AnswerBuffer[0] = new List<double>(Builder.GetOutput<double>("ExtTenkanBuffer"));
            AnswerBuffer[1] = new List<double>(Builder.GetOutput<double>("ExtKijunBuffer"));
            AnswerBuffer[2] = new List<double>(Builder.GetOutput<double>("ExtSpanA_Buffer"));
            AnswerBuffer[3] = new List<double>(Builder.GetOutput<double>("ExtSpanB_Buffer"));
            AnswerBuffer[4] = new List<double>(Builder.GetOutput<double>("ExtChikouBuffer"));
            AnswerBuffer[5] = new List<double>(Builder.GetOutput<double>("ExtSpanA2_Buffer"));
            AnswerBuffer[6] = new List<double>(Builder.GetOutput<double>("ExtSpanB2_Buffer"));
        }
    }
}
