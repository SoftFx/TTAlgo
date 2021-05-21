using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public static class CommissionExtensions
    {
        public static float CmsValue(this SymbolInfo symbol)
        {
            return (float)symbol.Commission.Commission;
        }

        public static float CmsValueBookOrders(this SymbolInfo symbol)
        {
            return (float)symbol.Commission.LimitsCommission;
        }

        public static bool IsReducedOpenCommission(this OrderAccessor position)
        {
            return false;
        }

        public static bool IsReducedCloseCommission(this OrderAccessor position)
        {
            return false;
        }
    }
}
