using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    internal static class FormulaBuilder
    {
        public static IConversionFormula Direct()
        {
            return new NoConvertion();
        }

        public static IConversionFormula Conversion(SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new GetBid(tracker);
            else
                return new GetAsk(tracker);
        }

        public static IConversionFormula InverseConversion(SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new GetInvertedBid(tracker);
            else
                return new GetInvertedAsk(tracker);
        }

        public static IConversionFormula Then(this IConversionFormula formula, SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new MultByBid(tracker, formula);
            else
                return new MultByAsk(tracker, formula);
        }

        public static IConversionFormula ThenDivide(this IConversionFormula formula, SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new DivByBid(tracker, formula);
            else
                return new DivByAsk(tracker, formula);
        }

        public static IConversionFormula Error(ISymbolInfo symbol, string currency, string accountCurrency)
        {
            return new ConversionError(CalcErrorCodes.NoCrossSymbol);
        }
    }
}
