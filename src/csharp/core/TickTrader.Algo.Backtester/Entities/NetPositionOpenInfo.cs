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
        public double CloseAmount { get; set; }
        public double ClosePrice { get; set; }
        public double Profit { get; set; }
        public double Swap { get; set; }
    }
}
