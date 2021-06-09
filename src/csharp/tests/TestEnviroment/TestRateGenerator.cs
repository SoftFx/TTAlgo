using System;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Core.Lib.Math;

namespace TestEnviroment
{
    public sealed class TestRateGenerator
    {
        private const int GeneratorSeed = 42;

        private readonly Random _generator;


        public static TestRateGenerator Instance { get; } = new TestRateGenerator();

        private TestRateGenerator() 
        {
            _generator = new Random(GeneratorSeed);
        }

        internal QuoteInfo BuildNewQuote(string symbol)
        {
            var ask = _generator.NextDoubleInRange(0.1, 2);
            var bid = _generator.NextDoubleInRange(0.1, 2);

            if (ask.Lt(bid))
            {
                var t = ask;
                ask = bid;
                bid = t;
            }

            return new QuoteInfo(symbol, new QuoteData(DateTime.UtcNow, bid, ask));
        }
    }
}
