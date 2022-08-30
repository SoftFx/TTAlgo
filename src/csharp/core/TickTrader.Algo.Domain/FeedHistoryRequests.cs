namespace TickTrader.Algo.Domain
{
    public partial class BarListRequest
    {
        public UtcTicks From
        {
            get => new UtcTicks(FromTicks);
            set => FromTicks = value.Value;
        }

        public UtcTicks To
        {
            get => new UtcTicks(ToTicks);
            set => ToTicks = value.Value;
        }
    }

    public partial class QuoteListRequest
    {
        public UtcTicks From
        {
            get => new UtcTicks(FromTicks);
            set => FromTicks = value.Value;
        }

        public UtcTicks To
        {
            get => new UtcTicks(ToTicks);
            set => ToTicks = value.Value;
        }
    }
}
