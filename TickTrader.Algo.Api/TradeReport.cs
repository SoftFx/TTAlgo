using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface TradeReport
    {
        string ReportId { get; }
        string OrderId { get; }
        string PositionId { get; }
        string PositionById { get; }
        DateTime ReportTime { get; }
        DateTime OpenTime { get; }
        TradeRecordTypes Type { get; }
        TradeExecActions ActionType { get; }
        string Symbol { get; }
        double OpenQuantity { get; }
        double OpenPrice { get; }
        double StopLoss { get; }
        double TakeProfit { get; }
        DateTime CloseTime { get; }
        double CloseQuantity { get; }
        double ClosePrice { get; }
        double RemainingQuantity { get; }
        double Commission { get; }
        string CommissionCurrency { get; }
        double Swap { get; }
        double Balance { get; }
        string Comment { get; }
        double GrossProfitLoss { get; }
        double NetProfitLoss { get; }
        OrderSide TradeRecordSide { get; }
        OrderType TradeRecordType { get; }
        double? MaxVisibleQuantity { get; }
        string Tag { get; }
        double? Slippage { get; }
        double? ReqCloseQuantity { get; }
        double? ReqClosePrice { get; }
        double? ReqOpenQuantity { get; }
        double? ReqOpenPrice { get; }
        bool ImmediateOrCancel { get; }
    }

    public enum TradeRecordTypes
    {
        Unknown,
        Buy,
        BuyLimit,
        BuyStop,
        Deposit,
        Withdrawal,
        Sell,
        SellLimit,
        SellStop
    }

    public enum TradeExecActions
    {
        None = -1,
        OrderOpened = 0,
        OrderCanceled = 1,
        OrderExpired = 2,
        OrderFilled = 3,
        PositionClosed = 4,
        BalanceTransaction = 5,
        Credit = 6,
        PositionOpened = 7,
        OrderActivated = 8,
        TradeModified = 9
    }
}
