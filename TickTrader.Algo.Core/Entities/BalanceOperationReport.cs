using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BalanceOperationReport
    {
        public BalanceOperationReport(double balance, string currency, double amount)
        {
            Balance = balance;
            CurrencyCode = currency;
            Amount = amount;
        }

        public double Balance { get; set; }
        public string CurrencyCode { get; set; }
        public double Amount { get; set; }
    }
}
