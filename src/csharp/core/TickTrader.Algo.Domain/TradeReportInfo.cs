using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public partial class TradeReportInfo
    {
        public OrderOptions OrderOptions
        {
            get { return (OrderOptions)OrderOptionsBitmask; }
            set { OrderOptionsBitmask = (int)value; }
        }

        public double OpenPrice => Price;
        public Timestamp OpenTime => OrderOpened;
        public Timestamp CloseTime => TransactionTime;
        public Timestamp ReportTime => TransactionTime;
        public double GrossProfitLoss => TransactionAmount - Swap - Commission;
        public double Balance => AccountBalance;
        public double NetProfitLoss => TransactionAmount;
        public double Quantity => OpenQuantity;
    }
}
