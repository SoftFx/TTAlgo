using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Tests.TestCases;

namespace TickTrader.Algo.Indicators.Tests.BillWilliamsTests.Fractals
{
    public class FractalsTestCase : SimpleTestCase
    {
        public FractalsTestCase(Type indicatorType, string symbol, string quotesPath, string answerPath)
            : base(indicatorType, symbol, quotesPath, answerPath, 16)
        {
        }

        protected override void SetupParameters() { }

        protected override void GetOutput()
        {
            PutOutputToBuffer<Marker>("FractalsUp", 0, ParseMarker);
            PutOutputToBuffer<Marker>("FractalsDown", 1, ParseMarker);
        }

        private const string upPrefix = "Fractal Up";
        private const string downPrefix = "Fractal Down";

        private static double ParseMarker(Marker marker)
        {
            var text = marker?.DisplayText;

            if (string.IsNullOrEmpty(text))
                return double.NaN;

            if (text.StartsWith(upPrefix))
                return double.Parse(text.Substring(upPrefix.Length));

            if (text.StartsWith(downPrefix))
                return double.Parse(text.Substring(downPrefix.Length));

            throw new Exception("Cannot parse fractal output: " + text);

        }
    }
}
