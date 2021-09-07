using System;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.BillWilliamsTests.MarketFacilitationIndex
{
    public class MarketFacilitationIndexTestCase : SimpleTestCase
    {
        public double PointSize { get; protected set; }

        public MarketFacilitationIndexTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath,
            double pointSize) : base(indicatorType, symbol, quotesPath, answerPath, 32)
        {
            PointSize = pointSize;
        }

        protected override void SetupParameters()
        {
            SetParameter("PointSize", PointSize);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("MfiUpVolumeUp", 0);
            PutOutputToBuffer("MfiDownVolumeDown", 1);
            PutOutputToBuffer("MfiUpVolumeDown", 2);
            PutOutputToBuffer("MfiDownVolumeUp", 3);
        }
    }
}
