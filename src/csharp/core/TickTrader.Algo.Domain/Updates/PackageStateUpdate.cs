namespace TickTrader.Algo.Domain
{
    public partial class PackageStateUpdate
    {
        public PackageStateUpdate(string id, bool isLocked)
        {
            Id = id;
            IsLocked = isLocked;
        }
    }
}
