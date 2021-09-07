using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.VolumesTests.AccumulationDistribution
{
    public class AccumulationDistributionTestCase : SimpleTestCase
    {
        public AccumulationDistributionTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Ad", 0);
        }
    }
}
