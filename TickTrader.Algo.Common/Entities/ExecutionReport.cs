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
        public string OrderId { get; }
        public DateTime? Expiration { get; }
        public DateTime? Created { get; }
        public DateTime? Modified { get; }
        public OrderCmdResultCodes RejectReason { get; }
        public double? TakeProfit { get; }
        public double? StopLoss { get; }
        public string Text { get; }
        public string Comment { get; }
        public string Tag { get; }
        public int? Magic { get; }
        public bool IsReducedOpenCommission { get; set; }
        public bool IsReducedCloseCommission { get; set; }
        public bool ImmediateOrCancel { get; }
        public bool MarketWithSlippage { get; }
        public string ClosePositionRequestId { get; }
        public double TradePrice { get; }
        public AssetEntity[] Assets { get; }
        public double? StopPrice { get; }
        public double? AveragePrice { get; }
        public string ClientOrderId { get; }
        public string TradeRequestId { get; }
        public OrderStatus OrderStatus { get; }
        public ExecutionType ExecutionType { get; }
        public string Symbol { get; }
        public double ExecutedVolume { get; }
        public double? InitialVolume { get; }
        public double LeavesVolume { get; }
        public double? MaxVisibleVolume { get; }
        public double? TradeAmount { get; }
        public double Commission { get; }
        public double AgentCommission { get; }
        public double Swap { get; }
        public OrderType OrderType { get; }
        public OrderSide OrderSide { get; }
        public double? Price { get; }
        public double Balance { get; }
    }

    public enum ExecutionType
    {
        None,
        Opened,
        Modified,
        Expired,
        Canceled,
        Rejected,
        Trade
    }

    public enum OrderStatus
    {
        Calculated,
        Filled,
        Rejected
    }
}
