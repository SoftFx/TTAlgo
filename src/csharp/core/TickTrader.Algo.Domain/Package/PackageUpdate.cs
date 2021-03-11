namespace TickTrader.Algo.Domain
{
    public partial class PackageUpdate
    {
        public static PackageUpdate Upsert(string id, PackageInfo package)
        {
            return new PackageUpdate { Action = Domain.Package.Types.UpdateAction.Upsert, Id = id, Package = package };
        }

        public static PackageUpdate Remove(string id)
        {
            return new PackageUpdate { Action = Domain.Package.Types.UpdateAction.Removed, Id = id };
        }
    }
}
