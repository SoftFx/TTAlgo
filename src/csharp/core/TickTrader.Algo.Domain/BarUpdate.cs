namespace TickTrader.Algo.Domain
{
    public partial class BarUpdate
    {
        public UtcTicks OpenTime => (BidData ?? AskData).OpenTime;

        public UtcTicks CloseTime => (BidData ?? AskData).CloseTime;
    }
}
