using System;
using TickTrader.Algo.Indicators.UTest.TestCases;

namespace TickTrader.Algo.Indicators.UTest.OtherTests.ZigZag
{
    public class ZigZagTestCase : SimpleTestCase
    {
        public int Depth { get; protected set; }
        public int Deviation { get; protected set; }
        public int Backstep { get; protected set; }
        public double PointSize { get; protected set; }

        public ZigZagTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath, int depth,
            int deviation, int backstep, double pointSize) : base(indicatorType, symbol, quotesPath, answerPath, 8)
        {
            Depth = depth;
            Deviation = deviation;
            Backstep = backstep;
            PointSize = pointSize;
        }

        protected override void SetupParameters()
        {
            SetParameter("Depth", Depth);
            SetParameter("Deviation", Deviation);
            SetParameter("Backstep", Backstep);
            SetParameter("PointSize", PointSize);
        }

        protected override void GetOutput()
        {
            PutOutputToBuffer("Zigzag", 0);
        }
    }
}
