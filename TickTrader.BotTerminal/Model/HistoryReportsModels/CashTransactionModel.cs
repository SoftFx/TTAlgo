using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    sealed class CashTransactionModel : BaseTransactionModel
    {
        public CashTransactionModel(TradeReportInfo transaction, SymbolInfo symbol, int profitDigits) : base(transaction, symbol, profitDigits)
        {
            ProfitDigits = symbol?.ProfitDigits ?? -1;

            if (GetTransactionSide(transaction) == TransactionSide.Buy)
            {
                AssetI = transaction.DstAssetMovement ?? 0;
                AssetICurrency = transaction.DstAssetCurrency;

                AssetII = transaction.SrcAssetMovement ?? 0;
                AssetIICurrency = transaction.SrcAssetCurrency;
            }
            else if (GetTransactionSide(transaction) == TransactionSide.Sell)
            {
                AssetII = transaction.DstAssetMovement ?? 0;
                AssetIICurrency = transaction.DstAssetCurrency;

                AssetI = transaction.SrcAssetMovement ?? 0;
                AssetICurrency = transaction.SrcAssetCurrency;
            }
            else if (IsBalanceTransaction)
            {
                AssetI = transaction.TransactionAmount / LotSize;
                AssetICurrency = transaction.TransactionCurrency;

                AssetII = 0;
                AssetIICurrency = "";
            }

            UpdateFieldsAfterSplit(transaction);
        }

        public double AssetI { get; private set; }
        public double AssetII { get; private set; }
        public string AssetICurrency { get; set; }
        public string AssetIICurrency { get; set; }

        protected override string GetCommissionCurrency(TradeReportInfo transaction)
        {
            switch (GetTransactionSide(transaction))
            {
                case TransactionSide.Sell:
                case TransactionSide.Buy:
                    return transaction.CommissionCurrency ?? transaction.DstAssetCurrency;
                case TransactionSide.None: return "";
                default: throw new NotSupportedException(GetTransactionSide(transaction).ToString());
            }
        }

        protected override double? GetOpenPrice(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
            {
                if (transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.Stop || transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.StopLimit)
                    return transaction.StopPrice;
            }

            return transaction.Price;
        }

        protected override double? GetClosePrice(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.OrderFillPrice;
        }

        protected override double? GetCloseQuantity(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.OrderLastFillAmount / LotSize;
        }

        protected override double? GetOpenQuntity(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? transaction.TransactionAmount / LotSize : base.GetOpenQuntity(transaction);
        }

        protected override void UpdateFieldsAfterSplit(TradeReportInfo transaction)
        {
            if (transaction.TransactionReason != TradeReportInfo.Types.Reason.Split)
                return;

            ClosePrice = OpenPrice;
            CloseQuantity = OpenQuantity;
            OpenPrice = null;
            OpenQuantity = null;
        }
    }
}
