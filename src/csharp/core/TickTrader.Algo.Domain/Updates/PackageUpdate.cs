namespace TickTrader.Algo.Domain
{
    public partial class PackageUpdate
    {
        public static PackageUpdate Added(string id, PackageInfo package)
        {
            return new PackageUpdate { Action = Update.Types.Action.Added, Id = id, Package = package };
        }

        public static PackageUpdate Updated(string id, PackageInfo package)
        {
            return new PackageUpdate { Action = Update.Types.Action.Updated, Id = id, Package = package };
        }

        public static PackageUpdate Removed(string id)
        {
            return new PackageUpdate { Action = Update.Types.Action.Removed, Id = id };
        }
    }
}
