using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended.Reports;
using SoftFX.Extended;

namespace TickTrader.BotTerminal
{
    abstract class TransactionReport
    {
        public enum AggregatedTransactionType { Unknown, Buy, BuyLimit, BuyStop, Deposit, Sell, SellLimit, SellStop, Withdrawal, BuyStopLimit, SellStopLimit }
        public enum TransactionSide { None = -1, Buy, Sell }

        public TransactionReport() { }
        public TransactionReport(TradeTransactionReport transaction, SymbolModel symbol)
        {
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;

            IsPosition = transaction.TradeRecordType == TradeRecordType.Position;
            IsMarket = transaction.TradeRecordType == TradeRecordType.Market;
            IsPending = transaction.TradeRecordType == TradeRecordType.Limit
                || transaction.TradeRecordType == TradeRecordType.Stop
                || transaction.TradeRecordType == TradeRecordType.StopLimit;
            IsBalanceTransaction = transaction.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

            OrderId = GetId(transaction);
            OpenTime = GetOpenTime(transaction);
            Type = GetTransactionType(transaction);
            Side = GetTransactionSide(transaction);
            ActionType = transaction.TradeTransactionReportType;
            Symbol = GetSymbolOrCurrency(transaction);
            OpenQuantity = GetOpenQuntity(transaction);
            OpenPrice = GetOpenPrice(transaction);
            CloseTime = GetCloseTime(transaction);
            CloseQuantity = GetCloseQuantity(transaction);
            ClosePrice = GetClosePrice(transaction);
            RemainingQuantity = GetRemainingQuntity(transaction);
            Swap = GetSwap(transaction);
            Commission = GetCommission(transaction);
            CommissionCurrency = GetCommissionCurrency(transaction);
            Comment = transaction.Comment;
            Balance = transaction.AccountBalance;
            NetProfitLoss = GetNetProfiLoss(transaction);
            GrossProfitLoss = GetGrossProfitLoss(transaction);
            StopLoss = GetStopLoss(transaction);
            TakeProfit = GetTakeProfit(transaction);
            UniqueId = GetUniqueId(transaction);
            MaxVisibleVolume = GetMaxVisibleVolume(transaction);
        }

        private double? GetMaxVisibleVolume(TradeTransactionReport transaction)
        {
            return transaction.MaxVisibleQuantity;
        }

        public string UniqueId { get; protected set; }
        public string OrderId { get; protected set; }
        public DateTime OpenTime { get; protected set; }
        public AggregatedTransactionType Type { get; protected set; }
        public TransactionSide Side { get; protected set; }
        public TradeTransactionReportType ActionType { get; protected set; }
        public string Symbol { get; protected set; }
        public double OpenQuantity { get; protected set; }
        public double OpenPrice { get; protected set; }
        public double StopLoss { get; protected set; }
        public double TakeProfit { get; protected set; }
        public DateTime CloseTime { get; protected set; }
        public double CloseQuantity { get; protected set; }
        public double ClosePrice { get; protected set; }
        public double RemainingQuantity { get; protected set; }
        public double Commission { get; protected set; }
        public string CommissionCurrency { get; protected set; }
        public double Swap { get; protected set; }
        public double Balance { get; protected set; }
        public string Comment { get; protected set; }
        public int PriceDigits { get; protected set; }
        public int ProfitDigits { get; protected set; }
        public double GrossProfitLoss { get; protected set; }
        public double NetProfitLoss { get; protected set; }
        public bool IsPosition { get; protected set; }
        public bool IsMarket { get; protected set; }
        public bool IsPending { get; protected set; }
        public bool IsBalanceTransaction { get; protected set; }
        public double? MaxVisibleVolume { get; protected set; }

        protected virtual AggregatedTransactionType GetTransactionType(TradeTransactionReport transaction)
        {
            if (transaction.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction)
                return transaction.TransactionAmount > 0 ? AggregatedTransactionType.Deposit : AggregatedTransactionType.Withdrawal;

            switch (transaction.TradeRecordType)
            {
                case TradeRecordType.Market:
                case TradeRecordType.Position:
                    return transaction.TradeRecordSide == TradeRecordSide.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
                case TradeRecordType.Limit:
                    return transaction.TradeRecordSide == TradeRecordSide.Buy ? AggregatedTransactionType.BuyLimit : AggregatedTransactionType.SellLimit;
                case TradeRecordType.StopLimit:
                    return transaction.TradeRecordSide == TradeRecordSide.Buy ? AggregatedTransactionType.BuyStopLimit : AggregatedTransactionType.SellStopLimit;
                case TradeRecordType.Stop:
                    return transaction.TradeRecordSide == TradeRecordSide.Buy ? AggregatedTransactionType.BuyStop : AggregatedTransactionType.SellStop;
                default: return AggregatedTransactionType.Unknown;
            }
        }
        protected virtual string GetSymbolOrCurrency(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? transaction.TransactionCurrency : transaction.Symbol;
        }
        protected virtual DateTime? CheckIsNull(DateTime dateTime)
        {
            if (dateTime.Year == 1970 && dateTime.Month == 1 && dateTime.Day == 1)
                return null;

            return dateTime;
        }
        protected virtual TransactionSide GetTransactionSide(TradeTransactionReport transaction)
        {
            switch (transaction.TradeRecordSide)
            {
                case TradeRecordSide.Buy: return TransactionSide.Buy;
                case TradeRecordSide.Sell: return TransactionSide.Sell;
                default: return TransactionSide.None;
            }
        }
        protected virtual string GetUniqueId(TradeTransactionReport transaction)
        {
            return transaction.Id;
        }
        protected virtual string GetId(TradeTransactionReport transaction)
        {
            return transaction.Id;
        }
        protected virtual string GetCommissionCurrency(TradeTransactionReport transaction)
        {
            return transaction.TransactionCurrency;
        }
        protected virtual double GetRemainingQuntity(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.LeavesQuantity;
        }
        protected virtual double GetClosePrice(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.PositionClosePrice;
        }
        protected virtual double GetCloseQuantity(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.PositionLastQuantity;
        }
        protected virtual DateTime GetCloseTime(TradeTransactionReport transaction)
        {
            return transaction.TransactionTime;
        }
        protected virtual double GetOpenPrice(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.Price;
        }
        protected virtual double GetOpenQuntity(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.Quantity;
        }
        protected virtual DateTime GetOpenTime(TradeTransactionReport transaction)
        {
            return transaction.OrderCreated;
        }
        protected virtual double GetNetProfiLoss(TradeTransactionReport transaction)
        {
            return transaction.TransactionAmount;
        }
        protected virtual double GetGrossProfitLoss(TradeTransactionReport transaction)
        {
            return transaction.TransactionAmount - transaction.Swap - transaction.Commission;
        }
        protected virtual double GetSwap(TradeTransactionReport transaction)
        {
            return transaction.Swap;
        }
        protected virtual double GetCommission(TradeTransactionReport transaction)
        {
            return transaction.Commission;
        }
        protected virtual double GetStopLoss(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.StopLoss;
        }
        protected virtual double GetTakeProfit(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.TakeProfit;
        }
    }

    class NetTransactionModel : TransactionReport
    {
        public NetTransactionModel(TradeTransactionReport transaction, SymbolModel model) : base(transaction, model) { }

        protected override string GetUniqueId(TradeTransactionReport transaction)
        {
            return (transaction.TradeRecordType == TradeRecordType.Limit || transaction.TradeRecordType == TradeRecordType.Stop)
                && (transaction.ActionId > 1
                    || (GetRemainingQuntity(transaction) < GetOpenQuntity(transaction) && GetRemainingQuntity(transaction) > 0))
                ? $"{GetId(transaction)} #{transaction.ActionId}" : GetId(transaction);
        }
        protected override double GetOpenPrice(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ?
                double.NaN : transaction.TradeRecordType == TradeRecordType.Stop || transaction.TradeRecordType == TradeRecordType.StopLimit ?
                transaction.StopPrice : transaction.Price;
        }
    }

    class GrossTransactionModel : TransactionReport
    {
        public GrossTransactionModel(TradeTransactionReport transaction, SymbolModel symbol) : base(transaction, symbol)
        {
            PositionId = GetPositionId(transaction);
        }

        public string PositionId { get; private set; }

        private string GetPositionId(TradeTransactionReport transaction)
        {
            return IsPosition
                && GetOpenQuntity(transaction) != GetCloseQuantity(transaction)
                && GetCloseQuantity(transaction) > 0
                ? $"{transaction.PositionId} #{transaction.ActionId}" : transaction.PositionId;
        }
        protected override string GetUniqueId(TradeTransactionReport transaction)
        {
            return $"{GetId(transaction)} {GetPositionId(transaction)} {transaction.ActionId}";
        }
        protected override DateTime GetOpenTime(TradeTransactionReport transaction)
        {
            return IsPosition ? transaction.PositionOpened : transaction.OrderCreated;
        }
        protected override double GetOpenQuntity(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : IsPosition ? transaction.PositionQuantity : transaction.Quantity;
        }
        protected override double GetOpenPrice(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN :
                transaction.TradeRecordType == TradeRecordType.Stop || transaction.TradeRecordType == TradeRecordType.StopLimit ? transaction.StopPrice :
                IsPosition ? transaction.PosOpenPrice : transaction.Price;
        }
        protected override double GetRemainingQuntity(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : IsPosition ? transaction.PositionLeavesQuantity : transaction.LeavesQuantity;
        }
    }

    class CashTransactionModel : TransactionReport
    {
        public CashTransactionModel(TradeTransactionReport transaction, SymbolModel symbol) : base(transaction, symbol)
        {
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
                AssetI = transaction.TransactionAmount;
                AssetICurrency = transaction.TransactionCurrency;

                AssetII = 0;
                AssetIICurrency = "";
            }
        }

        public double AssetI { get; private set; }
        public double AssetII { get; private set; }
        public string AssetICurrency { get; set; }
        public string AssetIICurrency { get; set; }

        protected override string GetCommissionCurrency(TradeTransactionReport transaction)
        {
            switch (GetTransactionSide(transaction))
            {
                case TransactionSide.Sell: return transaction.DstAssetCurrency;
                case TransactionSide.Buy: return transaction.SrcAssetCurrency;
                case TransactionSide.None: return "";
                default: throw new NotSupportedException(GetTransactionSide(transaction).ToString());
            }
        }
        protected override double GetClosePrice(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.OrderFillPrice ?? 0;
        }
        protected override double GetCloseQuantity(TradeTransactionReport transaction)
        {
            return IsBalanceTransaction ? double.NaN : transaction.OrderLastFillAmount ?? 0;
        }
        protected override string GetUniqueId(TradeTransactionReport transaction)
        {
            return transaction.ActionId > 1 || GetCloseQuantity(transaction) < GetOpenQuntity(transaction) && GetCloseQuantity(transaction) > 0 ?
                $"{GetId(transaction)} #{transaction.ActionId}" : GetId(transaction);
        }
    }
}
