namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class RemovePackageRequest
    {
        public RemovePackageRequest(string packageId)
        {
            PackageId = packageId;
        }
    }
}
