namespace TickTrader.Algo.Domain
{
    public partial class BarSubUpdate
    {
        public bool IsUpsertAction => Entries.Count > 0;

        public bool IsRemoveAction => Entries.Count == 0;
    }
}
