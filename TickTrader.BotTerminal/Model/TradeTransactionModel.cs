using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    abstract class TransactionReport
    {
        public enum AggregatedTransactionType
        {
            Unknown, Buy, BuyLimit, BuyStop, Deposit, Sell, SellLimit, SellStop, Withdrawal, BuyStopLimit, SellStopLimit, SellStopLimitCanceled,
            SellStopCanceled, SellLimitCanceled, BuyStopLimitCanceled, BuyStopCanceled, BuyLimitCanceled, TransferFunds, SplitBuy, SplitSell, Split, Dividend
        }

        public enum TransactionSide { None = -1, Buy, Sell }

        public enum Reasons { None = -1, DealerDecision, StopOut, Activated, CanceledByDealer, Expired, OcoRelatedOrder }

        public TransactionReport() { }

        public TransactionReport(TradeReportEntity transaction, SymbolModel symbol, int profitDigits)
        {
            PriceDigits = symbol?.PriceDigits ?? -1;
            ProfitDigits = profitDigits;
            LotSize = symbol?.LotSize ?? 1;

            IsPosition = transaction.TradeRecordType == OrderType.Position;
            IsMarket = transaction.TradeRecordType == OrderType.Market;
            IsPending = transaction.TradeRecordType == OrderType.Limit || transaction.TradeRecordType == OrderType.Stop || transaction.TradeRecordType == OrderType.StopLimit;
            IsBalanceTransaction = transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction;

            OrderId = GetId(transaction);
            PositionId = GetPositionId(transaction);
            ActionId = transaction.ActionId;
            OpenTime = GetOpenTime(transaction);
            Type = GetTransactionType(transaction);

            if (IsSplitTransaction)
                SplitRatio = GetSplitRatio(transaction);

            Side = GetTransactionSide(transaction);
            ActionType = transaction.TradeTransactionReportType;
            Symbol = GetSymbolOrCurrency(transaction);
            CloseTime = GetCloseTime(transaction);
            CloseQuantity = GetCloseQuantity(transaction);
            ClosePrice = GetClosePrice(transaction);
            OpenQuantity = GetOpenQuntity(transaction);
            OpenPrice = GetOpenPrice(transaction);
            RemainingQuantity = GetRemainingQuantity(transaction);
            Swap = GetSwap(transaction);
            Commission = GetCommission(transaction);
            CommissionCurrency = GetCommissionCurrency(transaction);
            Comment = transaction.Comment;
            Balance = transaction.AccountBalance;
            NetProfitLoss = GetNetProfiLoss(transaction);
            GrossProfitLoss = GetGrossProfitLoss(transaction);
            StopLoss = GetStopLoss(transaction);
            TakeProfit = GetTakeProfit(transaction);
            MaxVisibleVolume = GetMaxVisibleVolume(transaction);
            ReqQuantity = GetReqQuantity(transaction);
            PosRemainingPrice = GetPosRemainingPrice(transaction);
            OrderExecutionOption = GetOrderExecutionOption(transaction);
            InitialType = GetInitialOrderType(transaction);
            Reason = GetReason(transaction);
            Volume = GetVolume(transaction);
            Tag = GetTag(transaction);
            InstanceId = GetInstanceId(transaction);
            PosQuantity = GetPosQuantity(transaction);
            Fees = GetFees(transaction);
            Taxes = GetTaxes(transaction);
            ReqOpenPrice = GetReqOpenPrice(transaction);
            ReqClosePrice = GetReqClosePrice(transaction);
            ReqOpenVolume = GetReqOpenVolume(transaction);
            ReqCloseVolume = GetReqCloseVolume(transaction);

            ReqSlippage = GetReqSlippage(transaction);
            OpenSlippage = GetOpenSlippage(transaction);
            CloseSlippage = GetCloseSlippage(transaction);

            OCORelatedOrderId = GetOCORelatedOrderId(transaction);

            // should be last (it's based on other fields)
            UniqueId = GetUniqueId(transaction, transaction.OrderId, out long orderNum);
            OrderNum = orderNum;
            OpenSortedNumber = GetOpenSortedNumber();
            CloseSortedNumber = GetCloseSortedNumber();

            if (IsSplitTransaction && !IsBalanceTransaction)
            {
                SplitReqPrice = GetSplitReqPrice(OpenPrice);
                SplitReqVolume = GetSplitReqVolume(Volume);
            }
        }

        public static TransactionReport Create(AccountTypes accountType, TradeReportEntity tTransaction, int balanceDigits, SymbolModel symbol = null)
        {
            switch (accountType)
            {
                case AccountTypes.Gross: return new GrossTransactionModel(tTransaction, symbol, balanceDigits);
                case AccountTypes.Net: return new NetTransactionModel(tTransaction, symbol, balanceDigits);
                case AccountTypes.Cash: return new CashTransactionModel(tTransaction, symbol, balanceDigits);
                default: throw new NotSupportedException(accountType.ToString());
            }
        }

        private double? GetMaxVisibleVolume(TradeReportEntity transaction)
        {
            return transaction.MaxVisibleQuantity / LotSize;
        }

        public TradeReportKey UniqueId { get; protected set; }
        public string OrderId { get; protected set; }
        public string PositionId { get; protected set; }
        public int ActionId { get; }
        public long OrderNum { get; }
        public DateTime OpenTime { get; protected set; }
        public AggregatedTransactionType Type { get; protected set; }
        public TransactionSide Side { get; protected set; }
        public TradeExecActions ActionType { get; protected set; }
        public string Symbol { get; protected set; }
        public double? OpenQuantity { get; protected set; }
        public double? OpenPrice { get; protected set; }
        public double? StopLoss { get; protected set; }
        public double? TakeProfit { get; protected set; }
        public DateTime CloseTime { get; protected set; }
        public double? CloseQuantity { get; protected set; }
        public double? ClosePrice { get; protected set; }
        public double? RemainingQuantity { get; protected set; }
        public double? Commission { get; protected set; }
        public string CommissionCurrency { get; protected set; }
        public double? Swap { get; protected set; }
        public double Balance { get; protected set; }
        public string Comment { get; protected set; }
        public int PriceDigits { get; protected set; }
        public int ProfitDigits { get; protected set; }
        public double? GrossProfitLoss { get; protected set; }
        public double NetProfitLoss { get; protected set; }
        public bool IsPosition { get; protected set; }
        public bool IsMarket { get; protected set; }
        public bool IsPending { get; protected set; }
        public bool IsBalanceTransaction { get; protected set; }
        public double? MaxVisibleVolume { get; protected set; }
        public double LotSize { get; }
        public double? Volume { get; protected set; }
        public double? ReqQuantity { get; protected set; }
        public double? PosRemainingPrice { get; protected set; }
        public string OrderExecutionOption { get; protected set; }
        public OrderType? InitialType { get; protected set; }
        public Reasons? Reason { get; protected set; }
        public string Tag { get; protected set; }
        public string InstanceId { get; protected set; }
        public double? PosQuantity { get; protected set; }
        public double? ReqOpenPrice { get; protected set; }
        public double? ReqOpenVolume { get; protected set; }
        public double? ReqClosePrice { get; protected set; }
        public double? ReqCloseVolume { get; protected set; }

        public double? OpenSlippage { get; protected set; }
        public double? CloseSlippage { get; protected set; }
        public double? ReqSlippage { get; protected set; }

        public string OpenSortedNumber { get; protected set; }
        public string CloseSortedNumber { get; protected set; }

        public bool IsSplitTransaction => Type == AggregatedTransactionType.SplitBuy || Type == AggregatedTransactionType.SplitSell || Type == AggregatedTransactionType.Split;
        public bool IsNotSplitTransaction => !IsSplitTransaction;
        public double? SplitReqVolume { get; protected set; }
        public double? SplitReqPrice { get; protected set; }
        public double? SplitRatio { get; protected set; }

        public bool IsDividendTransaction => Type == AggregatedTransactionType.Dividend;
        public double? Taxes { get; protected set; }
        public double? Fees { get; protected set; }
        public string OCORelatedOrderId { get; protected set; }

        protected virtual AggregatedTransactionType GetTransactionType(TradeReportEntity transaction)
        {
            if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction)
            {
                switch (transaction.TradeTransactionReason)
                {
                    case TradeTransactionReason.Dividend:
                        return AggregatedTransactionType.Dividend;

                    case TradeTransactionReason.TransferMoney:
                        return AggregatedTransactionType.TransferFunds;

                    case TradeTransactionReason.Split:
                        return AggregatedTransactionType.Split;

                    default:
                        return transaction.TransactionAmount > 0 ? AggregatedTransactionType.Deposit : AggregatedTransactionType.Withdrawal;
                }
            }

            if (transaction.TradeTransactionReportType == TradeExecActions.TradeModified)
            {
                if (transaction.TradeTransactionReason == TradeTransactionReason.Split)
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.SplitBuy : AggregatedTransactionType.SplitSell;
            }

            switch (transaction.TradeRecordType)
            {
                case OrderType.Market:
                case OrderType.Position:
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
                case OrderType.Limit:
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.BuyLimit : AggregatedTransactionType.SellLimit;
                case OrderType.StopLimit:
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.BuyStopLimit : AggregatedTransactionType.SellStopLimit;
                case OrderType.Stop:
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.BuyStop : AggregatedTransactionType.SellStop;
                default:
                    return AggregatedTransactionType.Unknown;
            }
        }

        protected virtual string GetSymbolOrCurrency(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? transaction.TransactionCurrency : transaction.Symbol;
        }

        protected virtual TransactionSide GetTransactionSide(TradeReportEntity transaction)
        {
            switch (transaction.TradeRecordSide)
            {
                case OrderSide.Buy: return TransactionSide.Buy;
                case OrderSide.Sell: return TransactionSide.Sell;
                default: return TransactionSide.None;
            }
        }

        protected virtual TradeReportKey GetUniqueId(TradeReportEntity transaction, string id, out long orderNum)
        {
            orderNum = long.Parse(id);

            if (transaction.ActionId > 1)
                return new TradeReportKey(orderNum, transaction.ActionId);

            if (ActionId == 1 && RemainingQuantity > 0 && !OrderWasCanceled() && Reason != Reasons.Activated)
                return new TradeReportKey(orderNum, transaction.ActionId);
            else
                return new TradeReportKey(orderNum, null);
        }

        protected virtual string GetId(TradeReportEntity transaction) => transaction.Id;

        protected virtual string GetPositionId(TradeReportEntity transaction) => transaction.PositionId;

        protected virtual string GetCommissionCurrency(TradeReportEntity transaction)
        {
            return transaction.CommCurrency ?? transaction.TransactionCurrency;
        }

        protected virtual double? GetRemainingQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : (transaction.LeavesQuantity / LotSize);
        }

        protected virtual double? GetPosQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : (transaction.PositionQuantity / LotSize);
        }

        protected virtual double? GetClosePrice(TradeReportEntity transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : transaction.PositionClosePrice;
        }

        protected virtual double? GetCloseQuantity(TradeReportEntity transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : (transaction.PositionLastQuantity / LotSize);
        }

        protected virtual DateTime GetCloseTime(TradeReportEntity transaction)
        {
            return transaction.TransactionTime;
        }

        protected virtual double? GetOpenPrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.Price;
        }

        protected virtual double? GetOpenQuntity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : (transaction.Quantity / LotSize);
        }

        protected virtual DateTime GetOpenTime(TradeReportEntity transaction)
        {
            return transaction.OpenTime;
        }

        protected virtual double GetNetProfiLoss(TradeReportEntity transaction)
        {
            return transaction.TransactionAmount;
        }

        protected virtual double? GetGrossProfitLoss(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : (double?)(transaction.TransactionAmount - transaction.Swap - transaction.Commission);
        }

        protected virtual double? GetSwap(TradeReportEntity transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : transaction.Swap;
        }

        protected virtual double? GetCommission(TradeReportEntity transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : transaction.Commission;
        }

        protected virtual double? GetStopLoss(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.StopLoss;
        }

        protected virtual double? GetTakeProfit(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.TakeProfit;
        }

        protected virtual double? GetVolume(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return transaction.TransactionAmount / LotSize;

            if (IsSplitTransaction)
                return (transaction.Quantity == 0 ? transaction.PositionQuantity : transaction.Quantity) / LotSize;

            return (OrderWasCanceled() ? transaction.RemainingQuantity : transaction.OrderLastFillAmount) / LotSize;
        }

        protected virtual double? GetReqQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : ((transaction.RemainingQuantity + transaction.OrderLastFillAmount) / LotSize);
        }

        protected virtual double? GetPosRemainingPrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : transaction.PosRemainingPrice;
        }

        protected virtual double? GetFees(TradeReportEntity transaction)
        {
            return IsDividendTransaction ? (double?)transaction.Commission : null;
        }

        protected virtual double? GetTaxes(TradeReportEntity transaction)
        {
            return IsDividendTransaction ? (double?)transaction.Tax : null;
        }

        protected virtual string GetOrderExecutionOption(TradeReportEntity transaction)
        {
            var options = new List<OrderOptions>();

            if (transaction.ImmediateOrCancel && !IsSplitTransaction)
            {
                switch (Type)
                {
                    case AggregatedTransactionType.BuyLimit:
                        Type = AggregatedTransactionType.Buy;
                        break;
                    case AggregatedTransactionType.SellLimit:
                        Type = AggregatedTransactionType.Sell;
                        break;
                    default:
                        break;
                }

                options.Add(OrderOptions.ImmediateOrCancel);
            }

            if (transaction.MarketWithSlippage)
                options.Add(OrderOptions.MarketWithSlippage);

            if (transaction.MaxVisibleQuantity >= 0)
                options.Add(OrderOptions.HiddenIceberg);

            if (transaction.OneCancelsTheOther)
                options.Add(OrderOptions.OneCancelsTheOther);

            return string.Join(",", options);
        }

        protected virtual OrderType? GetInitialOrderType(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : (OrderType?)transaction.ReqOrderType;
        }

        protected virtual double? GetReqOpenPrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : transaction.ReqOpenPrice;
        }

        protected virtual double? GetReqClosePrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : transaction.ReqClosePrice;
        }

        protected virtual double? GetReqOpenVolume(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : transaction.ReqOpenQuantity / LotSize;
        }

        protected virtual double? GetReqCloseVolume(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : transaction.ReqCloseQuantity / LotSize;
        }

        protected virtual double? GetOpenSlippage(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return GetTransactionSide(transaction) == TransactionSide.Buy ? OpenPrice - ReqOpenPrice : ReqOpenPrice - OpenPrice;
        }

        protected virtual double? GetCloseSlippage(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return GetTransactionSide(transaction) == TransactionSide.Buy ? ClosePrice - ReqClosePrice : ReqClosePrice - ClosePrice;
        }

        protected virtual double? GetReqSlippage(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return transaction.Slippage;
        }

        protected virtual Reasons? GetReason(TradeReportEntity transaction)
        {
            if (transaction.TradeTransactionReportType == TradeExecActions.OrderFilled)
                Type = GetBuyOrSellType(transaction);

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderFilled && transaction.TradeTransactionReason == TradeTransactionReason.DealerDecision)
                return Reasons.DealerDecision;

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderFilled && transaction.TradeTransactionReason == TradeTransactionReason.StopOut)
                return Reasons.StopOut;

            if (transaction.TradeTransactionReportType == TradeExecActions.PositionClosed && transaction.TradeTransactionReason == TradeTransactionReason.StopOut)
                return Reasons.StopOut;

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderActivated && transaction.TradeTransactionReason == TradeTransactionReason.DealerDecision &&
                transaction.ReqOrderType == OrderType.StopLimit)
            {
                Type = Type == AggregatedTransactionType.Sell ? AggregatedTransactionType.SellStopLimit : AggregatedTransactionType.BuyStopLimit;
                return Reasons.Activated;
            }

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderCanceled && transaction.TradeTransactionReason == TradeTransactionReason.ClientRequest)
                Type = GetCanceledType(transaction);

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderCanceled && transaction.TradeTransactionReason == TradeTransactionReason.DealerDecision)
            {
                Type = GetCanceledType(transaction);
                return Reasons.CanceledByDealer;
            }

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderCanceled && transaction.TradeTransactionReason == TradeTransactionReason.OneCancelsTheOther)
            {
                Type = GetCanceledType(transaction);
                return Reasons.OcoRelatedOrder;
            }

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderCanceled && transaction.TradeTransactionReason == TradeTransactionReason.StopOut)
            {
                Type = GetCanceledType(transaction);
                return Reasons.StopOut;
            }

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderExpired && transaction.TradeTransactionReason == TradeTransactionReason.Expired)
            {
                Type = GetCanceledType(transaction);
                return Reasons.Expired;
            }

            return null;
        }

        protected virtual void UpdateFieldsAfterSplit(TradeReportEntity transaction) { }

        protected AggregatedTransactionType GetBuyOrSellType(TradeReportEntity transaction)
        {
            return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
        }

        protected virtual string GetOCORelatedOrderId(TradeReportEntity transaction)
        {
            return transaction.OCORelativeOrderId;
        }

        protected AggregatedTransactionType GetCanceledType(TradeReportEntity transaction)
        {
            switch (transaction.TradeRecordType)
            {
                case OrderType.Market:
                case OrderType.Position:
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
                case OrderType.Limit:
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.BuyLimitCanceled : AggregatedTransactionType.SellLimitCanceled;
                case OrderType.StopLimit:
                    OpenPrice = transaction.StopPrice;
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.BuyStopLimitCanceled : AggregatedTransactionType.SellStopLimitCanceled;
                case OrderType.Stop:
                    OpenPrice = transaction.StopPrice;
                    return transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.BuyStopCanceled : AggregatedTransactionType.SellStopCanceled;
                default: return AggregatedTransactionType.Unknown;
            }
        }

        protected string GetTag(TradeReportEntity transaction) => transaction.Tag;

        protected string GetInstanceId(TradeReportEntity transaction) => transaction.InstanceId;

        protected virtual double? GetSplitRatio(TradeReportEntity transaction) => transaction.SplitRatio;

        protected virtual double? GetSplitReqPrice(double? price) => price * SplitRatio;

        protected virtual double? GetSplitReqVolume(double? volume) => volume / SplitRatio;


        protected string GetOpenSortedNumber() => $"{OpenTime.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{UniqueId}";

        protected string GetCloseSortedNumber() => $"{CloseTime.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{UniqueId}";

        private bool OrderWasCanceled() => Type.ToString().Contains("Canceled");
    }

    class NetTransactionModel : TransactionReport
    {
        public NetTransactionModel(TradeReportEntity transaction, SymbolModel model, int profitDigits) : base(transaction, model, profitDigits) { }

        protected override double? GetOpenPrice(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
            {
                if (transaction.TradeRecordType == OrderType.Stop || transaction.TradeRecordType == OrderType.StopLimit)
                    return transaction.StopPrice;
                else
                    return transaction.PosRemainingPrice ?? transaction.Price;
            }

            if (transaction.TradeRecordType == OrderType.Stop)
                return transaction.OrderFillPrice;

            if (transaction.TradeRecordType == OrderType.StopLimit)
                return transaction.StopPrice;

            return transaction.PosOpenPrice == 0 ? transaction.Price : transaction.PosOpenPrice;
        }

        //protected override AggregatedTransactionType GetTransactionType(TradeReportEntity transaction)
        //{
        //    var type = base.GetTransactionType(transaction);

        //    if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction)
        //    {
        //        if (transaction.TradeTransactionReason == TradeTransactionReason.Dividend)
        //            type = AggregatedTransactionType.Dividend;

        //        if (transaction.TradeTransactionReason == TradeTransactionReason.TransferMoney)
        //            type = AggregatedTransactionType.TransferFunds;
        //    }

        //    if (transaction.TradeTransactionReportType == TradeExecActions.TradeModified)
        //    {
        //        if (transaction.TradeTransactionReason == TradeTransactionReason.Split)
        //            type = transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.SplitBuy : AggregatedTransactionType.SplitSell;
        //    }

        //    return type;
        //}
    }

    class GrossTransactionModel : TransactionReport
    {
        public GrossTransactionModel(TradeReportEntity transaction, SymbolModel symbol, int profitDigits) : base(transaction, symbol, profitDigits)
        {
            if (transaction.PositionId != null)
            {
                UniqueId = GetUniqueId(transaction, transaction.PositionId, out _);

                if (!transaction.IsEmulatedEntity) //from Backtester grids OrderId is invalid
                    ParentOrderId = OrderId;
            }

            if (UniqueId.ActionNo != null && !transaction.IsEmulatedEntity) //from Backtester grids OrderId is invalid
                ParentOrderId = OrderId;
        }

        public string ParentOrderId { get; protected set; }

        protected override double? GetOpenQuntity(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
                return GetSplitReqVolume(CloseQuantity);

            return (IsPosition ? transaction.PositionQuantity : transaction.Quantity) / LotSize;
        }

        protected override double? GetOpenPrice(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
                return GetSplitReqPrice(ClosePrice);

            if (IsPosition)
                return transaction.PosOpenPrice;

            return transaction.TradeRecordType == OrderType.Stop || transaction.TradeRecordType == OrderType.StopLimit ? transaction.StopPrice : transaction.Price;
        }

        protected override double? GetRemainingQuantity(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
                return transaction.LeavesQuantity / LotSize;

            return (IsPosition ? transaction.PositionLeavesQuantity : transaction.LeavesQuantity) / LotSize;
        }

        protected override double? GetClosePrice(TradeReportEntity transaction) => IsSplitTransaction ? transaction.OpenPrice : base.GetClosePrice(transaction);

        protected override double? GetCloseQuantity(TradeReportEntity transaction) => IsSplitTransaction ? transaction.OpenQuantity / LotSize : base.GetCloseQuantity(transaction);
    }

    class CashTransactionModel : TransactionReport
    {
        public CashTransactionModel(TradeReportEntity transaction, SymbolModel symbol, int profitDigits) : base(transaction, symbol, profitDigits)
        {
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? -1;

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

        protected override string GetCommissionCurrency(TradeReportEntity transaction)
        {
            switch (GetTransactionSide(transaction))
            {
                case TransactionSide.Sell:
                case TransactionSide.Buy:
                    return transaction.CommCurrency ?? transaction.DstAssetCurrency;
                case TransactionSide.None: return "";
                default: throw new NotSupportedException(GetTransactionSide(transaction).ToString());
            }
        }

        protected override double? GetOpenPrice(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
            {
                if (transaction.TradeRecordType == OrderType.Stop || transaction.TradeRecordType == OrderType.StopLimit)
                    return transaction.StopPrice;
            }

            return transaction.Price;
        }

        //protected override AggregatedTransactionType GetTransactionType(TradeReportEntity transaction) //after creation Splits and Dividends on Grosses make common
        //{
        //    var type = base.GetTransactionType(transaction);

        //    if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction)
        //    {
        //        if (transaction.TradeTransactionReason == TradeTransactionReason.Dividend)
        //            type = AggregatedTransactionType.Dividend;

        //        if (transaction.TradeTransactionReason == TradeTransactionReason.TransferMoney)
        //            type = AggregatedTransactionType.TransferFunds;
        //    }

        //    if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction && transaction.TradeTransactionReason == TradeTransactionReason.Split)
        //        type = AggregatedTransactionType.Split;

        //    if (transaction.TradeTransactionReportType == TradeExecActions.TradeModified)
        //    {
        //        if (transaction.TradeTransactionReason == TradeTransactionReason.Split)
        //            type = transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.SplitBuy : AggregatedTransactionType.SplitSell;
        //    }

        //    return type;
        //}

        protected override double? GetClosePrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.OrderFillPrice;
        }

        protected override double? GetCloseQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.OrderLastFillAmount / LotSize;
        }

        protected override double? GetOpenQuntity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? transaction.TransactionAmount / LotSize : base.GetOpenQuntity(transaction);
        }

        protected override void UpdateFieldsAfterSplit(TradeReportEntity transaction)
        {
            if (transaction.TradeTransactionReason != TradeTransactionReason.Split)
                return;

            ClosePrice = OpenPrice;
            CloseQuantity = OpenQuantity;
            OpenPrice = null;
            OpenQuantity = null;
        }
    }
}
