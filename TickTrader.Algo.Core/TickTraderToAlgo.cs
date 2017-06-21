using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public static class TickTraderToAlgo
    {
        public static BO.AccountingTypes Convert(AccountTypes type)
        {
            switch (type)
            {
                case AccountTypes.Cash: return BO.AccountingTypes.Cash;
                case AccountTypes.Gross: return BO.AccountingTypes.Gross;
                case AccountTypes.Net: return BO.AccountingTypes.Net;
            }
            throw new NotImplementedException("Unsupported account type: " + type);
        }
    }
}
