using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.LegacyTests.Ichimoku
{
    public class IchimokuTestCase : LegacyTestCase
    {
        public int InpTenkan { get; set; }
        public int InpKijun { get; set; }
        public int InpSenkou { get; set; }

        public IchimokuTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int inpTenkan,
            int inpKijun, int inpSenkou) : base(indicatorType, symbol, quotesPath, answerPath, 56, 77)
        {
            InpTenkan = inpTenkan;
            InpKijun = inpKijun;
            InpSenkou = inpSenkou;
        }

        protected override void SetupBuilder()
        {
            base.SetupBuilder();
            SetBuilderParameter("InpTenkan", InpTenkan);
            SetBuilderParameter("InpKijun", InpKijun);
            SetBuilderParameter("InpSenkou", InpSenkou);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ExtTenkanBuffer", 0);
            PutOutputToBuffer("ExtKijunBuffer", 1);
            PutOutputToBuffer("ExtSpanA_Buffer", 2);
            PutOutputToBuffer("ExtSpanB_Buffer", 3);
            PutOutputToBuffer("ExtChikouBuffer", 4);
            PutOutputToBuffer("ExtSpanA2_Buffer", 5);
            PutOutputToBuffer("ExtSpanB2_Buffer", 6);
        }
    }
}
