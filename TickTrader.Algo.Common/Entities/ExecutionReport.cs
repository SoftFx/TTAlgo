using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class ExecutionReport
    {
        public string OrderId { get; set; }
        public string ParentOrderId { get; set; }
        //public DateTime ExecTime { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public OrderCmdResultCodes RejectReason { get; set; }
        public double? TakeProfit { get; set; }
        public double? StopLoss { get; set; }
        public double? Slippage { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
        public string Tag { get; set; }
        public int? Magic { get; set; }
        public bool IsReducedOpenCommission { get; set; }
        public bool IsReducedCloseCommission { get; set; }
        public bool ImmediateOrCancel { get; set; }
        public bool MarketWithSlippage { get; set; }
        public string ClosePositionRequestId { get; set; }
        public double TradePrice { get; set; }
        public AssetEntity[] Assets { get; set; }
        public double? StopPrice { get; set; }
        public double? AveragePrice { get; set; }
        public string ClientOrderId { get; set; }
        public string TradeRequestId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public ExecutionType ExecutionType { get; set; }
        public string Symbol { get; set; }
        public double ExecutedVolume { get; set; }
        public double? InitialVolume { get; set; }
        public double LeavesVolume { get; set; }
        public double? MaxVisibleVolume { get; set; }
        public double? TradeAmount { get; set; }
        public double Commission { get; set; }
        public double AgentCommission { get; set; }
        public double Swap { get; set; }
        public OrderType InitialOrderType { get; set; }
        public OrderType OrderType { get; set; }
        public OrderSide OrderSide { get; set; }
        public double? Price { get; set; }
        public double Balance { get; set; }
        public double? ReqOpenPrice { get; set; }
        public bool IsOneCancelsTheOther { get; set; }
        public string OCORelatedOrderId { get; set; }
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
