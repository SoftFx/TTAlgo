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
        private static IndificationNumberGenerator _numberGenerator = new IndificationNumberGenerator();

        public enum AggregatedTransactionType
        {
            Unknown, Buy, BuyLimit, BuyStop, Deposit, Sell, SellLimit, SellStop, Withdrawal, BuyStopLimit, SellStopLimit, SellStopLimitCanceled,
            SellStopCanceled, SellLimitCanceled, BuyStopLimitCanceled, BuyStopCanceled, BuyLimitCanceled, TransferFunds, SplitBuy, SplitSell, Dividend
        }

        public enum TransactionSide { None = -1, Buy, Sell }

        public enum Reasons { None = -1, DealerDecision, StopOut, Activated, CanceledByDealer, Expired }

        public enum OrderExecutionOptions { None = -1, IoC, MarketWithSlippage, HiddenIceberg }

        public TransactionReport() { }

        public TransactionReport(TradeReportEntity transaction, SymbolModel symbol)
        {
            PriceDigits = symbol?.PriceDigits ?? 5;
            ProfitDigits = symbol?.QuoteCurrencyDigits ?? 2;
            LotSize = symbol?.LotSize ?? 1;

            IsPosition = transaction.TradeRecordType == OrderType.Position;
            IsMarket = transaction.TradeRecordType == OrderType.Market;
            IsPending = transaction.TradeRecordType == OrderType.Limit
                || transaction.TradeRecordType == OrderType.Stop
                || transaction.TradeRecordType == OrderType.StopLimit;
            IsBalanceTransaction = transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction;

            OrderId = GetId(transaction);
            ActionId = transaction.ActionId;
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
            Slippage = GetSlippage(transaction);
            Tag = GetTag(transaction);
            PosQuantity = GetPosQuantity(transaction);
            SortedNumber = GetSortedNumber();

            // should be last (it's based on other fields)
            UniqueId = GetUniqueId(transaction, out long orderNum);
            OrderNum = orderNum;

            if (IsSplitTransaction)
            {
                SplitRatio = transaction.SplitRatio;
                SplitReqPrice = OpenPrice * SplitRatio;
                SplitReqVolume = Volume / SplitRatio;
            }
        }

        public static TransactionReport Create(AccountTypes accountType, TradeReportEntity tTransaction, SymbolModel symbol = null)
        {
            switch (accountType)
            {
                case AccountTypes.Gross: return new GrossTransactionModel(tTransaction, symbol);
                case AccountTypes.Net: return new NetTransactionModel(tTransaction, symbol);
                case AccountTypes.Cash: return new CashTransactionModel(tTransaction, symbol);
                default: throw new NotSupportedException(accountType.ToString());
            }
        }

        private double? GetMaxVisibleVolume(TradeReportEntity transaction)
        {
            return transaction.MaxVisibleQuantity;
        }

        public TradeReportKey UniqueId { get; protected set; }
        public string OrderId { get; protected set; }
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
        public double GrossProfitLoss { get; protected set; }
        public double NetProfitLoss { get; protected set; }
        public bool IsPosition { get; protected set; }
        public bool IsMarket { get; protected set; }
        public bool IsPending { get; protected set; }
        public bool IsBalanceTransaction { get; protected set; }
        public double? MaxVisibleVolume { get; protected set; }
        public double LotSize { get; }
        public double? Volume { get; protected set; }
        public double? Slippage { get; protected set; }
        public double? ReqQuantity { get; protected set; }
        public double? PosRemainingPrice { get; protected set; }
        public string OrderExecutionOption { get; protected set; }
        public OrderType? InitialType { get; protected set; }
        public Reasons? Reason { get; protected set; }
        public string Tag { get; protected set; }
        public double? PosQuantity { get; protected set; }
        public string SortedNumber { get; protected set; }

        public bool IsSplitTransaction => Type == AggregatedTransactionType.SplitBuy || Type == AggregatedTransactionType.SplitSell;
        public bool IsNotSplitTransaction => !IsSplitTransaction;
        public double? SplitReqVolume { get; protected set; }
        public double? SplitReqPrice { get; protected set; }
        public double? SplitRatio { get; protected set; }

        protected virtual AggregatedTransactionType GetTransactionType(TradeReportEntity transaction)
        {
            if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction)
                return transaction.TransactionAmount > 0 ? AggregatedTransactionType.Deposit : AggregatedTransactionType.Withdrawal;

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
                default: return AggregatedTransactionType.Unknown;
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

        protected virtual TradeReportKey GetUniqueId(TradeReportEntity transaction, out long orderNum)
        {
            orderNum = long.Parse(transaction.OrderId);

            if (transaction.ActionId > 1)
                return new TradeReportKey(orderNum, transaction.ActionId);

            if (ActionId == 1 && RemainingQuantity > 0 && !OrderWasCanceled() && Reason != Reasons.Activated)
                return new TradeReportKey(orderNum, transaction.ActionId);
            else
                return new TradeReportKey(orderNum, null);
        }

        protected virtual string GetId(TradeReportEntity transaction)
        {
            return transaction.Id;
        }

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
            return transaction.OrderCreated;
        }

        protected virtual double GetNetProfiLoss(TradeReportEntity transaction)
        {
            return transaction.TransactionAmount;
        }

        protected virtual double GetGrossProfitLoss(TradeReportEntity transaction)
        {
            return transaction.TransactionAmount - transaction.Swap - transaction.Commission;
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
                return transaction.TransactionAmount;

            if (IsSplitTransaction)
                return (transaction.Quantity == 0 ? transaction.PositionQuantity : transaction.Quantity) / LotSize;

            return (OrderWasCanceled() ? transaction.RemainingQuantity : transaction.OrderLastFillAmount) / LotSize;
        }

        protected virtual double? GetSlippage(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction || IsSplitTransaction)
                return null;

            return GetTransactionSide(transaction) == TransactionSide.Buy ? OpenPrice - transaction.ReqOpenPrice : transaction.ReqOpenPrice - OpenPrice;
        }

        protected virtual double? GetReqQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : ((transaction.RemainingQuantity + transaction.OrderLastFillAmount) / LotSize);
        }

        protected virtual double? GetPosRemainingPrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : transaction.PosRemainingPrice;
        }

        protected virtual string GetOrderExecutionOption(TradeReportEntity transaction)
        {
            List<OrderExecutionOptions> options = new List<OrderExecutionOptions>();

            if (transaction.ImmediateOrCancel)
            {
                Type = Type == AggregatedTransactionType.BuyLimit ? AggregatedTransactionType.Buy : AggregatedTransactionType.Sell;
                options.Add(OrderExecutionOptions.IoC);
            }

            if (transaction.MarketWithSlippage)
                options.Add(OrderExecutionOptions.MarketWithSlippage);

            if (transaction.MaxVisibleQuantity >= 0)
                options.Add(OrderExecutionOptions.HiddenIceberg);

            return string.Join(",", options);
        }

        protected virtual OrderType? GetInitialOrderType(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? null : (OrderType?)transaction.ReqOrderType;
        }

        protected virtual Reasons? GetReason(TradeReportEntity transaction)
        {
            if (transaction.TradeTransactionReportType == TradeExecActions.OrderFilled)
                Type = GetBuyOrSellType(transaction);

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderFilled && transaction.TradeTransactionReason == TradeTransactionReason.DealerDecision)
                return Reasons.DealerDecision;

            if (transaction.TradeTransactionReportType == TradeExecActions.OrderFilled && transaction.TradeTransactionReason == TradeTransactionReason.StopOut)
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

        protected AggregatedTransactionType GetCanceledType(TradeReportEntity transaction)
        {
            switch (transaction.TradeRecordType)
            {
                case OrderType.Market:
                case OrderType.Position:
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

        protected string GetTag(TradeReportEntity transaction)
        {
            return CompositeTag.TryParse(transaction.Tag, out CompositeTag tag) ? tag?.Tag : transaction.Tag;
        }

        protected string GetSortedNumber()
        {
            return $"{CloseTime.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{_numberGenerator.GetNumber(CloseTime)}";
        }

        private bool OrderWasCanceled() => Type.ToString().Contains("Canceled");
    }

    class NetTransactionModel : TransactionReport
    {
        public NetTransactionModel(TradeReportEntity transaction, SymbolModel model) : base(transaction, model) { }

        protected override double? GetOpenPrice(TradeReportEntity transaction)
        {
            if (IsBalanceTransaction)
                return null;

            if (transaction.TradeRecordType == OrderType.Stop)
                return transaction.OrderFillPrice;

            if (transaction.TradeRecordType == OrderType.StopLimit)
                return transaction.StopPrice;

            if (IsSplitTransaction)
                return transaction.PosRemainingPrice ?? transaction.Price;

            return transaction.PosOpenPrice == 0 ? transaction.Price : transaction.PosOpenPrice;
        }

        protected override AggregatedTransactionType GetTransactionType(TradeReportEntity transaction)
        {
            var type = base.GetTransactionType(transaction);

            if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction)
            {
                if (transaction.TradeTransactionReason == TradeTransactionReason.Dividend)
                    type = AggregatedTransactionType.Dividend;

                if (transaction.TradeTransactionReason == TradeTransactionReason.TransferMoney)
                    type = AggregatedTransactionType.TransferFunds;
            }

            if (transaction.TradeTransactionReportType == TradeExecActions.TradeModified)
            {
                if (transaction.TradeTransactionReason == TradeTransactionReason.Split)
                    type = transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.SplitBuy : AggregatedTransactionType.SplitSell;
            }

            return type;
        }
    }

    class GrossTransactionModel : TransactionReport
    {
        public GrossTransactionModel(TradeReportEntity transaction, SymbolModel symbol) : base(transaction, symbol)
        {
        }

        protected override DateTime GetOpenTime(TradeReportEntity transaction)
        {
            return IsPosition ? transaction.PositionOpened : transaction.OrderCreated;
        }

        protected override double? GetOpenQuntity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : IsPosition ? (transaction.PositionQuantity / LotSize) : (transaction.Quantity / LotSize);
        }

        protected override double? GetOpenPrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null :
                transaction.TradeRecordType == OrderType.Stop || transaction.TradeRecordType == OrderType.StopLimit ? transaction.StopPrice :
                IsPosition ? transaction.PosOpenPrice : transaction.Price;
        }

        protected override double? GetRemainingQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : IsPosition ? (transaction.PositionLeavesQuantity / LotSize) : (transaction.LeavesQuantity / LotSize);
        }
    }

    class CashTransactionModel : TransactionReport
    {
        public CashTransactionModel(TradeReportEntity transaction, SymbolModel symbol) : base(transaction, symbol)
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

        protected override AggregatedTransactionType GetTransactionType(TradeReportEntity transaction) //after creation Splits and Dividends on Grosses make common
        {
            var type = base.GetTransactionType(transaction);

            if (transaction.TradeTransactionReportType == TradeExecActions.BalanceTransaction)
            {
                if (transaction.TradeTransactionReason == TradeTransactionReason.Dividend)
                    type = AggregatedTransactionType.Dividend;

                if (transaction.TradeTransactionReason == TradeTransactionReason.TransferMoney)
                    type = AggregatedTransactionType.TransferFunds;
            }

            if (transaction.TradeTransactionReason == TradeTransactionReason.Split)
                type = transaction.TradeRecordSide == OrderSide.Buy ? AggregatedTransactionType.SplitBuy : AggregatedTransactionType.SplitSell;

            return type;
        }

        protected override double? GetClosePrice(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.OrderFillPrice;
        }

        protected override double? GetCloseQuantity(TradeReportEntity transaction)
        {
            return IsBalanceTransaction ? (double?)null : transaction.OrderLastFillAmount;
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
