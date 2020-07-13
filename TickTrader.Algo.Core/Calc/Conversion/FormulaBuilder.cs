using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Common.Business;

namespace TickTrader.Algo.Core.Calc.Conversion
{
    internal static class FormulaBuilder
    {
        public static IConversionFormula Direct()
        {
            return new NoConvertion();
        }

        public static IConversionFormula Conversion(SymbolMarketNode tracker, FxPriceType side)
        {
            if (side == FxPriceType.Bid)
                return new GetBid() { SrcSymbol = tracker };
            else 
                return new GetAsk() { SrcSymbol = tracker };
        }

        public static IConversionFormula InverseConversion(SymbolMarketNode tracker, FxPriceType side)
        {
            if (side == FxPriceType.Bid)
                return new GetInvertedBid() { SrcSymbol = tracker };
            else
                return new GetInvertedAsk() { SrcSymbol = tracker };
        }

        public static IConversionFormula Then(this IConversionFormula formula, SymbolMarketNode tracker, FxPriceType side)
        {
            if (side == FxPriceType.Bid)
                return new MultByBid() { SrcSymbol = tracker, SrcFromula = formula };
            else
                return new MultByAsk() { SrcSymbol = tracker, SrcFromula = formula };
        }

        public static IConversionFormula ThenDivide(this IConversionFormula formula, SymbolMarketNode tracker, FxPriceType side)
        {
            if (side == FxPriceType.Bid)
                return new DivByBid() { SrcSymbol = tracker, SrcFromula = formula };
            else
                return new DivByAsk() { SrcSymbol = tracker, SrcFromula = formula };
        }

        public static IConversionFormula Error(ISymbolInfo2 symbol, string currency, string accountCurrency)
        {
            return new ConversionError(CalcErrorCodes.NoCrossSymbol);
        }
    }
}
