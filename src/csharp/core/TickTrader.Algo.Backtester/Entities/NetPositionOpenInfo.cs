using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
{
    internal class NetPositionOpenInfo
    {
        public TradeChargesInfo Charges { get; set; }
        public PositionAccessor ResultingPosition { get; set; }
        public NetPositionCloseInfo CloseInfo { get; set; }
    }

    internal class NetPositionCloseInfo
    {
        public decimal CloseAmount { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal BalanceMovement { get; set; }
    }
}
