using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TradeReportEntity
    {
        public string OrderId { get; set; }
        public string PositionId { get; set; }
        public string PositionById { get; set; }
        public TradeRecordTypes Type { get; set; }
        public TradeExecActions ActionType { get; set; }
        public string Symbol { get; set; }
        public double OpenQuantity { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public double PositionCloseQuantity { get; set; }
        public double PositionClosePrice { get; set; }
        public double RemainingQuantity { get; set; }
        public double Commission { get; set; }
        public string CommissionCurrency { get; set; }
        public double Swap { get; set; }
        public string Comment { get; set; }
        public double? OrderLastFillAmount { get; set; }
        public double? OrderFillPrice { get; set; }
        public DateTime TransactionTime { get; set; }
        public double? PositionRemainingPrice { get; set; }
        public string SrcAssetCurrency { get; set; }
        public string DstAssetCurrency { get; set; }
        public double? SrcAssetMovement { get; set; }
        public double? DstAssetMovement { get; set; }
        public double PositionLeavesQuantity { get; set; }
        public Domain.OrderInfo.Types.Side OrderSide { get; set; }
        public Domain.OrderInfo.Types.Type OrderType { get; set; }
        public double StopPrice { get; set; }
        public double Price { get; set; }
        public double? MaxVisibleQuantity { get; set; }
        public string Id { get; set; }
        public string TransactionCurrency { get; set; }
        public double TransactionAmount { get; set; }
        public double AccountBalance { get; set; }
        public TradeTransactionReason TradeTransactionReason { get; set; }
        public string Tag { get; set; }
        public double PositionQuantity { get; set; }
        public double PositionOpenPrice { get; set; }
        public Domain.OrderInfo.Types.Type RequestedOrderType { get; set; }
        public double? SplitRatio { get; set; }
        public double Tax { get; set; }
        public double? Slippage { get; set; }
        public bool IsEmulatedEntity { get; set; }
        public bool MarketWithSlippage { get; set; }
        public bool ImmediateOrCancel { get; set; }
        public double? RequestedCloseQuantity { get; set; }
        public double? RequestedClosePrice { get; set; }
        public double? RequestedOpenQuantity { get; set; }
        public double? RequestedOpenPrice { get; set; }
        public bool OneCancelsTheOther { get; set; }
        public string OCORelativeOrderId { get; set; }

        public int ActionId { get; set; }

        #region Aliases

        public double OpenPrice => Price;
        public DateTime OpenTime => OrderOpened;
        public DateTime CloseTime => TransactionTime;
        public DateTime ReportTime => TransactionTime;
        public double GrossProfitLoss => TransactionAmount - Swap - Commission;
        public double Balance => AccountBalance;
        public double NetProfitLoss => TransactionAmount;
        public TradeExecActions TradeTransactionReportType => ActionType;
        public double Quantity => OpenQuantity;
        #endregion

        #region Unused fields

        public Domain.OrderInfo.Types.Side PositionRemainingSide { get; set; }
        public DateTime PositionModified { get; set; }
        public DateTime? Expiration { get; set; }
        public string ProfitCurrency { get; set; }
        public string MarginCurrency { get; set; }
        public double? DstAssetAmount { get; set; }
        public double? SrcAssetAmount { get; set; }
        public DateTime PositionClosed { get; set; }
        public DateTime PositionOpened { get; set; }
        public DateTime OrderModified { get; set; }
        public DateTime OrderOpened { get; set; }

        #endregion
    }

    public enum TradeTransactionReason
    {
        None = -1,
        ClientRequest = 0,
        PendingOrderActivation = 1,
        StopOut = 2,
        StopLossActivation = 3,
        TakeProfitActivation = 4,
        DealerDecision = 5,
        Rollover = 6,
        DeleteAccount = 7,
        Expired = 8,
        TransferMoney = 9,
        Split = 10,
        Dividend = 11,
        OneCancelsTheOther = 12,
    }
}
