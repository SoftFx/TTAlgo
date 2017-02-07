using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.VolumesTests.Volumes
{
    public class VolumesTestCase : SimpleTestCase
    {
        public VolumesTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer("ValueUp", 0);
            PutOutputToBuffer("ValueDown", 1);
        }
    }
}
