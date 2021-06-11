using System;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public static class TestRateGenerator
    {
        private const int GeneratorSeed = 42;

        private static readonly Random _generator = new(GeneratorSeed);

        public static double Rate => _generator.NextDoubleInRange(0.1, 2);


        public static SymbolInfo BuildNewQuote(this SymbolInfo symbol)
        {
            var ask = Rate;
            var bid = Rate;

            if (ask.Lt(bid))
            {
                var t = ask;
                ask = bid;
                bid = t;
            }

            return BuildQuote(symbol, bid, ask);
        }

        public static SymbolInfo BuildNullQuote(this SymbolInfo symbol) => BuildQuote(symbol, null, null);

        public static SymbolInfo BuildZeroQuote(this SymbolInfo symbol) => BuildQuote(symbol, 0.0, 0.0);

        public static SymbolInfo BuildOneSideBidQuote(this SymbolInfo symbol) => BuildQuote(symbol, Rate, null);

        public static SymbolInfo BuildOneSideAskQuote(this SymbolInfo symbol) => BuildQuote(symbol, null, Rate);


        private static SymbolInfo BuildQuote(SymbolInfo symbol, double? bid, double? ask)
        {
            symbol.UpdateRate(new QuoteInfo(symbol.Name, new QuoteData(DateTime.Now, bid, ask)));

            return symbol;
        }
    }
}
