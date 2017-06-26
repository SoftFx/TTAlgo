using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class TickTraderToAlgo
    {
        public static BusinessObjects.AccountingTypes Convert(AccountTypes type)
        {
            switch (type)
            {
                case AccountTypes.Cash: return BusinessObjects.AccountingTypes.Cash;
                case AccountTypes.Gross: return BusinessObjects.AccountingTypes.Gross;
                case AccountTypes.Net: return BusinessObjects.AccountingTypes.Net;
            }
            throw new NotImplementedException("Unsupported account type: " + type);
        }

        public static BusinessLogic.AssetChangeTypes Convert(AssetChangeType cType)
        {
            switch (cType)
            {
                case AssetChangeType.Added: return BusinessLogic.AssetChangeTypes.Added;
                case AssetChangeType.Updated: return BusinessLogic.AssetChangeTypes.Replaced;
                case AssetChangeType.Removed: return BusinessLogic.AssetChangeTypes.Removed;
            }
            throw new NotImplementedException("Unsupported change type: " + cType);
        }
    }
}
