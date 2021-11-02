using System;

namespace TickTrader.Algo.Api
{
    public enum AccountTypes
    {
        Gross,
        Net,
        Cash
    }

    public interface AccountDataProvider
    {
        string Id { get; }
        double Balance { get; }
        string BalanceCurrency { get; }
        double Equity { get; }
        double Margin { get; }
        double MarginLevel { get; }
        double Profit { get; }
        int Leverage { get; }
        AccountTypes Type { get; }
        OrderList Orders { get; }
        NetPositionList NetPositions { get; }
        AssetList Assets { get; }
        TradeHistory TradeHistory { get; }
        TriggerHistory TriggerHistory { get; }
        OrderList OrdersByTag(string orderTag);
        OrderList OrdersBySymbol(string symbol);
        OrderList OrdersBy(Predicate<Order> customCondition);

        /// <summary>
        /// Margin in balance currency for all orders/positions by specified symbol and side.
        /// Isolation is ignored. Null for cash accounts
        /// </summary>
        double? GetSymbolMargin(string symbol, OrderSide side);
        /// <summary>
        /// Calculates order margin based on params. If calculator fails returns null and logs exception in bot logs
        /// </summary>
        double? CalculateOrderMargin(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None);
        /// <summary>
        /// Check if account has sufficient margin to open order. If calculator fails returns false and logs exception in bot logs
        /// </summary>
        bool HasEnoughMarginToOpenOrder(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None);

        event Action BalanceUpdated;
        event Action<IBalanceDividendEventArgs> BalanceDividend;
        /// <summary>
        /// This event signals that all data is updated by fresh snapshots coming from server.
        /// Usually happens when connection to server is lost and then restored back.
        /// All order/positon/asset events lost during disconnected periods will not be fired!
        /// </summary>
        event Action Reset;
    }
}
