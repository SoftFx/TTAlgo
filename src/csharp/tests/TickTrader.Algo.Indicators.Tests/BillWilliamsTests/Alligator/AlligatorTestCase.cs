using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.BillWilliamsTests.Alligator
{
    public class AlligatorTestCase : MethodsPricesTestCase
    {
        public int JawsPeriod { get; protected set; }
        public int JawsShift { get; protected set; }
        public int TeethPeriod { get; protected set; }
        public int TeethShift { get; protected set; }
        public int LipsPeriod { get; protected set; }
        public int LipsShift { get; protected set; }

        public AlligatorTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int jawsPeriod,
            int jawsShift, int teethPeriod, int teethShift, int lipsPeriod, int lipsShift)
            : base(indicatorType, symbol, quotesPath, answerPath, 24, 4, 7)
        {
            JawsPeriod = jawsPeriod;
            JawsShift = jawsShift;
            TeethPeriod = teethPeriod;
            TeethShift = teethShift;
            LipsPeriod = lipsPeriod;
            LipsShift = lipsShift;
        }

        protected override void SetupParameters()
        {
            base.SetupParameters();
            SetParameter("JawsPeriod", JawsPeriod);
            SetParameter("JawsShift", JawsShift);
            SetParameter("TeethPeriod", TeethPeriod);
            SetParameter("TeethShift", TeethShift);
            SetParameter("LipsPeriod", LipsPeriod);
            SetParameter("LipsShift", LipsShift);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Jaws", 0);
            PutOutputToBuffer("Teeth", 1);
            PutOutputToBuffer("Lips", 2);
        }
    }
}
