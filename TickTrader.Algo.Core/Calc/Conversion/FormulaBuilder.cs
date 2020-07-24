using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Calc.Conversion
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
                return new GetBid() { SrcSymbol = tracker };
            else
                return new GetAsk() { SrcSymbol = tracker };
        }

        public static IConversionFormula InverseConversion(SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new GetInvertedBid() { SrcSymbol = tracker };
            else
                return new GetInvertedAsk() { SrcSymbol = tracker };
        }

        public static IConversionFormula Then(this IConversionFormula formula, SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new MultByBid() { SrcSymbol = tracker, SrcFromula = formula };
            else
                return new MultByAsk() { SrcSymbol = tracker, SrcFromula = formula };
        }

        public static IConversionFormula ThenDivide(this IConversionFormula formula, SymbolMarketNode tracker, PriceType side)
        {
            if (side == PriceType.Bid)
                return new DivByBid() { SrcSymbol = tracker, SrcFromula = formula };
            else
                return new DivByAsk() { SrcSymbol = tracker, SrcFromula = formula };
        }

        public static IConversionFormula Error(ISymbolInfo symbol, string currency, string accountCurrency)
        {
            return new ConversionError(CalcErrorCodes.NoCrossSymbol);
        }
    }
}
