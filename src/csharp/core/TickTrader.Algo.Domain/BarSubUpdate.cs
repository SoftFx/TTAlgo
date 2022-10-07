namespace TickTrader.Algo.Domain
{
    public partial class BarSubUpdate
    {
        public bool IsUpsertAction => ChangeFlag;

        public bool IsRemoveAction => !ChangeFlag;


        public static BarSubUpdate Upsert(BarSubEntry entry) => new BarSubUpdate { Entry = entry, ChangeFlag = true };

        public static BarSubUpdate Remove(BarSubEntry entry) => new BarSubUpdate { Entry = entry, ChangeFlag = false };


        public string ToShortString() => (ChangeFlag ? $"+" : "-") + Entry.ToShortString();
    }


    public partial class BarSubEntry
    {
        public BarSubEntry(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
        {
            Symbol = symbol;
            MarketSide = marketSide;
            Timeframe = timeframe;
        }


        public string ToShortString() => $"{Symbol}.{MarketSide}.{Timeframe}";
    }
}
