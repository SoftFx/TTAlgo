using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class GrossTransactionModel : BaseTransactionModel
    {
        public GrossTransactionModel(TradeReportInfo transaction, SymbolInfo symbol, int profitDigits) : base(transaction, symbol, profitDigits)
        {
            if (transaction.PositionId != null)
            {
                UniqueId = GetUniqueId(transaction, transaction.PositionId, out _);

                if (!transaction.IsEmulated) //from Backtester grids OrderId is invalid
                    ParentOrderId = OrderId;
            }

            if (UniqueId.ActionNo != null && !transaction.IsEmulated) //from Backtester grids OrderId is invalid
                ParentOrderId = OrderId;
        }

        public string ParentOrderId { get; private set; }

        protected override double? GetOpenQuntity(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
                return GetSplitReqVolume(CloseQuantity);

            return (IsPosition ? transaction.PositionQuantity : transaction.Quantity) / LotSize;
        }

        protected override double? GetOpenPrice(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
                return GetSplitReqPrice(ClosePrice);

            if (IsPosition)
                return transaction.PositionOpenPrice;

            return transaction.OrderType == OrderInfo.Types.Type.Stop || transaction.OrderType == OrderInfo.Types.Type.StopLimit ? transaction.StopPrice : transaction.Price;
        }

        protected override double? GetRemainingQuantity(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
                return transaction.RemainingQuantity / LotSize;

            return (IsPosition ? transaction.PositionLeavesQuantity : transaction.RemainingQuantity) / LotSize;
        }

        protected override double? GetClosePrice(TradeReportInfo transaction) => IsSplitTransaction ? transaction.OpenPrice : base.GetClosePrice(transaction);

        protected override double? GetCloseQuantity(TradeReportInfo transaction) => IsSplitTransaction ? transaction.OpenQuantity / LotSize : base.GetCloseQuantity(transaction);
    }
}
