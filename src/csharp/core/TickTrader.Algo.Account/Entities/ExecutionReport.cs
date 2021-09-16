using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class ExecutionReport : IOrderUpdateInfo
    {
        public string Id { get; set; }
        public string ParentOrderId { get; set; }
        //public DateTime ExecTime { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public Domain.OrderExecReport.Types.CmdResultCode RejectReason { get; set; }
        public double? TakeProfit { get; set; }
        public double? StopLoss { get; set; }
        public double? Slippage { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
        public string UserTag { get; set; }
        public int? Magic { get; set; }
        public bool IsReducedOpenCommission { get; set; }
        public bool IsReducedCloseCommission { get; set; }
        public bool ImmediateOrCancel { get; set; }
        public bool MarketWithSlippage { get; set; }
        public string ClosePositionRequestId { get; set; }
        public double TradePrice { get; set; }
        public Domain.AssetInfo[] Assets { get; set; }
        public double? StopPrice { get; set; }
        public double? AveragePrice { get; set; }
        public string ClientOrderId { get; set; }
        public string TradeRequestId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public ExecutionType ExecutionType { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public double ExecutedVolume { get; set; }
        public double? InitialVolume { get; set; }
        public double LeavesVolume { get; set; }
        public double? MaxVisibleAmount { get; set; }
        public double? TradeAmount { get; set; }
        public double Commission { get; set; }
        public double AgentCommission { get; set; }
        public double Swap { get; set; }
        public Domain.OrderInfo.Types.Type InitialType { get; set; }
        public Domain.OrderInfo.Types.Type Type { get; set; }
        public Domain.OrderInfo.Types.Side Side { get; set; }
        public double? Price { get; set; }
        double IMarginProfitCalc.Price => Price ?? 0;
        public double Balance { get; set; }
        public double? RequestedOpenPrice { get; set; }

        public Domain.OrderOptions Options => GetOptions();

        public double RequestedAmount => InitialVolume ?? 0;

        public bool IsHidden => MaxVisibleAmount.HasValue && !(Math.Abs(MaxVisibleAmount.Value) < 1e-9);

        double IMarginProfitCalc.RemainingAmount => LeavesVolume;

        Timestamp IOrderUpdateInfo.Created => Created?.ToTimestamp();

        Timestamp IOrderUpdateInfo.Expiration => Expiration?.ToTimestamp();

        Timestamp IOrderUpdateInfo.Modified => Modified?.ToTimestamp();

        public double? ExecPrice => AveragePrice;

        public double? ExecAmount => ExecutedVolume;

        public double? LastFillPrice => TradePrice;

        public double? LastFillAmount => TradeAmount;

        public string InstanceId => null;

        private Domain.OrderOptions GetOptions()
        {
            Domain.OrderOptions options = Domain.OrderOptions.None;

            if (ImmediateOrCancel)
                options |= Domain.OrderOptions.ImmediateOrCancel;

            if (MarketWithSlippage)
                options |= Domain.OrderOptions.MarketWithSlippage;

            if (IsHidden)
                options |= Domain.OrderOptions.HiddenIceberg;

            if (IsOneCancelsTheOther)
                options |= Domain.OrderOptions.OneCancelsTheOther;

            if (IsContingentOrder)
                options |= Domain.OrderOptions.ContingentOrder;

            return options;
        }

        public bool IsOneCancelsTheOther { get; set; }

        public string OcoRelatedOrderId { get; set; }

        public bool IsContingentOrder { get; set; }

        public Domain.ContingentOrderTrigger OtoTrigger { get; set; }
    }

    public enum ExecutionType
    {
        None = -1,
        New = 0,
        Trade = 1,
        Expired = 2,
        Canceled = 3,
        PendingCancel = 4,
        Rejected = 5,
        Calculated = 6,
        PendingReplace = 7,
        Replace = 8,
        OrderStatus = 9,
        PendingClose = 10,
        Split = 11
    }

    public enum OrderStatus
    {
        None = -1,
        New = 0,
        Calculated = 1,
        Filled = 2,
        PartiallyFilled = 3,
        Canceled = 4,
        PendingCancel = 5,
        Rejected = 6,
        Expired = 7,
        PendingReplace = 8,
        Done = 9,
        PendingClose = 10,
        Activated = 11,
        Executing = 12,
    }
}
