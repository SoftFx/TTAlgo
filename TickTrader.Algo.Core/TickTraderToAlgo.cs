using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public static class TickTraderToAlgo
    {
        //public static BusinessObjects.AccountingTypes Convert(AccountTypes type)
        //{
        //    switch (type)
        //    {
        //        case AccountTypes.Cash: return BusinessObjects.AccountingTypes.Cash;
        //        case AccountTypes.Gross: return BusinessObjects.AccountingTypes.Gross;
        //        case AccountTypes.Net: return BusinessObjects.AccountingTypes.Net;
        //    }
        //    throw new NotImplementedException("Unsupported account type: " + type);
        //}

        //public static BusinessLogic.AssetChangeTypes Convert(AssetChangeType cType)
        //{
        //    switch (cType)
        //    {
        //        case AssetChangeType.Added: return BusinessLogic.AssetChangeTypes.Added;
        //        case AssetChangeType.Updated: return BusinessLogic.AssetChangeTypes.Replaced;
        //        case AssetChangeType.Removed: return BusinessLogic.AssetChangeTypes.Removed;
        //    }
        //    throw new NotImplementedException("Unsupported change type: " + cType);
        //}

        public static float CmsValue(this SymbolInfo symbol)
        {
            return (float)symbol.Commission.Commission;
        }

        public static float CmsValueBookOrders(this SymbolInfo symbol)
        {
            return (float)symbol.Commission.LimitsCommission;
        }

        //public static BO.CommissionValueType CmsValueType(this SymbolAccessor symbol)
        //{
        //    switch (symbol.CommissionType)
        //    {
        //        case CommissionType.Percent: return BO.CommissionValueType.Percentage;
        //        case CommissionType.PerUnit: return BO.CommissionValueType.Points;
        //        case CommissionType.Absolute: return BO.CommissionValueType.Money;
        //    }

        //    throw new InvalidOperationException("Unsupported commission type: " + symbol.Commission);
        //}

        //public static BO.CommissionChargeType CmsChType(this SymbolAccessor symbol)
        //{
        //    switch (symbol.CommissionChargeType)
        //    {
        //        case CommissionChargeType.PerLot: return BO.CommissionChargeType.PerLot;
        //        case CommissionChargeType.PerTrade: return BO.CommissionChargeType.PerDeal;
        //    }

        //    throw new InvalidOperationException("Unsupported commission type: " + symbol.Commission);
        //}

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
