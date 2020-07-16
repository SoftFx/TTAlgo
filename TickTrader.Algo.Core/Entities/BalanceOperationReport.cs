using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    //[Serializable]
    //public class BalanceOperationReport
    //{
    //    public BalanceOperationReport(double balance, string currency, double amount, BalanceOperationType type)
    //    {
    //        Balance = balance;
    //        Currency = currency;
    //        TransactionAmount = amount;
    //        Type = type;
    //    }

    //    public double Balance { get; set; }
    //    public string Currency { get; set; }
    //    public double TransactionAmount { get; set; }
    //    public BalanceOperationType Type { get; set; }
    //}

    public class BalanceDividendEventArgsImpl : IBalanceDividendEventArgs
    {
        public double Balance { get; set; }

        public string Currency { get; set; }

        public double TransactionAmount { get; set; }

        public BalanceDividendEventArgsImpl(Domain.BalanceOperation report)
        {
            Balance = report.Balance;
            Currency = report.Currency;
            TransactionAmount = report.TransactionAmount;
        }
    }

    //public enum BalanceOperationType
    //{
    //    DepositWithdrawal = 1,
    //    Dividend,
    //}
}
