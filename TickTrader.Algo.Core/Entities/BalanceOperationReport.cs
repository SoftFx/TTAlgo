using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BalanceOperationReport
    {
        public BalanceOperationReport(double balance, string currency)
        {
            Balance = balance;
            CurrencyCode = currency;
        }

        public double Balance { get; set; }
        public string CurrencyCode { get; set; }
    }
}
