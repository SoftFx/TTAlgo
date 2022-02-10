using System;
using TickTrader.Algo.Core.Lib.Math;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public static class TestRateGenerator
    {
        private const int GeneratorSeed = 42;
        private const double MinPrice = 1.0;
        private const double MaxPrice = 3.0;

        private static readonly Random _generator = new(GeneratorSeed);

        public static double Rate => _generator.NextDoubleInRange(MinPrice, MaxPrice);


        public static ISymbolInfoWithRate BuildNewQuote(this ISymbolInfoWithRate symbol)
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

        public static ISymbolInfoWithRate BuildNullQuote(this ISymbolInfoWithRate symbol) => BuildQuote(symbol, null, null);

        public static ISymbolInfoWithRate BuildZeroQuote(this ISymbolInfoWithRate symbol) => BuildQuote(symbol, 0.0, 0.0);

        public static ISymbolInfoWithRate BuildOneSideBidQuote(this ISymbolInfoWithRate symbol) => BuildQuote(symbol, Rate, null);

        public static ISymbolInfoWithRate BuildOneSideAskQuote(this ISymbolInfoWithRate symbol) => BuildQuote(symbol, null, Rate);


        private static ISymbolInfoWithRate BuildQuote(ISymbolInfoWithRate symbol, double? bid, double? ask)
        {
            symbol.UpdateRate(new QuoteInfo(symbol.Name, new QuoteData(DateTime.Now, bid, ask)));

            return symbol;
        }
    }
}
