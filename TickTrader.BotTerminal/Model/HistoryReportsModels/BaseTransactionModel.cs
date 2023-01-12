using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public abstract class BaseTransactionModel
    {
        public enum AggregatedTransactionType
        {
            Unknown, Buy, BuyLimit, BuyStop, Deposit, Sell, SellLimit, SellStop, Withdrawal, BuyStopLimit, SellStopLimit, SellStopLimitCanceled,
            SellStopCanceled, SellLimitCanceled, BuyStopLimitCanceled, BuyStopCanceled, BuyLimitCanceled, TransferFunds, SplitBuy, SplitSell, Split, Dividend
        }

        public enum TransactionSide { None = -1, Buy, Sell }

        public enum Reasons { None = -1, DealerDecision, StopOut, Activated, CanceledByDealer, Expired, OcoRelatedOrder }

        public enum TriggerResult { Success, Failed }

        public enum TriggerTypes { OnPendingOrderExpired, OnPendingOrderPartiallyFilled, OnTime }

        public BaseTransactionModel() { }

        protected BaseTransactionModel(ISymbolInfo symbol)
        {
            PriceDigits = symbol?.Digits ?? -1;
            LotSize = symbol?.LotSize ?? 1;
            ProfitDigits = -1;
        }

        public BaseTransactionModel(TradeReportInfo transaction, ISymbolInfo symbol, int profitDigits) : this(symbol)
        {
            ProfitDigits = profitDigits;

            IsPosition = transaction.OrderType == OrderInfo.Types.Type.Position;
            IsMarket = transaction.OrderType == OrderInfo.Types.Type.Market;
            IsPending = transaction.OrderType == OrderInfo.Types.Type.Limit || transaction.OrderType == OrderInfo.Types.Type.Stop || transaction.OrderType == OrderInfo.Types.Type.StopLimit;
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

        public static BaseTransactionModel Create<T>(AccountInfo.Types.Type accountType, T tTransaction, int balanceDigits, ISymbolInfo symbol = null)
        {
            return tTransaction switch
            {
                TriggerReportInfo triggerReport => new TriggerTransactionModel(triggerReport, symbol, accountType),

                TradeReportInfo tradeReport => accountType switch
                {
                    AccountInfo.Types.Type.Gross => new GrossTransactionModel(tradeReport, symbol, balanceDigits),
                    AccountInfo.Types.Type.Net => new NetTransactionModel(tradeReport, symbol, balanceDigits),
                    AccountInfo.Types.Type.Cash => new CashTransactionModel(tradeReport, symbol, balanceDigits),
                    _ => throw new NotSupportedException(accountType.ToString()),
                },

                _ => throw new NotSupportedException(nameof(T)),
            };
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
        public double? NetProfitLoss { get; protected set; }
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
        public OrderInfo.Types.Type? InitialType { get; protected set; }
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

        public TriggerTypes? TriggerType { get; protected set; }
        public string TriggeredByOrder { get; protected set; }
        public DateTime? TriggerTime { get; protected set; }
        public TriggerResult? TriggerState { get; protected set; }


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

            return GetOrderType(transaction.OrderType, transaction.OrderSide);
        }

        protected static AggregatedTransactionType GetOrderType(OrderInfo.Types.Type type, OrderInfo.Types.Side side)
        {
            return type switch
            {
                OrderInfo.Types.Type.Market or OrderInfo.Types.Type.Position => side.IsBuy() ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell,
                OrderInfo.Types.Type.Limit => side.IsBuy() ? AggregatedTransactionType.BuyLimit : AggregatedTransactionType.SellLimit,
                OrderInfo.Types.Type.StopLimit => side.IsBuy() ? AggregatedTransactionType.BuyStopLimit : AggregatedTransactionType.SellStopLimit,
                OrderInfo.Types.Type.Stop => side.IsBuy() ? AggregatedTransactionType.BuyStop : AggregatedTransactionType.SellStop,
                _ => AggregatedTransactionType.Unknown,
            };
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
            return transaction.OrderOptions.GetString();
        }

        protected virtual OrderInfo.Types.Type? GetInitialOrderType(TradeReportInfo transaction)
        {
            return IsBalanceTransaction ? null : (OrderInfo.Types.Type?)transaction.RequestedOrderType;
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
                transaction.RequestedOrderType == OrderInfo.Types.Type.StopLimit)
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

        protected static AggregatedTransactionType GetBuyOrSellType(TradeReportInfo transaction)
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

        protected static string GetTag(TradeReportInfo transaction) => transaction.Tag;

        protected static string GetInstanceId(TradeReportInfo transaction) => transaction.InstanceId;

        protected virtual double? GetSplitRatio(TradeReportInfo transaction) => transaction.SplitRatio;

        protected virtual double? GetSplitReqPrice(double? price) => price * SplitRatio;

        protected virtual double? GetSplitReqVolume(double? volume) => volume / SplitRatio;


        protected string GetOpenSortedNumber() => $"{OpenTime:dd.MM.yyyyHH:mm:ss.fff}-{UniqueId}";

        protected string GetCloseSortedNumber() => $"{CloseTime:dd.MM.yyyyHH:mm:ss.fff}-{UniqueId}";

        private bool OrderWasCanceled() => Type.ToString().Contains("Canceled");
    }
}
