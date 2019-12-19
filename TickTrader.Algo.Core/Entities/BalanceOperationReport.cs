using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BalanceOperationReport
    {
        public BalanceOperationReport(double balance, string currency, double amount, BalanceOperationType type)
        {
            Balance = balance;
            CurrencyCode = currency;
            Amount = amount;
            Type = type;
        }

        public double Balance { get; set; }
        public string CurrencyCode { get; set; }
        public double Amount { get; set; }
        public BalanceOperationType Type { get; set; }
    }

    public class BalanceDividendEventArgsImpl : IBalanceDividendEventArgs
    {
        public double Balance { get; set; }

        public string Symbol { get; set; }

        public double Amount { get; set; }

        public BalanceDividendEventArgsImpl(BalanceOperationReport report)
        {
            Balance = report.Balance;
            Symbol = report.CurrencyCode;
            Amount = report.Amount;
        }
    }

    public enum BalanceOperationType
    {
        DepositWithdrawal = 1,
        Dividend,
    }
}
