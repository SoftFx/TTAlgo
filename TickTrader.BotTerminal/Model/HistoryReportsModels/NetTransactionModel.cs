using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class NetTransactionModel : BaseTransactionModel
    {
        public NetTransactionModel(TradeReportInfo transaction, ISymbolInfo model, int profitDigits) : base(transaction, model, profitDigits) { }

        protected override double? GetOpenPrice(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
            {
                if (transaction.OrderType == OrderInfo.Types.Type.Stop || transaction.OrderType == OrderInfo.Types.Type.StopLimit)
                    return transaction.StopPrice;
                else
                    return transaction.PositionRemainingPrice ?? transaction.Price;
            }

            if (transaction.OrderType == OrderInfo.Types.Type.Stop)
                return transaction.OrderFillPrice;

            if (transaction.OrderType == OrderInfo.Types.Type.StopLimit)
                return transaction.StopPrice;

            return transaction.PositionOpenPrice == 0 ? transaction.Price : transaction.PositionOpenPrice;
        }
    }
}
