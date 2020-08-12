using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
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
}
