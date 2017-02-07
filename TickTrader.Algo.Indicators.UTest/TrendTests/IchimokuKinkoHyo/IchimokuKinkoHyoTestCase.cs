using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.TrendTests.IchimokuKinkoHyo
{
    public class IchimokuKinkoHyoTestCase : SimpleTestCase
    {
        public int TenkanSen { get; protected set; }
        public int KijunSen { get; protected set; }
        public int SenkouSpanB { get; protected set; }

        public IchimokuKinkoHyoTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            int tenkanSen, int kijunSen, int senkouSpanB) : base(indicatorType, symbol, quotesPath, answerPath, 40)
        {
            TenkanSen = tenkanSen;
            KijunSen = kijunSen;
            SenkouSpanB = senkouSpanB;
        }

        protected override void SetupParameters()
        {
            SetParameter("TenkanSen", TenkanSen);
            SetParameter("KijunSen", KijunSen);
            SetParameter("SenkouSpanB", SenkouSpanB);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Tenkan", 0);
            PutOutputToBuffer("Kijun", 1);
            PutOutputToBuffer("SenkouA", 2);
            PutOutputToBuffer("SenkouB", 3);
            PutOutputToBuffer("Chikou", 4);
        }
    }
}
