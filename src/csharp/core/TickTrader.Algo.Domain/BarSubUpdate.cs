namespace TickTrader.Algo.Domain
{
    public partial class BarSubUpdate
    {
        public bool IsUpsertAction => ChangeFlag;

        public bool IsRemoveAction => !ChangeFlag;


        public static BarSubUpdate Upsert(BarSubEntry entry) => new BarSubUpdate { Entry = entry, ChangeFlag = true };

        public static BarSubUpdate Remove(BarSubEntry entry) => new BarSubUpdate { Entry = entry, ChangeFlag = false };
    }
}
