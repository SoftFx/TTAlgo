using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

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

        public TransactionReport(TradeReportInfo transaction, SymbolInfo symbol, int profitDigits)
        {
            PriceDigits = symbol?.Digits ?? -1;
            ProfitDigits = profitDigits;
            LotSize = symbol?.LotSize ?? 1;

            IsPosition = transaction.OrderType == OrderInfo.Types.Type.Position;
            IsMarket = transaction.OrderType == OrderInfo.Types.Type.Market;
            IsPending = transaction.OrderType == OrderInfo.Types.Type.Limit || transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.Stop || transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.StopLimit;
            IsBalanceTransaction = transaction.ReportType == TradeReportInfo.Types.ReportType.BalanceTransaction;

            OrderId = GetId(transaction);
            PositionId = GetPositionId(transaction);
            ActionId = transaction.ActionId;
            OpenTime = GetOpenTime(transaction);
            Type = GetTransactionType(transaction);

            if (IsSplitTransaction)
                SplitRatio = GetSplitRatio(transaction);

            Side = GetTransactionSide(transaction);
            ActionType = transaction.ReportType;
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

        public static TransactionReport Create(AccountInfo.Types.Type accountType, TradeReportInfo tTransaction, int balanceDigits, SymbolInfo symbol = null)
        {
            switch (accountType)
            {
                case AccountInfo.Types.Type.Gross: return new GrossTransactionModel(tTransaction, symbol, balanceDigits);
                case AccountInfo.Types.Type.Net: return new NetTransactionModel(tTransaction, symbol, balanceDigits);
                case AccountInfo.Types.Type.Cash: return new CashTransactionModel(tTransaction, symbol, balanceDigits);
                default: throw new NotSupportedException(accountType.ToString());
            }
        }

        private double? GetMaxVisibleVolume(TradeReportInfo transaction)
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
        public TradeReportInfo.Types.ReportType ActionType { get; protected set; }
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

        protected virtual AggregatedTransactionType GetTransactionType(TradeReportInfo transaction)
        {
            if (transaction.ReportType == TradeReportInfo.Types.ReportType.BalanceTransaction)
            {
                switch (transaction.TransactionReason)
                {
                    case TradeReportInfo.Types.Reason.Dividend:
                        return AggregatedTransactionType.Dividend;

                    case TradeReportInfo.Types.Reason.TransferMoney:
                        return AggregatedTransactionType.TransferFunds;

                    case TradeReportInfo.Types.Reason.Split:
                        return AggregatedTransactionType.Split;

                    default:
                        return transaction.TransactionAmount > 0 ? AggregatedTransactionType.Deposit : AggregatedTransactionType.Withdrawal;
                }
            }

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.TradeModified)
            {
                if (transaction.TransactionReason == TradeReportInfo.Types.Reason.Split)
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.SplitBuy : AggregatedTransactionType.SplitSell;
            }

            switch (transaction.OrderType)
            {
                case OrderInfo.Types.Type.Market:
                case OrderInfo.Types.Type.Position:
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
                case OrderInfo.Types.Type.Limit:
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.BuyLimit : AggregatedTransactionType.SellLimit;
                case OrderInfo.Types.Type.StopLimit:
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.BuyStopLimit : AggregatedTransactionType.SellStopLimit;
                case OrderInfo.Types.Type.Stop:
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.BuyStop : AggregatedTransactionType.SellStop;
                default:
                    return AggregatedTransactionType.Unknown;
            }
        }

        protected virtual string GetSymbolOrCurrency(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? transaction.TransactionCurrency : transaction.Symbol;
        }

        protected virtual TransactionSide GetTransactionSide(TradeReportInfo transaction)
        {
            switch (transaction.OrderSide)
            {
                case OrderInfo.Types.Side.Buy: return TransactionSide.Buy;
                case OrderInfo.Types.Side.Sell: return TransactionSide.Sell;
                default: return TransactionSide.None;
            }
        }

        protected virtual TradeReportKey GetUniqueId(TradeReportInfo transaction, string id, out long orderNum)
        {
            orderNum = long.Parse(id);

            if (transaction.ActionId > 1)
                return new TradeReportKey(orderNum, transaction.ActionId);

            if (ActionId == 1 && RemainingQuantity > 0 && !OrderWasCanceled() && Reason != Reasons.Activated)
                return new TradeReportKey(orderNum, transaction.ActionId);
            else
                return new TradeReportKey(orderNum, null);
        }

        protected virtual string GetId(TradeReportInfo transaction) => transaction.Id;

        protected virtual string GetPositionId(TradeReportInfo transaction) => transaction.PositionId;

        protected virtual string GetCommissionCurrency(TradeReportInfo transaction)
        {
            return transaction.CommissionCurrency ?? transaction.TransactionCurrency;
        }

        protected virtual double? GetRemainingQuantity(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : (transaction.RemainingQuantity / LotSize);
        }

        protected virtual double? GetPosQuantity(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : (transaction.PositionQuantity / LotSize);
        }

        protected virtual double? GetClosePrice(TradeReportInfo transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : transaction.PositionClosePrice;
        }

        protected virtual double? GetCloseQuantity(TradeReportInfo transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : (transaction.PositionCloseQuantity / LotSize);
        }

        protected virtual DateTime GetCloseTime(TradeReportInfo transaction)
        {
            return transaction.TransactionTime.ToDateTime();
        }

        protected virtual double? GetOpenPrice(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.Price;
        }

        protected virtual double? GetOpenQuntity(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : (transaction.Quantity / LotSize);
        }

        protected virtual DateTime GetOpenTime(TradeReportInfo transaction)
        {
            return transaction.OpenTime.ToDateTime();
        }

        protected virtual double GetNetProfiLoss(TradeReportInfo transaction)
        {
            return transaction.TransactionAmount;
        }

        protected virtual double? GetGrossProfitLoss(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : (double?)(transaction.TransactionAmount - transaction.Swap - transaction.Commission);
        }

        protected virtual double? GetSwap(TradeReportInfo transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : transaction.Swap;
        }

        protected virtual double? GetCommission(TradeReportInfo transaction)
        {
            return (IsBalanceTransaction | IsSplitTransaction) ? (double?)null : transaction.Commission;
        }

        protected virtual double? GetStopLoss(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.StopLoss;
        }

        protected virtual double? GetTakeProfit(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.TakeProfit;
        }

        protected virtual double? GetVolume(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return transaction.TransactionAmount / LotSize;

            if (IsSplitTransaction)
                return (transaction.Quantity == 0 ? transaction.PositionQuantity : transaction.Quantity) / LotSize;

            return (OrderWasCanceled() ? transaction.RemainingQuantity : transaction.OrderLastFillAmount) / LotSize;
        }

        protected virtual double? GetReqQuantity(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : ((transaction.RemainingQuantity + transaction.OrderLastFillAmount) / LotSize);
        }

        protected virtual double? GetPosRemainingPrice(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : transaction.PositionRemainingPrice;
        }

        protected virtual double? GetFees(TradeReportInfo transaction)
        {
            return IsDividendTransaction ? (double?)transaction.Commission : null;
        }

        protected virtual double? GetTaxes(TradeReportInfo transaction)
        {
            return IsDividendTransaction ? (double?)transaction.Tax : null;
        }

        protected virtual string GetOrderExecutionOption(TradeReportInfo transaction)
        {
            return transaction.OrderOptions.ToString();
        }

        protected virtual OrderType? GetInitialOrderType(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : (OrderType?)transaction.RequestedOrderType;
        }

        protected virtual double? GetReqOpenPrice(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : transaction.RequestedOpenPrice;
        }

        protected virtual double? GetReqClosePrice(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : transaction.RequestedClosePrice;
        }

        protected virtual double? GetReqOpenVolume(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : transaction.RequestedOpenQuantity / LotSize;
        }

        protected virtual double? GetReqCloseVolume(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : transaction.RequestedCloseQuantity / LotSize;
        }

        protected virtual double? GetOpenSlippage(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return GetTransactionSide(transaction) == TransactionSide.Buy ? OpenPrice - ReqOpenPrice : ReqOpenPrice - OpenPrice;
        }

        protected virtual double? GetCloseSlippage(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return GetTransactionSide(transaction) == TransactionSide.Buy ? ClosePrice - ReqClosePrice : ReqClosePrice - ClosePrice;
        }

        protected virtual double? GetReqSlippage(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return transaction.Slippage;
        }

        protected virtual Reasons? GetReason(TradeReportInfo transaction)
        {
            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderFilled)
                Type = GetBuyOrSellType(transaction);

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderFilled && transaction.TransactionReason == TradeReportInfo.Types.Reason.DealerDecision)
                return Reasons.DealerDecision;

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderFilled && transaction.TransactionReason == TradeReportInfo.Types.Reason.StopOut)
                return Reasons.StopOut;

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.PositionClosed && transaction.TransactionReason == TradeReportInfo.Types.Reason.StopOut)
                return Reasons.StopOut;

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderActivated && transaction.TransactionReason == TradeReportInfo.Types.Reason.DealerDecision &&
                transaction.RequestedOrderType == Algo.Domain.OrderInfo.Types.Type.StopLimit)
            {
                Type = Type == AggregatedTransactionType.Sell ? AggregatedTransactionType.SellStopLimit : AggregatedTransactionType.BuyStopLimit;
                return Reasons.Activated;
            }

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderCanceled && transaction.TransactionReason == TradeReportInfo.Types.Reason.ClientRequest)
                Type = GetCanceledType(transaction);

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderCanceled && transaction.TransactionReason == TradeReportInfo.Types.Reason.DealerDecision)
            {
                Type = GetCanceledType(transaction);
                return Reasons.CanceledByDealer;
            }

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderCanceled && transaction.TransactionReason == TradeReportInfo.Types.Reason.OneCancelsTheOther)
            {
                Type = GetCanceledType(transaction);
                return Reasons.OcoRelatedOrder;
            }

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderCanceled && transaction.TransactionReason == TradeReportInfo.Types.Reason.StopOut)
            {
                Type = GetCanceledType(transaction);
                return Reasons.StopOut;
            }

            if (transaction.ReportType == TradeReportInfo.Types.ReportType.OrderExpired && transaction.TransactionReason == TradeReportInfo.Types.Reason.Expired)
            {
                Type = GetCanceledType(transaction);
                return Reasons.Expired;
            }

            return null;
        }

        protected virtual void UpdateFieldsAfterSplit(TradeReportInfo transaction) { }

        protected AggregatedTransactionType GetBuyOrSellType(TradeReportInfo transaction)
        {
            return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
        }

        protected virtual string GetOCORelatedOrderId(TradeReportInfo transaction)
        {
            return transaction.OcoRelatedOrderId;
        }

        protected AggregatedTransactionType GetCanceledType(TradeReportInfo transaction)
        {
            switch (transaction.OrderType)
            {
                case OrderInfo.Types.Type.Market:
                case OrderInfo.Types.Type.Position:
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
                case OrderInfo.Types.Type.Limit:
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.BuyLimitCanceled : AggregatedTransactionType.SellLimitCanceled;
                case OrderInfo.Types.Type.StopLimit:
                    OpenPrice = transaction.StopPrice;
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.BuyStopLimitCanceled : AggregatedTransactionType.SellStopLimitCanceled;
                case OrderInfo.Types.Type.Stop:
                    OpenPrice = transaction.StopPrice;
                    return transaction.OrderSide == OrderInfo.Types.Side.Buy ? AggregatedTransactionType.BuyStopCanceled : AggregatedTransactionType.SellStopCanceled;
                default: return AggregatedTransactionType.Unknown;
            }
        }

        protected string GetTag(TradeReportInfo transaction) => transaction.Tag;

        protected string GetInstanceId(TradeReportInfo transaction) => transaction.InstanceId;

        protected virtual double? GetSplitRatio(TradeReportInfo transaction) => transaction.SplitRatio;

        protected virtual double? GetSplitReqPrice(double? price) => price * SplitRatio;

        protected virtual double? GetSplitReqVolume(double? volume) => volume / SplitRatio;


        protected string GetOpenSortedNumber() => $"{OpenTime.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{UniqueId}";

        protected string GetCloseSortedNumber() => $"{CloseTime.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{UniqueId}";

        private bool OrderWasCanceled() => Type.ToString().Contains("Canceled");
    }

    class NetTransactionModel : TransactionReport
    {
        public NetTransactionModel(TradeReportInfo transaction, SymbolInfo model, int profitDigits) : base(transaction, model, profitDigits) { }

        protected override double? GetOpenPrice(TradeReportInfo transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (IsSplitTransaction)
            {
                if (transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.Stop || transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.StopLimit)
                    return transaction.StopPrice;
                else
                    return transaction.PositionRemainingPrice ?? transaction.Price;
            }

            if (transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.Stop)
                return transaction.OrderFillPrice;

            if (transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.StopLimit)
                return transaction.StopPrice;

            return transaction.PositionOpenPrice == 0 ? transaction.Price : transaction.PositionOpenPrice;
        }

        //protected override AggregatedTransactionType GetTransactionType(TradeReportInfo transaction)
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

        public string ParentOrderId { get; protected set; }

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

            return transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.Stop || transaction.OrderType == Algo.Domain.OrderInfo.Types.Type.StopLimit ? transaction.StopPrice : transaction.Price;
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

    class CashTransactionModel : TransactionReport
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

        //protected override AggregatedTransactionType GetTransactionType(TradeReportInfo transaction) //after creation Splits and Dividends on Grosses make common
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
